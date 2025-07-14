using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AsynchronousWebPageDownload;

public class Program
{
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient();
                services.AddTransient<Downloader>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var config = host.Services.GetRequiredService<IConfiguration>();

        if (args.Length == 0)
        {
            logger.LogError("No arguments provided. Please specify a URL or a file containing URLs.");
            return;
        }

        var input = args[0];
        var outputDir = args.Length > 1 ? args[1] : config["DefaultOutputDirectory"] ?? "DownloadedPages";

        List<string> urls = new();
        if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
        {
            urls.Add(input);
        }
        else if (File.Exists(input))
        {
            urls = File.ReadAllLines(input).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }
        else
        {
            logger.LogError("Invalid input. Provide a valid URL or a path to a file with URLs.");
            return;
        }

        Directory.CreateDirectory(outputDir);

        var downloader = host.Services.GetRequiredService<Downloader>();
        await downloader.DownloadPagesAsync(urls, outputDir);
    }
}
