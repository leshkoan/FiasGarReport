using System.Net;
using System.Text;
using FiasGarReport.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Generation;

// В виду простоты проекта отдельный интерфейс для данного класса я намеренно не создаю
/// <summary>
/// Класс для генерации отчёта на основе HTML-шаблона. 
/// </summary>
// Допущение: Так как (теоретичеки) не планируется масштабировать данный проект, 
// то я решил генерировать отчёт простейшим методом с помощью StringBuilder.
public class ReportGenerator(IConfiguration configuration, ILogger<ReportGenerator> logger)
{
    private readonly string _reportOutputDirectory = 
        Path.Combine(Directory.GetCurrentDirectory(), configuration["Paths:ReportOutputDirectory"] ?? "reports");
    private readonly string _template = GetTemplate();

    private static string GetTemplate()
    {
        string templatePath = Path.Combine(AppContext.BaseDirectory, "Generation", "report-template.html");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Файл шаблона отчета не найден.", templatePath);
        }
        return File.ReadAllText(templatePath);
    }

    public string GenerateHtmlReport(ReportModel model)
    {
        logger.LogInformation("Генерация HTML-отчета на основе шаблона...");

        var report = new StringBuilder(_template);
        report.Replace("{{ReportDate}}", WebUtility.HtmlEncode(model.ReportDateText));

        var blocksBuilder = new StringBuilder();
        if (model.LevelBlocks == null || !model.LevelBlocks.Any())
        {
            blocksBuilder.AppendLine("<p class=\"no-data\">Нет новых адресных объектов для отображения.</p>");
        }
        else
        {
            foreach (var block in model.LevelBlocks)
            {
                blocksBuilder.AppendLine($"<h2>{WebUtility.HtmlEncode(block.LevelInfo?.Name)}</h2>");
                if (block.Objects == null || !block.Objects.Any())
                {
                    blocksBuilder.AppendLine("<p class=\"no-data\">Нет объектов для данного уровня.</p>");
                    continue;
                }

                blocksBuilder.AppendLine("<table>");
                blocksBuilder.AppendLine("<thead><tr><th>Тип объекта</th><th>Наименование</th></tr></thead>");
                blocksBuilder.AppendLine("<tbody>");
                foreach (var obj in block.Objects)
                {
                    blocksBuilder.AppendLine($"<tr><td>{WebUtility.HtmlEncode(obj.TypeName)}</td><td>{WebUtility.HtmlEncode(obj.Name)}</td></tr>");
                }
                blocksBuilder.AppendLine("</tbody></table>");
            }
        }
        
        report.Replace("<!-- {{LevelBlocks}} -->", blocksBuilder.ToString());
        
        logger.LogInformation("HTML-отчет сгенерирован.");
        return report.ToString();
    }

    public string SaveReport(string reportContent, string reportDateText)
    {
        EnsureOutputDirectoryExists();
        var fileName = $"FiasGarReport_{reportDateText.Replace(".", "-")}_{DateTime.Now:yyyyMMddHHmmss}.html";
        var filePath = Path.Combine(_reportOutputDirectory, fileName);

        File.WriteAllText(filePath, reportContent, Encoding.UTF8);

        logger.LogInformation("Отчет сохранен в: {FilePath}", filePath);
        return filePath;
    }

    private void EnsureOutputDirectoryExists()
    {
        if (!Directory.Exists(_reportOutputDirectory))
        {
            Directory.CreateDirectory(_reportOutputDirectory);
        }
    }
}
