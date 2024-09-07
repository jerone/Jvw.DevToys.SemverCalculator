using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Tests.Converters;

/// <summary>
/// DevToys DataGridCell converter used for Verify tool.
/// </summary>
public class DevToysDataGridCellConverter : WriteOnlyJsonConverter<IUIDataGridCell>
{
    public override void Write(VerifyJsonWriter writer, IUIDataGridCell value)
    {
        writer.WriteStartObject();
        writer.WriteMember(value, value.UIElement, value.UIElement!.GetType().Name);
        writer.WriteEndObject();
    }
}
