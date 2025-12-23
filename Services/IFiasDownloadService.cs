using FiasGarReport.Models;

namespace FiasGarReport.Services;

/// <summary>
/// Интерфейс для сервиса получения информации о файлах для скачивания.
/// </summary>
public interface IFiasDownloadService
{
    /// <summary>
    /// Асинхронно получает информацию о последней версии файлов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Объект с информацией о файлах.</returns>
    Task<DownloadFileInfo> GetLastDownloadFileInfoAsync(CancellationToken cancellationToken = default);
}
