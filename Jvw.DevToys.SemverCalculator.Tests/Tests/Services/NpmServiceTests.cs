using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// NPM service tests.
/// </summary>
public class NpmServiceTests
{
    [Fact]
    [Description("Verify that PackageManager is set to NPM.")]
    public void PackageManager_ReturnsNpm()
    {
        // Arrange.
        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.PackageManager;

        // Assert.
        Assert.Equal(PackageManager.Npm, result);
    }

    [Fact]
    [Description(
        "Verify that GetVersions returns versions (not pre-release) that match the range."
    )]
    public async Task GetVersions_WithoutPreReleases_ReturnsNoPreReleaseVersions()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "^2.0.0";

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.GetVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description("Verify that GetVersions returns (pre-release) versions that match the range.")]
    public async Task GetVersions_WithPreReleases_ReturnsAllVersions()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "^2.0.0-0";

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.GetVersions(includePreReleases: true);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description("Verify that GetVersions returns sorted versions.")]
    public async Task GetVersions_WithRandomOrder_ReturnsOrderedVersions()
    {
        // Arrange.
        var versions = new List<string> { "2.0.0", "3.0.0", "2.0.0-0", "1.0.0" };
        const string rangeValue = "*";

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.GetVersions(includePreReleases: true);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description("Verify that GetVersions returns empty list when there are no versions at all.")]
    public void GetVersions_WithoutVersions_ReturnsNoVersions()
    {
        // Arrange.
        var versions = new List<string>();

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);

        // Act.
        var result = sut.GetVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Empty(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that GetVersions returns versions without matching range.")]
    public async Task GetVersions_WithInvalidRange_ReturnsNoMatchingVersions()
    {
        // Arrange.
        var versions = new List<string> { "1.0.0", "2.0.0", "2.0.0-0", "3.0.0" };
        const string rangeValue = "invalid";

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        sut.SetVersions(versions);
        sut.TryParseRange(rangeValue);

        // Act.
        var result = sut.GetVersions(includePreReleases: false);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        fixture.VerifyAll();
        await Verify(result);
    }

    [Fact]
    [Description("Return true when trying to parse valid SemVer range.")]
    public void TryParseRange_WithValidRange_ReturnsTrue()
    {
        // Arrange.
        const string rangeValue = "2.1 || ^3.2 || ~5.0.5 || 7.* || 8.0.0-1";

        var fixture = new NpmServiceFixture();
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

        var fixture = new NpmServiceFixture();
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

        var fixture = new NpmServiceFixture();
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

        var fixture = new NpmServiceFixture();
        var sut = fixture.CreateSut();

        // Act.
        var result = sut.IsValidRange(rangeValue);

        // Assert.
        Assert.False(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Fetch package with existing package returns package versions.")]
    public async Task FetchPackage_WithExistingPackage_ReturnsPackageVersions()
    {
        // Arrange.
        const string packageName = "test-package";
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

        var fixture = new NpmServiceFixture()
            .WithSetupLoggerLog()
            .WithSetupOkGetRequest(packageName, packageJson);
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.FetchPackage(packageName);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(["1.0.0", "1.1.0", "2.0.0"], result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Fetch package with nonexistent package returns null.")]
    public async Task FetchPackage_WithNonexistentPackage_ReturnsNull()
    {
        // Arrange.
        const string packageName = "nonexistent-package";
        var fixture = new NpmServiceFixture()
            .WithSetupLoggerLog()
            .WithSetupNotFoundGetRequest(packageName);
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Fetch package with fetch failed returns null.")]
    public async Task FetchPackage_WithFetchFailed_ReturnsNull()
    {
        // Arrange.
        const string packageName = "test-package";
        var fixture = new NpmServiceFixture().WithSetupLoggerLog().WithThrowGetRequest(packageName);
        var sut = fixture.CreateSut();

        // Act.
        var result = await sut.FetchPackage(packageName);

        // Assert.
        Assert.Null(result);
        fixture.VerifyAll();
    }
}
