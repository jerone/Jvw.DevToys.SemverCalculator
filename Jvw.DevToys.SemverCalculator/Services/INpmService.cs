using Jvw.DevToys.SemverCalculator.Models;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Fetch package versions from the NPM registry.
/// </summary>
internal interface INpmService
{
    /// <summary>
    /// Fetch package versions from the NPM registry.
    /// </summary>
    /// <param name="packageName">Package name.</param>
    /// <returns>Package versions.</returns>
    Task<PackageJson?> FetchPackage(string packageName);
}
