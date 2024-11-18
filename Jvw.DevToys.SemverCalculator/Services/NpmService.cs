using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Converters;
using Jvw.DevToys.SemverCalculator.Enums;
using Microsoft.Extensions.Logging;
using Semver;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Fetch package versions from the NPM registry.
/// </summary>
[Export(typeof(IPackageManagerService))]
internal class NpmService : IPackageManagerService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private List<SemVersion> _versions = [];
    private SemVersionRange? _range;

    /// <inheritdoc cref="IPackageManagerService.PackageManager" />
    public PackageManager PackageManager => PackageManager.Npm;

    /// <summary>
    /// Fetch package versions from the NPM registry.
    /// </summary>
    /// <remarks>Empty constructor is required for MEF.</remarks>
    [ExcludeFromCodeCoverage(
        Justification = "Empty constructor is required for MEF. Parameterized constructor is for testing."
    )]
    public NpmService()
        : this(new HttpClient()) { }

    /// <summary>
    /// Fetch package versions from the NPM registry.
    /// </summary>
    /// <param name="httpClient">HTTP client.</param>
    /// <param name="logger">DevToys logger.</param>
    public NpmService(HttpClient httpClient, ILogger? logger = null)
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
                if (SemVersion.TryParse(v, SemVersionStyles.Strict, out var version))
                {
                    return version;
                }

                _logger.LogWarning("Invalid version string: {VersionString}", v);
                return null;
            })
            .Where(v => v != null)
            .Cast<SemVersion>()
            .ToList();
        _versions.Sort(SemVersion.SortOrderComparer);
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

            yield return (version.ToString(), _range != null && _range.Contains(version));
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
    private static bool TryParseRange(string value, out SemVersionRange? versionRange)
    {
        return SemVersionRange.TryParseNpm(value, true, out versionRange);
    }

    /// <inheritdoc cref="IPackageManagerService.FetchPackage" />
    public async Task<List<string>?> FetchPackage(string packageName)
    {
        _logger.LogInformation("Fetching package \"{PackageName}\"...", packageName);

        try
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.npm.install-vl+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jvw.DevToys.SemverCalculator");

            var url = $"https://registry.npmjs.org/{packageName}/";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Fetched package \"{PackageName}\".", packageName);

            var contentStream = await response.Content.ReadAsStreamAsync();

            var packageData = await JsonSerializer.DeserializeAsync<PackageJson>(
                contentStream,
                _jsonSerializerOptions
            );

            _logger.LogInformation("Extracted package: {PackageName}", packageName);

            return packageData?.Versions;
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

    private sealed class PackageJson
    {
        [JsonConverter(typeof(DictionaryToKeysListConverter))]
        public required List<string> Versions { get; set; } = [];
    }
}
