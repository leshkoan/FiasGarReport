using System.Net.Http.Json;
using System.Text.Json;
using FiasGarReport.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Services;

/// <summary>
/// Сервис для получения информации о файлах ФИАС/ГАР через JSON API.
/// </summary>
public class FiasDownloadService(
    HttpClient httpClient, 
    IConfiguration configuration, 
    ILogger<FiasDownloadService> logger) : IFiasDownloadService
{
    private readonly string _serviceUrl = configuration["FiasSettings:ServiceUrl"] 
        ?? throw new InvalidOperationException("Не указан URL сервиса ФИАС в конфигурации (FiasSettings:ServiceUrl).");

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    /// <summary>
    /// Асинхронно получает информацию о последней версии файлов, делая GET-запрос к JSON-сервису.
    /// </summary>
    public async Task<DownloadFileInfo> GetLastDownloadFileInfoAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Получение информации о последней версии файлов с {ServiceUrl}...", _serviceUrl);

        var downloadInfo = await httpClient.GetFromJsonAsync<DownloadFileInfo>(_serviceUrl, _jsonOptions, cancellationToken);

        if (downloadInfo == null)
        {
            throw new InvalidOperationException("Не удалось получить или десериализовать информацию о файлах.");
        }

        logger.LogInformation("Информация о последней версии файлов ФИАС успешно получена.");
        return downloadInfo;
    }
}
