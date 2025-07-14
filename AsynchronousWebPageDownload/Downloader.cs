using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AsynchronousWebPageDownload;
public class Downloader( IHttpClientFactory httpClientFactory, ILogger<Downloader> logger, IConfiguration configuration)
{
    private readonly IHttpClientFactory _httpClientFactory  = httpClientFactory;
    private readonly ILogger<Downloader> _logger            = logger;
    private readonly IConfiguration _configuration          = configuration;

    public async Task DownloadPagesAsync(List<string> urls, string outputDir)
    {
        var tasks = urls.Select(url => DownloadWithRetryAsync(url, outputDir));
        await Task.WhenAll(tasks);
    }

    private async Task DownloadWithRetryAsync(string url, string outputDir)
    {
        int retries = int.TryParse(_configuration["RetryCount"], out var r) ? r : 3;
        int delay   = int.TryParse(_configuration["RetryDelaySeconds"], out var d) ? d : 10;

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                var client      = _httpClientFactory.CreateClient();
                var response    = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content     = await response.Content.ReadAsStringAsync();
                var filename    = GetSafeFilename(url) + ".html";
                var filePath    = Path.Combine(outputDir, filename);
                await File.WriteAllTextAsync(filePath, content);

                _logger.LogInformation("Downloaded: {Url}", url);
                Console.WriteLine($"Downloaded: {url}");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Url}", attempt, url);
                if (attempt == retries)
                {
                    _logger.LogError("Failed to download {Url} after {Retries} attempts", url, retries);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }
        }
    }

    private string GetSafeFilename(string url)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            url = url.Replace(c, '_');
        }
        return url.Replace("https://", "").Replace("http://", "");
    }
}