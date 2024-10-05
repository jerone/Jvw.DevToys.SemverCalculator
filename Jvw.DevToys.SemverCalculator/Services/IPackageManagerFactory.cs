using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <inheritdoc cref="PackageManagerFactory" />
internal interface IPackageManagerFactory
{
    /// <inheritdoc cref="PackageManagerFactory.Load" />
    IPackageManagerService Load(PackageManager packageManager);
}
