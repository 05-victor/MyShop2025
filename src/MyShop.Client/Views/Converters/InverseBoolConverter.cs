using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Views.Converters;

/// <summary>
/// Converter đảo ngược giá trị bool
/// true → false, false → true
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }
}
