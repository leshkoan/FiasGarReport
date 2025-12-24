using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiasGarReport.Services;

// Сервис для загрузки и распаковки архива ГАР
public class GarArchiveDownloader(
    HttpClient httpClient, 
    IConfiguration configuration, 
    ILogger<GarArchiveDownloader> logger) : IGarArchiveDownloader
{
    private readonly string _tempDirectory = configuration["Paths:TempDirectory"] ?? "temp";

    public async Task<string> DownloadDeltaArchiveAsync(string archiveUrl, CancellationToken cancellationToken = default)
    {
        EnsureTempDirectoryExists();
        logger.LogInformation("Скачивание delta-архива с: {ArchiveUrl}...", archiveUrl);

        if (string.IsNullOrEmpty(archiveUrl))
        {
            throw new ArgumentNullException(nameof(archiveUrl), "URL архива не может быть пустым.");
        }

        var uri = new Uri(archiveUrl);
        var fileName = Path.GetFileName(uri.LocalPath);
        var filePath = Path.Combine(_tempDirectory, fileName);

        if (File.Exists(filePath))
        {
            logger.LogInformation("Архив уже существует: {FilePath}. Пропускаем скачивание.", filePath);
            return filePath;
        }

        using var response = await httpClient.GetAsync(archiveUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        
        await stream.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Delta-архив скачан в: {FilePath}", filePath);
        return filePath;
    }

    public (List<string> addrObjFilePaths, string objectLevelsFilePath) UnpackArchive(string zipFilePath)
    {
        EnsureTempDirectoryExists();
        logger.LogInformation("Распаковка архива: {ZipFilePath}...", zipFilePath);

        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"Архив не найден по пути: {zipFilePath}");
        }

        string extractPath = Path.Combine(_tempDirectory, Path.GetFileNameWithoutExtension(zipFilePath));
        
        string? objectLevelsFilePath = null;
        List<string> addrObjFilePaths;

        if (Directory.Exists(extractPath))
        {
            objectLevelsFilePath = Directory.EnumerateFiles(extractPath, "AS_OBJECT_LEVELS_*.XML", SearchOption.TopDirectoryOnly)
                                            .FirstOrDefault();
            addrObjFilePaths = Directory.EnumerateFiles(extractPath, "AS_ADDR_OBJ_*.XML", SearchOption.AllDirectories)
                                        .Where(f => !Path.GetFileName(f).Contains("TYPES"))
                                        .ToList();

            if (objectLevelsFilePath != null && addrObjFilePaths.Any())
            {
                logger.LogInformation("Архив уже распакован в: {ExtractPath}. Пропускаем распаковку.", extractPath);
                return (addrObjFilePaths, objectLevelsFilePath);
            }
            logger.LogInformation("Директория {ExtractPath} существует, но не содержит всех необходимых файлов. Перераспаковываем.", extractPath);
            Directory.Delete(extractPath, true);
        }
        
        Directory.CreateDirectory(extractPath);
        ZipFile.ExtractToDirectory(zipFilePath, extractPath);

        logger.LogInformation("Архив распакован в: {ExtractPath}", extractPath);

        objectLevelsFilePath = Directory.EnumerateFiles(extractPath, "AS_OBJECT_LEVELS_*.XML", SearchOption.TopDirectoryOnly)
                                        .FirstOrDefault();
        addrObjFilePaths = Directory.EnumerateFiles(extractPath, "AS_ADDR_OBJ_*.XML", SearchOption.AllDirectories)
                                   .Where(f => !Path.GetFileName(f).Contains("TYPES"))
                                   .ToList();

        if (!addrObjFilePaths.Any())
        {
            throw new FileNotFoundException("Не удалось найти файл AS_ADDR_OBJ_*.XML (исключая TYPES) в распакованном архиве (и его подпапках).");
        }
        if (objectLevelsFilePath == null)
        {
            throw new FileNotFoundException("Не удалось найти файл AS_OBJECT_LEVELS_*.XML в распакованном архиве (в корне).");
        }

        return (addrObjFilePaths, objectLevelsFilePath);
    }
    
    private void EnsureTempDirectoryExists()
    {
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }
}
