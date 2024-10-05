using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <inheritdoc cref="PackageVersionFactory" />
internal interface IPackageVersionFactory
{
    /// <inheritdoc cref="PackageVersionFactory.Load" />
    IPackageVersionService Load(PackageManager packageManager);
}
