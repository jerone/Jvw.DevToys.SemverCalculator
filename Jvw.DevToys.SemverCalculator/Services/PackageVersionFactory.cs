using System.ComponentModel.Composition;
using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Factory responsible for loading the correct package version services.
/// </summary>
[Export(typeof(IPackageVersionFactory))]
[method: ImportingConstructor]
internal class PackageVersionFactory(
    IPackageVersionService packageVersionService //IEnumerable<IPackageVersionService> packageVersionServices
) : IPackageVersionFactory
{
    /// <summary>
    /// Load the package version service.
    /// </summary>
    /// <param name="packageManager">Package manager.</param>
    /// <returns></returns>
    public IPackageVersionService Load(PackageManager packageManager)
    {
        return packageVersionService;
        //return packageVersionServices.FirstOrDefault(x => x.PackageManager == packageManager)
        //    ?? throw new NotSupportedException();
    }
}
