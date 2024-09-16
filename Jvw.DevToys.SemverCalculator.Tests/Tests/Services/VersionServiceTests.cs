using System.ComponentModel;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// VersionService tests.
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
        var clipboardMock = new Mock<IClipboard>();
        var versionService = new VersionService(clipboardMock.Object);
        versionService.SetVersions(versions);
        versionService.TryParseRange(rangeValue);

        // Act.
        var result = versionService.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
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
        var clipboardMock = new Mock<IClipboard>();
        var versionService = new VersionService(clipboardMock.Object);
        versionService.SetVersions(versions);
        versionService.TryParseRange(rangeValue);

        // Act.
        var result = versionService.MatchVersions(includePreReleases: true);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
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
        var clipboardMock = new Mock<IClipboard>();
        var versionService = new VersionService(clipboardMock.Object);
        versionService.SetVersions(versions);
        versionService.TryParseRange(rangeValue);

        // Act.
        var result = versionService.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
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
        var clipboardMock = new Mock<IClipboard>();
        var versionService = new VersionService(clipboardMock.Object);
        versionService.SetVersions(versions);
        versionService.TryParseRange(rangeValue);

        // Act.
        var result = versionService.MatchVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        await Verify(result);
    }

    [Fact]
    [Description("Verify that clicking on UI button triggers the clipboard with correct version.")]
    public async Task OnClickAction_Invoke_TriggersClipboardWithVersion()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0" };
        const string rangeValue = "1.0.0";
        var clipboardMock = new Mock<IClipboard>();
        var versionService = new VersionService(clipboardMock.Object);
        versionService.SetVersions(versions);
        versionService.TryParseRange(rangeValue);
        var result = versionService.MatchVersions(includePreReleases: false);

        // Act.
        await ((IUIButton)result.First()).OnClickAction!();

        // Assert.
        clipboardMock.Verify(c => c.SetClipboardTextAsync("1.0.0"), Times.Once);
    }
}
