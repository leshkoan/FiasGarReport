namespace FiasGarReport.Models;

/// <summary>
/// Класс для хранения информации об уровне адресного объекта
/// </summary>
public class ObjectLevel
{
    public int LevelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
}
