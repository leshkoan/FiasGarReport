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
    /// <returns>2 пути к файлам AS_ADDR_OBJ и AS_OBJECT_LEVELS соответственно.</returns>
    (string addrObjFilePath, string objectLevelsFilePath) UnpackArchive(string zipFilePath);
}
