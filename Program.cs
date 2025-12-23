using FiasGarReport.Services;
using FiasGarReport.Parsers;
using FiasGarReport.Generation;
using FiasGarReport.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FiasGarReport;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        
        var orchestrator = host.Services.GetRequiredService<ReportOrchestrator>();
        
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await orchestrator.RunAsync(cts.Token);
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();
                configuration
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Регистрация сервисов и их интерфейсов
                services.AddSingleton<HttpClient>();
                services.AddTransient<IFiasDownloadService, FiasDownloadService>();
                services.AddTransient<IGarArchiveDownloader, GarArchiveDownloader>();
                services.AddTransient<IXmlParser<ObjectLevel>, GarObjectLevelsParser>();
                services.AddTransient<IXmlParser<AddressObject>, GarAddrObjParser>();
                services.AddTransient<AddressChangeService>();
                services.AddTransient<ReportGenerator>();
                
                // Регистрация оркестратора
                services.AddTransient<ReportOrchestrator>();
            });
}
