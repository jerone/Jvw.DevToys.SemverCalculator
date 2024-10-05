using System.ComponentModel.Composition;
using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Factory responsible for loading the correct package manager services.
/// </summary>
[Export(typeof(IPackageManagerFactory))]
[method: ImportingConstructor]
internal class PackageManagerFactory(
    IPackageManagerService packageManagerService //IEnumerable<IPackageManagerService> packageManagerServices
) : IPackageManagerFactory
{
    /// <summary>
    /// Load the package manager service.
    /// </summary>
    /// <param name="packageManager">Package manager.</param>
    /// <returns>Package manager service.</returns>
    public IPackageManagerService Load(PackageManager packageManager)
    {
        return packageManagerService;
        //return packageManagerServices.FirstOrDefault(x => x.PackageManager == packageManager)
        //    ?? throw new NotSupportedException();
    }
}
