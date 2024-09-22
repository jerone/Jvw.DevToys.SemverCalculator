using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Models;
using Microsoft.Extensions.Logging;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Fetch package versions from the NPM registry.
/// </summary>
[Export(typeof(INpmService))]
internal class NpmService : INpmService
{
    /// <summary>
    /// JSON serializer options.
    /// </summary>
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

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

    /// <summary>
    /// Fetch package versions from the NPM registry.
    /// </summary>
    /// <param name="packageName">Package name.</param>
    /// <returns>Package versions.</returns>
    public async Task<PackageJson?> FetchPackage(string packageName)
    {
        _logger.LogInformation("Fetching package \"{PackageName}\"...", packageName);

        try
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.npm.install-vl+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jvw.DevToys.SemverCalculator");

            var response = await _httpClient.GetAsync($"https://registry.npmjs.org/{packageName}/");

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Fetched package \"{PackageName}\".", packageName);

            var contentStream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<PackageJson>(
                contentStream,
                _jsonSerializerOptions
            );

            return result;
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
}
