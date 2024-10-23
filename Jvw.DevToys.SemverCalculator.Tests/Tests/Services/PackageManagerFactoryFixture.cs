using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Fixture for the <see cref="PackageManagerFactory"/> class.
/// </summary>
internal class PackageManagerFactoryFixture
    : IBaseFixture<PackageManagerFactory, PackageManagerFactoryFixture>
{
    private readonly Mock<IEnumerable<IPackageManagerService>> _packageManagerServicesMock =
        new(MockBehavior.Strict);
    private readonly Mock<IPackageManagerService> _packageManagerServiceMock =
        new(MockBehavior.Strict);

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public PackageManagerFactory CreateSut()
    {
        return new PackageManagerFactory(_packageManagerServicesMock.Object);
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public PackageManagerFactoryFixture VerifyAll()
    {
        _packageManagerServicesMock.VerifyAll();
        _packageManagerServicesMock.VerifyNoOtherCalls();
        _packageManagerServiceMock.VerifyAll();
        _packageManagerServiceMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `IEnumerable&lt;IPackageManagerService&gt;`, which returns a list of services with a package manager if specified.
    /// When a package manager is specified, it will return a package manager service.
    /// If not specified, it will return an empty list.
    /// </summary>
    /// <param name="packageManager">Package manager or null.</param>
    /// <returns>This fixture, for chaining.</returns>
    public PackageManagerFactoryFixture WithPackageManagerServices(
        PackageManager? packageManager = null
    )
    {
        var items = new List<IPackageManagerService>();

        // If package manager is specified, add a mocked package manager service to the list.
        if (packageManager.HasValue)
        {
            _packageManagerServiceMock
                .SetupGet(service => service.PackageManager)
                .Returns(packageManager.Value);
            items.Add(_packageManagerServiceMock.Object);
        }

        _packageManagerServicesMock
            .Setup(services => services.GetEnumerator())
            .Returns(() => items.GetEnumerator());
        return this;
    }
}
