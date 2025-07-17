using AsynchronousWebPageDownload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AsynchronousWebPageDownloadTests;

public class DownloaderTests()
{
    [Fact]
    public async Task Downloader_SavesFile_WhenDownloadSuccessful()
    {
        // Arrange
        var url = "https://example.com/test";
        var content = "<html>Test</html>";
        var outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(outputDir);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            });

        var client = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var loggerMock = new Mock<ILogger<Downloader>>();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "RetryCount", "3" },
            { "RetryDelaySeconds", "1" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var downloader = new Downloader(factoryMock.Object, loggerMock.Object, configuration);

        // Act
        await downloader.DownloadPagesAsync(new List<string> { url }, outputDir);

        // Assert
        var savedFiles = Directory.GetFiles(outputDir);
        Assert.Single(savedFiles);
        var savedContent = await File.ReadAllTextAsync(savedFiles[0]);
        Assert.Equal(content, savedContent);

        // Cleanup
        Directory.Delete(outputDir, true);
    }
}