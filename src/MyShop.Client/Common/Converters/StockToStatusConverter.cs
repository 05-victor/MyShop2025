using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts stock quantity to a status string (In Stock / Low Stock / Out of Stock)
/// </summary>
public class StockToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            return stock switch
            {
                0 => "Out of Stock",
                < 10 => "Low Stock",
                _ => "In Stock"
            };
        }
        return "In Stock";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
