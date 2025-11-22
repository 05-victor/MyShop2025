using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter format string với parameter
/// Usage: Converter={StaticResource StringFormatConverter}, ConverterParameter='Hello {0}!'
/// </summary>
public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string format && !string.IsNullOrEmpty(format))
        {
            try
            {
                return string.Format(format, value);
            }
            catch
            {
                return value?.ToString() ?? string.Empty;
            }
        }

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack not supported for StringFormatConverter");
    }
}
