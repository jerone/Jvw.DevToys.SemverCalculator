﻿using System.ComponentModel.Composition;
using System.Text.Json;
using DevToys.Api;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput();
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput();
    private readonly IUIWrap _wrap = Wrap();
    private readonly IUIProgressRing _progressRing = ProgressRing();

    public SemverCalculatorGui()
    {
        _logger = this.Log();

#if DEBUG
        _packageNameInput.Text("@jerone/assert-includes");
        _versionRangeInput.Text("1.0.0");
#endif
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
                                _packageNameInput.Title("Package name"),
                                _versionRangeInput.Title("Version range"),
                                Label().Text("  "), // Padding.
                                Button()
                                    .AccentAppearance()
                                    .Text("List versions")
                                    .OnClick(OnButtonClick)
                            )
                    ),
                    Cell(
                        GridRow.Results,
                        GridColumn.Stretch,
                        Card(
                            Stack()
                                .Vertical()
                                .AlignHorizontally(UIHorizontalAlignment.Center)
                                .WithChildren(_progressRing, _wrap.LargeSpacing())
                        )
                    )
                )
        );

    private async ValueTask OnButtonClick()
    {
        _wrap.WithChildren([]);
        _progressRing.Show().StartIndeterminateProgress();

        var package = await FetchPackage(_packageNameInput.Text);
        if (package == null)
        {
            _progressRing.StopIndeterminateProgress().Hide();
            _wrap.WithChildren(Button().Text("No results"));
            return;
        }

        var list = new List<IUIElement>();
        foreach (var version in package.Versions)
        {
            IUIElement element = Button().Text(version);
            list.Add(element);
        }

        await Task.Delay(2000);

        _wrap.WithChildren([.. list]);
        _progressRing.StopIndeterminateProgress().Hide();
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // Not implemented.
    }

    private async Task<PackageJson?> FetchPackage(string packageName)
    {
        _logger.LogInformation($"Fetching package \"{packageName}\"...");
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.npm.install-vl+json");

            var response = await client.GetAsync($"https://registry.npmjs.org/{packageName}/");

            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Fetched packages \"{packageName}\".");

            var contentStream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<PackageJson>(
                contentStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to fetch package \"{packageName}\".");
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
