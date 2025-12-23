using System.Xml;
using FiasGarReport.Models;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Parsers;

// Парсер для чтения файла AS_OBJECT_LEVELS_*.XML
public class GarObjectLevelsParser(ILogger<GarObjectLevelsParser> logger) : IXmlParser<ObjectLevel>
{
    public IEnumerable<ObjectLevel> Parse(string filePath)
    {
        logger.LogInformation("Парсинг файла справочника уровней: {FilePath}...", filePath);

        var objectLevels = new List<ObjectLevel>();
        var settings = new XmlReaderSettings { Async = true };

        using XmlReader reader = XmlReader.Create(filePath, settings);
        
        // Намеренно не использую в следующем цикле "yield return", так как количество искомых уровней небольшое
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == GarXmlConstants.ObjectLevel)
            {
                var levelIdStr = reader.GetAttribute(GarXmlConstants.Level);
                var name = reader.GetAttribute(GarXmlConstants.Name);
                var shortName = reader.GetAttribute(GarXmlConstants.ShortName);
                var isActiveStr = reader.GetAttribute(GarXmlConstants.IsActive);

                if (isActiveStr != "true") 
                {
                    continue;
                }

                if (int.TryParse(levelIdStr, out int levelId))
                {
                    objectLevels.Add(new ObjectLevel
                    {
                        LevelId = levelId,
                        Name = name ?? string.Empty,
                        ShortName = shortName ?? string.Empty
                    });
                }
            }
        }

        logger.LogInformation("Завершено парсинг справочника уровней. Найдено {Count} активных уровней.", objectLevels.Count);
        return objectLevels;
    }
}
