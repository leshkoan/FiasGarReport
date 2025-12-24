namespace FiasGarReport.Services;

/// <summary>
/// Интерфейс для сервиса загрузки и распаковки архивов.
/// </summary>
public interface IGarArchiveDownloader
{
    /// <summary>
    /// Асинхронно скачивает архив по URL.
    /// </summary>
    /// <param name="archiveUrl">URL архива.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Путь к скачанному файлу.</returns>
    Task<string> DownloadDeltaArchiveAsync(string archiveUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Распаковывает архив и находит пути к ключевым XML-файлам.
    /// </summary>
    /// <param name="zipFilePath">Путь к zip-архиву.</param>
    /// <returns>Список путей к файлам AS_ADDR_OBJ и путь к файлу AS_OBJECT_LEVELS.</returns>
    (List<string> addrObjFilePaths, string objectLevelsFilePath) UnpackArchive(string zipFilePath);
}
