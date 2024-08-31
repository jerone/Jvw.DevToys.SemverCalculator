using System.Net;
using Jvw.DevToys.SemverCalculator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Jvw.DevToys.SemverCalculator.Tests;

/// <summary>
/// NPM service tests.
/// </summary>
public class NpmServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger> _loggerMock;

    public NpmServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public async Task FetchPackage_WithValidPackageName_ReturnsPackageJson()
    {
        // Arrange.
        const string packageJson = """
{
  "name": "test-package",
  "description": "test description",
  "versions": {
    "1.0.0": {
      "name": "test-package",
      "version": "1.0.0"
	},
    "1.1.0": {
      "name": "test-package",
      "version": "1.1.0"
	},
    "2.0.0": {
      "name": "test-package",
      "version": "2.0.0"
	}
  }
}
""";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(packageJson),
        };
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage("test-package");

        // Assert.
        Assert.NotNull(result);
        Assert.Equal("test-package", result.Name);
        Assert.Equal(["1.0.0", "1.1.0", "2.0.0"], result.Versions);
    }

    [Fact]
    public async Task FetchPackage_WithNonexistentPackageName_ReturnsNull()
    {
        // Arrange.
        const string packageName = "nonexistent-package";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);
        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPackage_WithException_ReturnsNull()
    {
        // Arrange.
        const string packageName = "test-package";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Failed to fetch package."));
        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
    }
}
