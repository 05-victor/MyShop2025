using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Globalization;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts a hex color string (e.g., "#10B981") to a SolidColorBrush
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
        {
            try
            {
                // Remove # if present
                hexColor = hexColor.TrimStart('#');

                if (hexColor.Length == 6)
                {
                    var r = byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                    var g = byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                    var b = byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
                    return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                }
                else if (hexColor.Length == 8)
                {
                    var a = byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                    var r = byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                    var g = byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
                    var b = byte.Parse(hexColor.Substring(6, 2), NumberStyles.HexNumber);
                    return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                }
            }
            catch
            {
                // Fall through to default
            }
        }

        // Default gray color
        return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
