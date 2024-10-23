using System.Reflection;
using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Tests.Converters;

/// <summary>
/// DevToys element converter used for Verify tool.
/// </summary>
public class DevToysElementConverter : WriteOnlyJsonConverter<IUIElement>
{
    public override void Write(VerifyJsonWriter writer, IUIElement element)
    {
        writer.WriteStartObject();

        var type = element.GetType();

        // Prepend type name.
        writer.WriteMember(element, type.Name, "$type");

        var props = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            var name = prop.Name;
            var val = prop.GetValue(element);

            // Replace button `OnClickAction` value with a placeholder, instead of delegate details.
            if (IsButtonClickAction(element, name, val))
            {
                writer.WriteMember(element, "{has click action}", prop.Name);
            }
            // Replace info-bar `OnCloseAction` value with a placeholder, instead of delegate details.
            else if (IsInfoBarCloseAction(element, name, val))
            {
                writer.WriteMember(element, "{has close action}", prop.Name);
            }
            else if (IsSelectDropDownListItemSelectedAction(element, name, val))
            {
                writer.WriteMember(element, "{has selected item action}", prop.Name);
            }
            else
            {
                writer.WritePropertyName(name);
                writer.Serializer.Serialize(writer, val);
            }
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Detect if property is `OnClickAction` from a button.
    /// </summary>
    /// <param name="element">Element.</param>
    /// <param name="name">Property name.</param>
    /// <param name="val">Property value.</param>
    /// <returns>Whether property is `OnClickAction` from a button.</returns>
    private static bool IsButtonClickAction(IUIElement element, string name, object? val)
    {
        return element is IUIButton && name == nameof(IUIButton.OnClickAction) && val is not null;
    }

    /// <summary>
    /// Detect if property is `OnCloseAction` from an info-bar.
    /// </summary>
    /// <param name="element">Element.</param>
    /// <param name="name">Property name.</param>
    /// <param name="val">Property value.</param>
    /// <returns>Whether property is `OnCloseAction` from an info-bar.</returns>
    private static bool IsInfoBarCloseAction(IUIElement element, string name, object? val)
    {
        return element is IUIInfoBar && name == nameof(IUIInfoBar.OnCloseAction) && val is not null;
    }

    /// <summary>
    /// Detect if property is `OnItemSelectedAction` from a select dropdown list.
    /// </summary>
    /// <param name="element">Element.</param>
    /// <param name="name">Property name.</param>
    /// <param name="val">Property value.</param>
    /// <returns>Whether property is `OnItemSelectedAction` from a select dropdown list.</returns>
    private static bool IsSelectDropDownListItemSelectedAction(
        IUIElement element,
        string name,
        object? val
    )
    {
        return element is IUISelectDropDownList
            && name == nameof(IUISelectDropDownList.OnItemSelectedAction)
            && val is not null;
    }
}
