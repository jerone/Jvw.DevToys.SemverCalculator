using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Models;
using static DevToys.Api.GUI;
using R = Jvw.DevToys.SemverCalculator.Resources.Resources;

namespace Jvw.DevToys.SemverCalculator.Components;

/// <summary>
/// Cheat sheet component.
/// </summary>
internal static class CheatSheetComponent
{
    internal static IUIDataGrid CheatSheetNpm() =>
        DataGrid(Ids.CheatSheetNpmDataGrid)
            .Title(R.CheatSheetTitle)
            .ForbidSelectItem()
            .Extendable()
            .WithColumns(
                R.CheatSheetColumnSyntaxTitle,
                R.CheatSheetColumnExampleTitle,
                R.CheatSheetColumnDescriptionTitle
            )
            .WithRows(
                CreateTitleRow(R.CheatSheetMajorMinorPatchTitle),
                CreateRow("MAJOR", "2.0.0", R.CheatSheetMajorDescription),
                CreateRow("MINOR", "1.2.0", R.CheatSheetMinorDescription),
                CreateRow("PATCH", "1.2.3", R.CheatSheetPatchDescription),
                CreateTitleRow(R.CheatSheetExplanationTitle),
                CreateRow("0.x.x", "0.0.1", R.CheatSheetInitialDevelopmentDescription),
                CreateRow("1.x.x", "1.0.0", R.CheatSheetFirstPublicDescription),
                CreateTitleRow(R.CheatSheetSyntaxTitle),
                CreateRow(">", ">1.2.3", R.CheatSheetNpmGreaterThanDescription),
                CreateRow("<", "<1.2.3", R.CheatSheetNpmLessThanDescription),
                CreateRow(">=", ">=1.2.3", R.CheatSheetNpmGreaterThanOrEqualDescription),
                CreateRow("<=", "<=1.2.3", R.CheatSheetNpmLessThanOrEqualDescription),
                CreateRow("-", "1.2.3 - 2.3.4", R.CheatSheetNpmBetweenDescription),
                CreateRow("~", "~1.2.3", R.CheatSheetNpmReasonablyCloseDescription),
                CreateRow("^", "^1.2.3", R.CheatSheetNpmCompatibleWithDescription),
                CreateRow("~x.x", "~1.2", R.CheatSheetNpmAnyStartingWithDescription),
                CreateRow("^x.x", "^1.2", R.CheatSheetNpmAnyCompatibleWithDescription),
                CreateRow("*", "*", R.CheatSheetNpmAnyDescription)
            );

    internal static IUIDataGrid CheatSheetNuGet() =>
        DataGrid(Ids.CheatSheetNuGetDataGrid)
            .Title(R.CheatSheetTitle)
            .ForbidSelectItem()
            .Extendable()
            .WithColumns(
                R.CheatSheetColumnSyntaxTitle,
                R.CheatSheetColumnExampleTitle,
                R.CheatSheetColumnDescriptionTitle
            )
            .WithRows(
                CreateTitleRow(R.CheatSheetMajorMinorPatchTitle),
                CreateRow("MAJOR", "2.0.0", R.CheatSheetMajorDescription),
                CreateRow("MINOR", "1.2.0", R.CheatSheetMinorDescription),
                CreateRow("PATCH", "1.2.3", R.CheatSheetPatchDescription),
                CreateTitleRow(R.CheatSheetExplanationTitle),
                CreateRow("0.x.x", "0.0.1", R.CheatSheetInitialDevelopmentDescription),
                CreateRow("1.x.x", "1.0.0", R.CheatSheetFirstPublicDescription),
                CreateTitleRow(R.CheatSheetSyntaxTitle),
                CreateRow(
                    "1.0",
                    "x \u2265 1.0",
                    R.CheatSheetNuGetMinimumVersionInclusiveDescription
                ),
                CreateRow(
                    "[1.0,)",
                    "x \u2265 1.0",
                    R.CheatSheetNuGetMinimumVersionInclusiveDescription
                ),
                CreateRow("(1.0,)", "x > 1.0", R.CheatSheetNuGetMinimumVersionExclusiveDescription),
                CreateRow("[1.0]", "x == 1.0", R.CheatSheetNuGetExactVersionMatchDescription),
                CreateRow(
                    "(,1.0]",
                    "x \u2264 1.0",
                    R.CheatSheetNuGetMaximumVersionInclusiveDescription
                ),
                CreateRow("(,1.0)", "x < 1.0", R.CheatSheetNuGetMaximumVersionExclusiveDescription),
                CreateRow(
                    "[1.0,2.0]",
                    "1.0 \u2264 x \u2264 2.0",
                    R.CheatSheetNuGetExactRangeInclusiveDescription
                ),
                CreateRow(
                    "(1.0,2.0)",
                    "1.0 < x < 2.0",
                    R.CheatSheetNuGetExactRangeExclusiveDescription
                ),
                CreateRow(
                    "[1.0,2.0)",
                    "1.0 \u2264 x < 2.0",
                    R.CheatSheetNuGetMixedInclusiveMinExclusiveMaxVersionDescription
                ),
                CreateRow("(1.0)", "", R.CheatSheetNuGetInvalidDescription)
            );

    /// <summary>
    /// Create a row for the cheat sheet.
    /// </summary>
    /// <param name="syntax">Semver syntax.</param>
    /// <param name="example">Semver example.</param>
    /// <param name="description">Semver description.</param>
    /// <returns>Row.</returns>
    private static IUIDataGridRow CreateRow(string syntax, string example, string description) =>
        Row(
            null,
            Cell(Label().NeverWrap().Style(UILabelStyle.BodyStrong).Text($"\t{syntax}\t")),
            Cell(Label().NeverWrap().Style(UILabelStyle.BodyStrong).Text($"\t{example}\t")),
            Cell(Label().NeverWrap().Text(description))
        );

    /// <summary>
    /// Create a title row for the cheat sheet.
    /// </summary>
    /// <param name="title">Row title.</param>
    /// <returns>Title row.</returns>
    private static IUIDataGridRow CreateTitleRow(string title) =>
        Row(
            null,
            Cell(Label().NeverWrap().Style(UILabelStyle.Subtitle).Text(title)),
            Cell(Label()),
            Cell(Label())
        );
}
