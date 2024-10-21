using System.ComponentModel.Composition;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Services;

namespace Jvw.DevToys.SemverCalculator.Detectors;

/// <summary>
/// Data-type detector for DevToys that can detect SemVer values and ranges.
/// </summary>
/// <param name="packageManagerServices">Package manager services.</param>
[Export(typeof(IDataTypeDetector))]
[DataTypeName(Name, baseName: PredefinedCommonDataTypeNames.Text)]
[method: ImportingConstructor]
internal sealed class SemVersionRangeDataTypeDetector(
    [ImportMany] IEnumerable<IPackageManagerService> packageManagerServices
) : IDataTypeDetector
{
    internal const string Name = "SemVersionRange";

    /// <inheritdoc cref="IDataTypeDetector.TryDetectDataAsync" />
    public ValueTask<DataDetectionResult> TryDetectDataAsync(
        object rawData,
        DataDetectionResult? resultFromBaseDetector,
        CancellationToken cancellationToken
    )
    {
        if (rawData is string dataString && !string.IsNullOrEmpty(dataString))
        {
            foreach (var packageManagerService in packageManagerServices)
            {
                if (packageManagerService.IsValidRange(dataString))
                {
                    return ValueTask.FromResult(new DataDetectionResult(Success: true, dataString));
                }
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
