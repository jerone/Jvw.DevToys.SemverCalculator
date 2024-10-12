using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Enums;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Fetch package versions from the NuGet registry.
/// </summary>
[Export(typeof(IPackageManagerService))]
internal class NuGetService : IPackageManagerService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private List<NuGetVersion> _versions = [];
    private VersionRange? _range;

    /// <inheritdoc cref="IPackageManagerService.PackageManager" />
    public PackageManager PackageManager => PackageManager.NuGet;

    /// <summary>
    /// Fetch package versions from the NuGet registry.
    /// </summary>
    /// <remarks>Empty constructor is required for MEF.</remarks>
    [ExcludeFromCodeCoverage(
        Justification = "Empty constructor is required for MEF. Parameterized constructor is for testing."
    )]
    public NuGetService()
        : this(new HttpClient()) { }

    /// <summary>
    /// Fetch package versions from the NuGet registry.
    /// </summary>
    /// <param name="httpClient">HTTP client.</param>
    /// <param name="logger">DevToys logger.</param>
    public NuGetService(HttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger ?? this.Log();
    }

    /// <inheritdoc cref="IPackageManagerService.SetVersions" />
    public void SetVersions(List<string> versions)
    {
        _versions = versions
            .Select(v =>
            {
                NuGetVersion.TryParseStrict(v, out var version);
                return version;
            })
            .ToList();
        _versions.Sort(VersionComparer.Default);
    }

    /// <inheritdoc cref="IPackageManagerService.GetVersions" />
    public IEnumerable<(string version, bool match)> GetVersions(bool includePreReleases)
    {
        foreach (var version in _versions)
        {
            if (!includePreReleases && version.IsPrerelease)
            {
                continue;
            }

            yield return (version.ToString(), _range != null && _range.Satisfies(version));
        }
    }

    /// <inheritdoc cref="IPackageManagerService.TryParseRange" />
    public bool TryParseRange(string value)
    {
        var valid = TryParseRange(value, out _range);
        if (!valid)
            _range = null;
        return valid;
    }

    /// <inheritdoc cref="IPackageManagerService.IsValidRange" />
    public bool IsValidRange(string value)
    {
        return TryParseRange(value, out _);
    }

    /// <summary>
    /// Try to parse range.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <param name="versionRange">Parsed version range.</param>
    /// <returns>Whether value is valid range.</returns>
    private static bool TryParseRange(string value, out VersionRange versionRange)
    {
        return VersionRange.TryParse(value, true, out versionRange);
    }

    /// <inheritdoc cref="IPackageManagerService.FetchPackage"/>
    public async Task<List<string>?> FetchPackage(string packageName)
    {
        _logger.LogInformation("Fetching package: {PackageName}", packageName);

        try
        {
            var url =
                $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLowerInvariant()}/index.json";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Fetched package \"{PackageName}\".", packageName);

            var contentStream = await response.Content.ReadAsStreamAsync();

            var packageData = await JsonSerializer.DeserializeAsync<NuGetPackageData>(
                contentStream,
                _jsonSerializerOptions
            );

            if (packageData == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize package data for: {PackageName}",
                    packageName
                );
                return null;
            }

            _logger.LogInformation("Successfully fetched package: {PackageName}", packageName);

            return packageData.Versions;
        }
        catch (HttpRequestException e)
            when (e.StatusCode == HttpStatusCode.NotFound && e.GetType().Name != "MockException")
        {
#pragma warning disable S6667
            _logger.LogWarning("Package \"{PackageName}\" not found.", packageName);
#pragma warning restore S6667
            Debug.WriteLine(e.Message);
            return null;
        }
        catch (Exception e) when (e.GetType().Name != "MockException")
        {
            _logger.LogError(e, "Failed to fetch package \"{PackageName}\".", packageName);
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private sealed class NuGetPackageData
    {
        // ReSharper disable once PropertyCanBeMadeInitOnly.Local
        public required List<string> Versions { get; set; } = [];
    }
}
