using System.Xml;
using FiasGarReport.Models;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Parsers;

// Парсер для чтения файла AS_ADDR_OBJ_*.XML
public class GarAddrObjParser(ILogger<GarAddrObjParser> logger) : IXmlParser<AddressObject>
{
    public IEnumerable<AddressObject> Parse(string filePath)
    {
        logger.LogInformation("Парсинг файла адресных объектов: {FilePath}...", filePath);

        var settings = new XmlReaderSettings { Async = true };
        
        using XmlReader reader = XmlReader.Create(filePath, settings);
        
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == GarXmlConstants.Object)
            {
                var isActiveStr = reader.GetAttribute(GarXmlConstants.IsActive);
                var isActualStr = reader.GetAttribute(GarXmlConstants.IsActual);

                if (isActiveStr == "1" && isActualStr == "1")
                {
                    var levelIdStr = reader.GetAttribute(GarXmlConstants.Level);
                    
                    if (int.TryParse(levelIdStr, out int levelId))
                    {
                        yield return new AddressObject
                        {
                            LevelId = levelId,
                            TypeName = reader.GetAttribute(GarXmlConstants.TypeName) ?? string.Empty,
                            Name = reader.GetAttribute(GarXmlConstants.Name) ?? string.Empty,
                            IsActive = true,
                            ObjectId = reader.GetAttribute(GarXmlConstants.ObjectId) ?? string.Empty,
                            ObjectGuid = reader.GetAttribute(GarXmlConstants.ObjectGuid) ?? string.Empty
                        };
                    }
                }
            }
        }
        logger.LogInformation("Завершено парсинг адресных объектов.");
    }
}
