using System.Diagnostics.CodeAnalysis;
using Jvw.DevToys.SemverCalculator.Detectors;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Detectors;

/// <summary>
/// Fixture for SemVersionRangeDataTypeDetector tests.
/// </summary>
[SuppressMessage(
    "PosInformatique.Moq.Analyzers",
    "PosInfoMoq1002",
    Justification = "Verification is handled in VerifyAll method."
)]
internal class SemVersionRangeDataTypeDetectorFixture
    : IBaseFixture<SemVersionRangeDataTypeDetector, SemVersionRangeDataTypeDetectorFixture>
{
    private readonly Mock<IEnumerable<IPackageManagerService>> _packageManagerServicesMock =
        new(MockBehavior.Strict);
    private readonly Mock<IPackageManagerService> _packageManagerServiceMock =
        new(MockBehavior.Strict);

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public SemVersionRangeDataTypeDetector CreateSut()
    {
        return new SemVersionRangeDataTypeDetector(_packageManagerServicesMock.Object);
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public SemVersionRangeDataTypeDetectorFixture VerifyAll()
    {
        _packageManagerServicesMock.VerifyAll();
        _packageManagerServicesMock.VerifyNoOtherCalls();
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
        var items = new List<IPackageManagerService>();

        _packageManagerServiceMock
            .Setup(service => service.IsValidRange(range))
            .Returns(result)
            .Verifiable(Times.Once);
        items.Add(_packageManagerServiceMock.Object);

        _packageManagerServicesMock
            .Setup(services => services.GetEnumerator())
            .Returns(() => items.GetEnumerator());

        return this;
    }
}
