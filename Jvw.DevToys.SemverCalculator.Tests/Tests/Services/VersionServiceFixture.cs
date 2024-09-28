using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Fixture for version service tests.
/// </summary>
internal class VersionServiceFixture
{
    private readonly Mock<IClipboard> _clipboardMock = new(MockBehavior.Strict);

    /// <summary>
    /// Create the system under test.
    /// </summary>
    /// <returns>System under test.</returns>
    internal VersionService CreateSut()
    {
        return new VersionService(_clipboardMock.Object);
    }

    /// <summary>
    /// Verify all mocks.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal VersionServiceFixture VerifyAll()
    {
        _clipboardMock.VerifyAll();
        _clipboardMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `IClipboard.SetClipboardTextAsync`.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal VersionServiceFixture WithClipboardSetClipboardTextAsync(string semver)
    {
        _clipboardMock
            .Setup(c => c.SetClipboardTextAsync(semver))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        return this;
    }
}
