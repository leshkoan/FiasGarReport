using FiasGarReport.Models;
using FiasGarReport.Parsers;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Services;

// Сервис для обработки изменений адресных объектов и формирования модели отчета 
// (в виду простоты проекта отдельный интерфейс для данного сервиса я намеренно не создаю)
public class AddressChangeService(
    IXmlParser<ObjectLevel> objectLevelsParser, 
    IXmlParser<AddressObject> addrObjParser,
    ILogger<AddressChangeService> logger)
{
    public ReportModel ProcessAddressChanges(string objectLevelsFilePath, List<string> addrObjFilePaths, DownloadFileInfo downloadInfo)
    {
        logger.LogInformation("Обработка изменений адресных объектов...");

        var objectLevels = objectLevelsParser.Parse(objectLevelsFilePath);
        var objectLevelsDict = objectLevels.ToDictionary(ol => ol.LevelId);

        var allAddressObjects = new List<AddressObject>();
        foreach(var addrObjFilePath in addrObjFilePaths)
        {
            logger.LogInformation("Парсинг файла: {Path}", addrObjFilePath);
            allAddressObjects.AddRange(addrObjParser.Parse(addrObjFilePath));
        }

        var groupedObjects = allAddressObjects
            .Where(ao => objectLevelsDict.ContainsKey(ao.LevelId))
            .GroupBy(ao => ao.LevelId)
            .OrderBy(g => g.Key)
            .Select(g => new ReportLevelBlock
            {
                LevelInfo = objectLevelsDict[g.Key],
                Objects = g.OrderBy(ao => ao.Name).ToList()
            })
            .ToList();

        logger.LogInformation("Обработка изменений адресных объектов завершена.");

        return new ReportModel
        {
            ReportDateText = downloadInfo.Date,
            LevelBlocks = groupedObjects
        };
    }
}
