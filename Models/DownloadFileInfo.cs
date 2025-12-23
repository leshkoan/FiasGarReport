using System;
using System.Text.Json.Serialization;

namespace FiasGarReport.Models;

/// <summary>
/// Класс для хранения информации о последней версии файлов ФИАС/ГАР
/// </summary>
public class DownloadFileInfo
{
    public long VersionId { get; set; } 
    
    public string TextVersion { get; set; } = string.Empty;
    
    // Поле в JSON-фале, содержащее дату последней версии файлов
    [JsonPropertyName("GarXMLDeltaURL")]
    public string GarXMLDeltaUrl { get; set; } = string.Empty;
    
    public DateTime ExpDate { get; set; }
    
    public string Date { get; set; } = string.Empty;
}
