using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Views.Converters;

/// <summary>
/// Converter chuyển đổi null thành Visibility
/// null → Collapsed, not null → Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack not supported for NullToVisibilityConverter");
    }
}
