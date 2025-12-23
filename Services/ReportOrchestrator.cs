using FiasGarReport.Models;
using FiasGarReport.Generation;
using Microsoft.Extensions.Logging;

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
            (string addrObjFilePath, string objectLevelsFilePath) = archiveDownloader.UnpackArchive(zipFilePath);

            logger.LogInformation("Путь к AS_ADDR_OBJ: {Path}", addrObjFilePath);
            logger.LogInformation("Путь к AS_OBJECT_LEVELS: {Path}", objectLevelsFilePath);

            ReportModel reportModel = addressChangeService.ProcessAddressChanges(objectLevelsFilePath, addrObjFilePath, downloadInfo);
            
            logger.LogInformation("Модель отчета сформирована для даты: {ReportDate}", reportModel.ReportDateText);
            logger.LogInformation("Найдено {Count} блоков уровней.", reportModel.LevelBlocks.Count);

            string htmlReportContent = reportGenerator.GenerateHtmlReport(reportModel);
            string finalReportPath = reportGenerator.SaveReport(htmlReportContent, reportModel.ReportDateText);

            logger.LogInformation("====================================================================");
            logger.LogInformation("*** Отчет успешно сформирован за {ReportDate}. ***", reportModel.ReportDateText);
            logger.LogInformation("Путь к файлу отчета: {Path}", finalReportPath);
            logger.LogInformation("====================================================================");
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
