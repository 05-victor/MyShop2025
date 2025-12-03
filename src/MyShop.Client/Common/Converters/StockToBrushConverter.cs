using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts stock quantity to a color brush for status display
/// </summary>
public class StockToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            return stock switch
            {
                0 => new SolidColorBrush(Color.FromArgb(255, 220, 38, 38)),   // Red - Out of Stock
                < 10 => new SolidColorBrush(Color.FromArgb(255, 245, 158, 11)), // Orange - Low Stock
                _ => new SolidColorBrush(Color.FromArgb(255, 16, 185, 129))  // Green - In Stock
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // Gray - Unknown
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
