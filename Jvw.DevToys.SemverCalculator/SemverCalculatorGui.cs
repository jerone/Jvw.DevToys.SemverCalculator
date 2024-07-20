using System.ComponentModel.Composition;
using DevToys.Api;
using static DevToys.Api.GUI;

namespace Jvw.DevToys.SemverCalculator;

[Export(typeof(IGuiTool))]
[Name("Jvw.DevToys.SemverCalculator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uf20a',
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(SemverCalculatorAssemblyIdentifier),
    ResourceManagerBaseName = "Jvw.DevToys.SemverCalculator." + nameof(SemverCalculatorResources),
    ShortDisplayTitleResourceName = nameof(SemverCalculatorResources.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(SemverCalculatorResources.LongDisplayTitle),
    DescriptionResourceName = nameof(SemverCalculatorResources.Description),
    AccessibleNameResourceName = nameof(SemverCalculatorResources.AccessibleName)
)]
internal sealed class SemverCalculatorGui : IGuiTool
{
    private enum GridColumn
    {
        Stretch
    }

    private enum GridRow
    {
        Settings,
        Results
    }

    public UIToolView View =>
        new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Settings, Auto),
                    (GridRow.Results, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns((GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Cells(
                    Cell(
                        GridRow.Settings,
                        GridColumn.Stretch,
                        Stack()
                            .Vertical()
                            .WithChildren(
                                SingleLineTextInput().Title("Package name"),
                                SingleLineTextInput().Title("Version range"),
                                Label().Text("  "), // Padding.
                                Button().AccentAppearance().Text("List versions")
                            )
                    ),
                    Cell(
                        GridRow.Results,
                        GridColumn.Stretch,
                        Stack()
                            .Vertical()
                            .WithChildren(Wrap().LargeSpacing().WithChildren(Button().Text("A")))
                    )
                )
        );

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }
}
