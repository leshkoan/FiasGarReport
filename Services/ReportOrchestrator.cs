using FiasGarReport.Models;
using FiasGarReport.Generation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FiasGarReport.Services;

/// <summary>
/// Оркестратор для выполнения полной последовательности операций по созданию отчета.
/// </summary>
public class ReportOrchestrator(
    IFiasDownloadService fiasDownloadService,
    IGarArchiveDownloader archiveDownloader,
    AddressChangeService addressChangeService,
    ReportGenerator reportGenerator,
    ILogger<ReportOrchestrator> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Начало работы программы...");

            DownloadFileInfo downloadInfo = await fiasDownloadService.GetLastDownloadFileInfoAsync(cancellationToken);

            logger.LogInformation("Версия ФИАС: {VersionId}", downloadInfo.VersionId);
            logger.LogInformation("URL Delta: {Url}", downloadInfo.GarXMLDeltaUrl);
            logger.LogInformation("Дата выгрузки: {Date}", downloadInfo.Date);

            string zipFilePath = await archiveDownloader.DownloadDeltaArchiveAsync(downloadInfo.GarXMLDeltaUrl, cancellationToken);
            (List<string> addrObjFilePaths, string objectLevelsFilePath) = archiveDownloader.UnpackArchive(zipFilePath);

            logger.LogInformation("Найдено {Count} файлов AS_ADDR_OBJ.", addrObjFilePaths.Count);
            addrObjFilePaths.ForEach(p => logger.LogInformation("  - {Path}", p));
            logger.LogInformation("Путь к AS_OBJECT_LEVELS: {Path}", objectLevelsFilePath);

            ReportModel reportModel = addressChangeService.ProcessAddressChanges(objectLevelsFilePath, addrObjFilePaths, downloadInfo);
            
            logger.LogInformation("Модель отчета сформирована для даты: {ReportDate}", reportModel.ReportDateText);
            logger.LogInformation("Найдено {Count} блоков уровней.", reportModel.LevelBlocks.Count);

            string htmlReportContent = reportGenerator.GenerateHtmlReport(reportModel);
            string finalReportPath = reportGenerator.SaveReport(htmlReportContent, reportModel.ReportDateText);

            logger.LogInformation("====================================================================");
            logger.LogInformation("*** Отчет успешно сформирован за {ReportDate}. ***", reportModel.ReportDateText);
            logger.LogInformation("Путь к файлу отчета: {Path}", finalReportPath);
            logger.LogInformation("====================================================================");

            await Task.Delay(100); // Даем логгеру время на вывод последних сообщений
            Console.Write("Хотите открыть сгенерированный отчет? (да/нет): ");
            string? userInput = Console.ReadLine();
            var trimmedInput = userInput?.Trim();
            
            string[] affirmativeAnswers = ["да", "д", "y", "yes"];

            if (trimmedInput != null && affirmativeAnswers.Contains(trimmedInput, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(finalReportPath) { UseShellExecute = true });
                    logger.LogInformation("Открываем отчет...");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Не удалось открыть файл отчета.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Операция была отменена.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Произошла критическая ошибка во время выполнения");
        }
        finally
        {
            logger.LogInformation("Работа программы завершена.");
        }
    }
}
