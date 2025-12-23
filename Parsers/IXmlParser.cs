namespace FiasGarReport.Parsers;

/// <summary>
/// Общий интерфейс для парсеров XML-файлов.
/// </summary>
/// <typeparam name="T">Тип объекта, который будет возвращен парсером.</typeparam>
public interface IXmlParser<out T>
{
    /// <summary>
    /// Парсит указанный файл и возвращает перечисление объектов.
    /// </summary>
    /// <param name="filePath">Путь к XML-файлу.</param>
    /// <returns>Перечисление объектов типа <typeparamref name="T"/>.</returns>
    IEnumerable<T> Parse(string filePath);
}
