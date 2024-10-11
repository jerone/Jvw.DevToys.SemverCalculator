using System.Diagnostics.CodeAnalysis;
using System.Net;
using Jvw.DevToys.SemverCalculator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Fixture for NPM service tests.
/// </summary>
[SuppressMessage(
    "PosInformatique.Moq.Analyzers",
    "PosInfoMoq1002",
    Justification = "Verification is handled in VerifyAll method."
)]
internal class NpmServiceFixture : IBaseFixture<NpmService, NpmServiceFixture>
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new(MockBehavior.Strict);
    private readonly Mock<ILogger> _loggerMock = new(MockBehavior.Strict);

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public NpmService CreateSut()
    {
        var httpClient = _httpMessageHandlerMock.CreateClient();
        return new NpmService(httpClient, _loggerMock.Object);
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public NpmServiceFixture VerifyAll()
    {
        _httpMessageHandlerMock.VerifyAll();
        _httpMessageHandlerMock.VerifyNoOtherCalls();
        _loggerMock.VerifyAll();
        _loggerMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `ILogger.Log`.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal NpmServiceFixture WithSetupLoggerLog()
    {
        _loggerMock
            .Setup(l =>
                l.Log(
#pragma warning disable PosInfoMoq1003
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
#pragma warning restore PosInfoMoq1003
                )
            )
            .Verifiable(Times.AtLeastOnce);
        return this;
    }

    /// <summary>
    /// Setup mock for request to NPM registry, which returns status-code 200 (ok) with package JSON response.
    /// </summary>
    /// <param name="packageName">Package name on NPM registry.</param>
    /// <param name="packageJson">Package JSON response.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal NpmServiceFixture WithSetupOkGetRequest(string packageName, string packageJson)
    {
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, $"https://registry.npmjs.org/{packageName}/")
            .ReturnsResponse(packageJson, "application/vnd.npm.install-vl+json")
            .Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for request to NPM registry, which returns status-code 404 (not found).
    /// </summary>
    /// <param name="packageName">Package name on NPM registry.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal NpmServiceFixture WithSetupNotFoundGetRequest(string packageName)
    {
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, $"https://registry.npmjs.org/{packageName}/")
            .ReturnsResponse(HttpStatusCode.NotFound)
            .Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for request to NPM registry, which throws an exception.
    /// </summary>
    /// <param name="packageName">Package name on NPM registry.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal NpmServiceFixture WithThrowGetRequest(string packageName)
    {
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, $"https://registry.npmjs.org/{packageName}/")
            .Throws(new HttpRequestException("Failed to fetch package."))
            .Verifiable(Times.Once);
        return this;
    }
}
