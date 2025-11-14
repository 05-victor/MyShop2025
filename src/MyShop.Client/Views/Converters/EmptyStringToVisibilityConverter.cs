using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Views.Converters;

/// <summary>
/// Converter chuyển đổi empty string thành Visibility
/// empty/null → Collapsed, has value → Visible
/// </summary>
public class EmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack not supported for EmptyStringToVisibilityConverter");
    }
}
