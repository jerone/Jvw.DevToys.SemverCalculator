using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Package manager factory tests.
/// </summary>
public class PackageManagerFactoryTests
{
    [Theory]
    [Description(
        "Verify that the package manager factory loads the correct package manager service."
    )]
    [InlineData(PackageManager.Npm)]
    [InlineData(PackageManager.NuGet)]
    public void Load_WithPackageManager_ReturnsCorrectPackageManagerService(
        PackageManager packageManager
    )
    {
        // Arrange.
        var fixture = new PackageManagerFactoryFixture().WithPackageManagerServices(packageManager);
        var sut = fixture.CreateSut();

        // Act.
        var service = sut.Load(packageManager);

        // Assert.
        Assert.NotNull(service);
        Assert.Equal(packageManager, service.PackageManager);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that the package manager factory throws an exception when the package manager service is not found."
    )]
    public async Task Load_WithMissingService_ThrowsException()
    {
        // Arrange.
        const PackageManager packageManager = PackageManager.NuGet;
        var fixture = new PackageManagerFactoryFixture().WithPackageManagerServices();
        var sut = fixture.CreateSut();

        // Act.
        void Result() => sut.Load(packageManager);

        // Assert.
        var exception = Assert.Throws<NotSupportedException>(Result);
        await Verify(exception);
        fixture.VerifyAll();
    }
}
