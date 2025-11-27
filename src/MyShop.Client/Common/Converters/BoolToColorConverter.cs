using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts a boolean value to a color brush based on parameter specification.
/// Parameter format: "FalseColor|TrueColor" (e.g., "#FEE2E2|#D1FAE5")
/// If no parameter provided: False=Red, True=Green
/// </summary>
public partial class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        
        // Parse parameter if provided
        if (parameter is string colorSpec && !string.IsNullOrEmpty(colorSpec))
        {
            var colors = colorSpec.Split('|');
            if (colors.Length == 2)
            {
                var targetColor = boolValue ? colors[1] : colors[0];
                return ParseColorBrush(targetColor);
            }
        }
        
        // Default colors: False=Red, True=Green
        return boolValue 
            ? new SolidColorBrush(Color.FromArgb(255, 16, 185, 129))  // #10B981 Green
            : new SolidColorBrush(Color.FromArgb(255, 220, 38, 38));   // #DC2626 Red
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static SolidColorBrush ParseColorBrush(string colorString)
    {
        // Remove # if present
        colorString = colorString.TrimStart('#');
        
        // Parse hex color
        if (colorString.Length == 6)
        {
            byte r = System.Convert.ToByte(colorString.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(colorString.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(colorString.Substring(4, 2), 16);
            return new SolidColorBrush(Color.FromArgb(255, r, g, b));
        }
        
        // Fallback to gray
        return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // #6B7280
    }
}
