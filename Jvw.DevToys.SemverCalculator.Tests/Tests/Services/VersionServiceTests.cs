using System.ComponentModel;
using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Version service tests.
/// </summary>
public class VersionServiceTests
{
    [Fact]
    [Description(
        "Verify that MatchVersions returns UI buttons for versions (not pre-release) that match the range."
    )]
    public async Task MatchVersions_WithoutPreReleases_ReturnsMatchingUIElements()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "^2.0.0";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description(
        "Verify that MatchVersions returns UI buttons for (pre-release) versions that match the range."
    )]
    public async Task MatchVersions_WithPreReleases_ReturnsMatchingUIElements()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "^2.0.0";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.MatchVersions(includePreReleases: true);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description(
        "Verify that MatchVersions returns no UI buttons when there are no versions at all."
    )]
    public void MatchVersions_WithoutVersions_ReturnsNoUIElements()
    {
        // Arrange.
        var versions = new List<string>();
        const string rangeValue = "^2.0.0";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Empty(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that MatchVersions returns UI buttons for versions without matching range."
    )]
    public async Task MatchVersions_WithInvalidRange_ReturnsNoMatchingUIElements()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "invalid";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description("Verify that clicking on UI button triggers the clipboard with correct version.")]
    public async Task MatchVersions_OnClickAction_TriggersClipboardWithVersion()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0" };
        const string rangeValue = "1.0.0";

        var fixture = new VersionServiceFixture().WithClipboardSetClipboardTextAsync(rangeValue);
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.MatchVersions(includePreReleases: false);
        await ((IUIButton)result[0]).OnClickAction!();

        // Assert.
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Return true when trying to parse valid SemVer range.")]
    public void TryParseRange_WithValidRange_ReturnsTrue()
    {
        // Arrange.
        const string rangeValue = "2.1 || ^3.2 || ~5.0.5 || 7.* || 8.0.0-1";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.TryParseRange(rangeValue);

        // Assert.
        Assert.True(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Return false when trying to parse invalid SemVer range.")]
    public void TryParseRange_WithInvalidRange_ReturnsFalse()
    {
        // Arrange.
        const string rangeValue = "invalid";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.TryParseRange(rangeValue);

        // Assert.
        Assert.False(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Return true when checking if value is valid SemVer range.")]
    public void IsValidRange_WithValidRange_ReturnsTrue()
    {
        // Arrange.
        const string rangeValue = "2.1 || ^3.2 || ~5.0.5 || 7.* || 8.0.0-1";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.IsValidRange(rangeValue);

        // Assert.
        Assert.True(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Return false when checking if value is invalid SemVer range.")]
    public void IsValidRange_WithInvalidRange_ReturnsFalse()
    {
        // Arrange.
        const string rangeValue = "invalid";

        var fixture = new VersionServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.IsValidRange(rangeValue);

        // Assert.
        Assert.False(result);
        fixture.VerifyAll();
    }
}
