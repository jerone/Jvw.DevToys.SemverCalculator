using System.ComponentModel;
using System.Net;
using Jvw.DevToys.SemverCalculator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

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
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = _httpMessageHandlerMock.CreateClient();
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    [Description("Fetch package with existing package returns package versions.")]
    public async Task FetchPackage_WithExistingPackage_ReturnsPackageVersions()
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
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, "https://registry.npmjs.org/test-package/")
            .ReturnsResponse(packageJson, "application/vnd.npm.install-vl+json");
        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage("test-package");

        // Assert.
        Assert.NotNull(result);
        Assert.Equal("test-package", result.Name);
        Assert.Equal(["1.0.0", "1.1.0", "2.0.0"], result.Versions);
    }

    [Fact]
    [Description("Fetch package with nonexistent package returns null.")]
    public async Task FetchPackage_WithNonexistentPackage_ReturnsNull()
    {
        // Arrange.
        const string packageName = "nonexistent-package";
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, "https://registry.npmjs.org/nonexistent-package/")
            .ReturnsResponse(HttpStatusCode.NotFound);
        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
    }

    [Fact]
    [Description("Fetch package with fetch failed returns null.")]
    public async Task FetchPackage_WithFetchFailed_ReturnsNull()
    {
        // Arrange.
        const string packageName = "test-package";
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, "https://registry.npmjs.org/test-package/")
            .Throws(new HttpRequestException("Failed to fetch package."));
        var npmService = new NpmService(_httpClient, _loggerMock.Object);

        // Act.
        var result = await npmService.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
    }
}
