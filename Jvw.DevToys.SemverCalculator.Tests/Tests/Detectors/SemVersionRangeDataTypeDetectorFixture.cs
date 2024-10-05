using Jvw.DevToys.SemverCalculator.Detectors;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Detectors;

/// <summary>
/// Fixture for SemVersionRangeDataTypeDetector tests.
/// </summary>
internal class SemVersionRangeDataTypeDetectorFixture
    : IBaseFixture<SemVersionRangeDataTypeDetector, SemVersionRangeDataTypeDetectorFixture>
{
    private readonly Mock<IPackageManagerService> _packageManagerServiceMock =
        new(MockBehavior.Strict);

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public SemVersionRangeDataTypeDetector CreateSut()
    {
        return new SemVersionRangeDataTypeDetector(_packageManagerServiceMock.Object);
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public SemVersionRangeDataTypeDetectorFixture VerifyAll()
    {
        _packageManagerServiceMock.VerifyAll();
        _packageManagerServiceMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerService.IsValidRange`.
    /// </summary>
    /// <param name="range">SemVer range.</param>
    /// <param name="result">Whether range is valid.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal SemVersionRangeDataTypeDetectorFixture WithPackageManagerServiceIsValidRange(
        string range,
        bool result
    )
    {
        _packageManagerServiceMock
            .Setup(x => x.IsValidRange(range))
            .Returns(result)
            .Verifiable(Times.Once);
        return this;
    }
}
