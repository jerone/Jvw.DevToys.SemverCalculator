using System.Reflection;
using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Tests.Converters;

/// <summary>
/// DevToys Button converter used for Verify tool.
/// Replace `OnClickAction` value with a placeholder, instead of delegate details.
/// </summary>
public class DevToysButtonConverter : WriteOnlyJsonConverter<IUIButton>
{
    public override void Write(VerifyJsonWriter writer, IUIButton value)
    {
        writer.WriteStartObject();

        var type = value.GetType();
        var props = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            var val = prop.GetValue(value);
            if (prop.Name == nameof(IUIButton.OnClickAction) && val is not null)
            {
                writer.WriteMember(value, "{has click action}", prop.Name);
            }
            else
            {
                writer.WritePropertyName(prop.Name);
                writer.Serializer.Serialize(writer, val);
            }
        }

        writer.WriteEndObject();
    }
}
