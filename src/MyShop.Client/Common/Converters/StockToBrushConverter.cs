using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts stock quantity to a color brush for status display
/// Supports parameter: "Background" or "Foreground" to get appropriate colors
/// </summary>
public class StockToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int stock)
        {
            return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // Gray - Unknown
        }

        var colorType = parameter as string ?? "Background";
        
        return (stock, colorType) switch
        {
            // Out of Stock (0)
            (0, "Background") => new SolidColorBrush(Color.FromArgb(255, 254, 226, 226)), // #FEE2E2
            (0, "Foreground") => new SolidColorBrush(Color.FromArgb(255, 185, 28, 28)),   // #B91C1C
            
            // Low Stock (< 10)
            (< 10, "Background") => new SolidColorBrush(Color.FromArgb(255, 254, 243, 199)), // #FEF3C7
            (< 10, "Foreground") => new SolidColorBrush(Color.FromArgb(255, 180, 83, 9)),    // #B45309
            
            // In Stock (>= 10)
            (_, "Background") => new SolidColorBrush(Color.FromArgb(255, 209, 250, 229)), // #D1FAE5
            (_, "Foreground") => new SolidColorBrush(Color.FromArgb(255, 5, 150, 105)),   // #059669
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
