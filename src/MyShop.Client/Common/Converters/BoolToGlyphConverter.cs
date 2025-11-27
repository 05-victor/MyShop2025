using Microsoft.UI.Xaml.Data;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts a boolean value to a glyph/icon character based on parameter specification.
/// Parameter format: "FalseGlyph|TrueGlyph" (e.g., "&#xE8D8;|&#xE73E;")
/// If no parameter provided: False="❌", True="✔️"
/// </summary>
public partial class BoolToGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        
        // Parse parameter if provided
        if (parameter is string glyphSpec && !string.IsNullOrEmpty(glyphSpec))
        {
            var glyphs = glyphSpec.Split('|');
            if (glyphs.Length == 2)
            {
                return boolValue ? glyphs[1] : glyphs[0];
            }
        }
        
        // Default glyphs
        return boolValue ? "✔️" : "❌";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
