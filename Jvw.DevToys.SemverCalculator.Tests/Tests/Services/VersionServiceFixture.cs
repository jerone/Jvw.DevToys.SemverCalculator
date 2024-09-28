using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Fixture for version service tests.
/// </summary>
internal class VersionServiceFixture : IBaseFixture<VersionService, VersionServiceFixture>
{
    private readonly Mock<IClipboard> _clipboardMock = new(MockBehavior.Strict);

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public VersionService CreateSut()
    {
        return new VersionService(_clipboardMock.Object);
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public VersionServiceFixture VerifyAll()
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
