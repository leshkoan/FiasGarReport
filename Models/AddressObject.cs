namespace FiasGarReport.Models;

/// <summary>
/// Класс для хранения информации об адресном объекте
/// </summary>
public class AddressObject
{
    public int LevelId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string ObjectId { get; set; } = string.Empty;
    public string ObjectGuid { get; set; } = string.Empty;
}
