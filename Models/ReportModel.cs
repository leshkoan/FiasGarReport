namespace FiasGarReport.Models;

/// <summary>
/// Класс для блока отчёта по конкретному уровню адресных объектов
/// </summary>
public class ReportLevelBlock
{
    public ObjectLevel LevelInfo { get; set; } = new();
    public List<AddressObject> Objects { get; set; } = new();
}

/// <summary>
/// Класс для полной модели отчёта
/// </summary>
public class ReportModel
{
    public string ReportDateText { get; set; } = string.Empty;
    public List<ReportLevelBlock> LevelBlocks { get; set; } = new();
}
