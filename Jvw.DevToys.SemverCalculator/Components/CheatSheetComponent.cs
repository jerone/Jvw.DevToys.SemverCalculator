using DevToys.Api;
using static DevToys.Api.GUI;
using R = Jvw.DevToys.SemverCalculator.Resources.Resources;

namespace Jvw.DevToys.SemverCalculator.Components;

/// <summary>
/// Cheat sheet component.
/// </summary>
internal class CheatSheetComponent
{
    /// <summary>
    /// Columns for the cheat sheet.
    /// </summary>
    internal static readonly string[] Columns =
    [
        R.CheatSheetColumnSyntaxTitle,
        R.CheatSheetColumnExampleTitle,
        R.CheatSheetColumnDescriptionTitle
    ];

    /// <summary>
    /// Rows for the cheat sheet.
    /// </summary>
    internal static readonly IUIDataGridRow[] Rows =
    [
        CreateTitleRow(R.CheatSheetMajorMinorPatchTitle),
        CreateRow("MAJOR", "2.0.0", R.CheatSheetMajorDescription),
        CreateRow("MINOR", "1.2.0", R.CheatSheetMinorDescription),
        CreateRow("PATCH", "1.2.3", R.CheatSheetPatchDescription),
        CreateTitleRow(R.CheatSheetExplanationTitle),
        CreateRow("0.x.x", "0.0.1", R.CheatSheetInitialDevelopmentDescription),
        CreateRow("1.x.x", "1.0.0", R.CheatSheetFirstPublicDescription),
        CreateTitleRow(R.CheatSheetSyntaxTitle),
        CreateRow(">", ">1.2.3", R.CheatSheetGreaterThanDescription),
        CreateRow("<", "<1.2.3", R.CheatSheetLessThanDescription),
        CreateRow(">=", ">=1.2.3", R.CheatSheetGreaterThanOrEqualDescription),
        CreateRow("<=", "<=1.2.3", R.CheatSheetLessThanOrEqualDescription),
        CreateRow("-", "1.2.3 - 2.3.4", R.CheatSheetBetweenDescription),
        CreateRow("~", "~1.2.3", R.CheatSheetReasonablyCloseDescription),
        CreateRow("^", "^1.2.3", R.CheatSheetCompatibleWithDescription),
        CreateRow("~x.x", "~1.2", R.CheatSheetAnyStartingWithDescription),
        CreateRow("^x.x", "^1.2", R.CheatSheetAnyCompatibleWithDescription),
        CreateRow("*", "*", R.CheatSheetAnyDescription),
    ];

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
