using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Jvw.DevToys.SemverCalculator.Models;
using Microsoft.Extensions.Logging;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Fetch package versions from the NPM registry.
/// </summary>
/// <param name="logger">DevToys logger.</param>
internal class NpmService(ILogger logger)
{
    /// <summary>
    /// JSON serializer options.
    /// </summary>
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Fetch package versions from the NPM registry.
    /// </summary>
    /// <param name="packageName">Package name.</param>
    /// <returns>Package versions.</returns>
    public async Task<PackageJson?> FetchPackage(string packageName)
    {
        logger.LogInformation("Fetching package \"{PackageName}\"...", packageName);

        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.npm.install-vl+json");
            client.DefaultRequestHeaders.Add("User-Agent", "Jvw.DevToys.SemverCalculator");

            var response = await client.GetAsync($"https://registry.npmjs.org/{packageName}/");

            response.EnsureSuccessStatusCode();

            logger.LogInformation("Fetched package \"{PackageName}\".", packageName);

            var contentStream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<PackageJson>(
                contentStream,
                _jsonSerializerOptions
            );

            return result;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
#pragma warning disable S6667
            logger.LogWarning("Package \"{PackageName}\" not found.", packageName);
#pragma warning restore S6667
            Debug.WriteLine(e.Message);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch package \"{PackageName}\".", packageName);
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
