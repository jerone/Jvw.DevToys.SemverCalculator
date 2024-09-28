using Jvw.DevToys.SemverCalculator.Detectors;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Detectors;

/// <summary>
/// Fixture for SemVersionRangeDataTypeDetector tests.
/// </summary>
internal class SemVersionRangeDataTypeDetectorFixture
{
    private readonly Mock<IVersionService> _versionServiceMock = new(MockBehavior.Strict);

    internal SemVersionRangeDataTypeDetector CreateSut()
    {
        return new SemVersionRangeDataTypeDetector(_versionServiceMock.Object);
    }

    /// <summary>
    /// Verify all mocks.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal SemVersionRangeDataTypeDetectorFixture VerifyAll()
    {
        _versionServiceMock.VerifyAll();
        _versionServiceMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `VersionService.IsValidRange`.
    /// </summary>
    /// <param name="range">SemVer range.</param>
    /// <param name="result">Whether range is valid.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal SemVersionRangeDataTypeDetectorFixture WithVersionServiceIsValidRange(
        string range,
        bool result
    )
    {
        _versionServiceMock
            .Setup(x => x.IsValidRange(range))
            .Returns(result)
            .Verifiable(Times.Once);
        return this;
    }
}
