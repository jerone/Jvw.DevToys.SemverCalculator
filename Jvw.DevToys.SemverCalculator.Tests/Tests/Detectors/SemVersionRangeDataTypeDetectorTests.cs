using System.ComponentModel;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Detectors;

/// <summary>
/// Unit tests for SemVersionRangeDataTypeDetector.
/// </summary>
public class SemVersionRangeDataTypeDetectorTests
{
    [Fact]
    [Description(
        "Verify that TryDetectDataAsync returns success when valid SemVer range is provided."
    )]
    public async Task TryDetectDataAsync_ValidSemVerRange_ReturnsSuccess()
    {
        // Arrange.
        const string semVerRange = "1.0.0 - 2.0.0";

        var fixture =
            new SemVersionRangeDataTypeDetectorFixture().WithPackageVersionServiceIsValidRange(
                semVerRange,
                true
            );
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.TryDetectDataAsync(semVerRange, null, It.IsAny<CancellationToken>());

        // Assert.
        Assert.True(result.Success);
        Assert.Equal(semVerRange, result.Data);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that TryDetectDataAsync returns unsuccessful when invalid SemVer range is provided."
    )]
    public async Task TryDetectDataAsync_InvalidSemVerRange_ReturnsUnsuccessful()
    {
        // Arrange.
        const string semVerRange = "invalid-range";

        var fixture =
            new SemVersionRangeDataTypeDetectorFixture().WithPackageVersionServiceIsValidRange(
                semVerRange,
                false
            );
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.TryDetectDataAsync(semVerRange, null, It.IsAny<CancellationToken>());

        // Assert.
        Assert.False(result.Success);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that TryDetectDataAsync returns unsuccessful when rawData is not a string."
    )]
    public async Task TryDetectDataAsync_NonStringRawData_ReturnsUnsuccessful()
    {
        // Arrange.
        var nonStringRawData = new object();

        var fixture = new SemVersionRangeDataTypeDetectorFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.TryDetectDataAsync(
            nonStringRawData,
            null,
            It.IsAny<CancellationToken>()
        );

        // Assert.
        Assert.False(result.Success);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that TryDetectDataAsync returns unsuccessful when rawData is an empty string."
    )]
    public async Task TryDetectDataAsync_EmptyStringRawData_ReturnsUnsuccessful()
    {
        // Arrange.
        var emptyStringRawData = string.Empty;

        var fixture = new SemVersionRangeDataTypeDetectorFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.TryDetectDataAsync(
            emptyStringRawData,
            null,
            It.IsAny<CancellationToken>()
        );

        // Assert.
        Assert.False(result.Success);
        fixture.VerifyAll();
    }
}
