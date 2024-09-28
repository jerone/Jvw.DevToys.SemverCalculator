using System.ComponentModel;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// NPM service tests.
/// </summary>
public class NpmServiceTests
{
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
        Assert.Equal("test-package", result.Name);
        Assert.Equal(["1.0.0", "1.1.0", "2.0.0"], result.Versions);
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
