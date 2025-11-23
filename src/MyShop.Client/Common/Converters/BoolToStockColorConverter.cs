using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts a boolean stock availability to a color brush
/// True (in stock) -> Green, False (out of stock) -> Red
/// </summary>
public partial class BoolToStockColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isInStock)
        {
            return isInStock 
                ? new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)) // #10B981 Green
                : new SolidColorBrush(Color.FromArgb(255, 220, 38, 38));  // #DC2626 Red
        }
        return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // #6B7280 Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
