using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts between nullable DateTime and DateTimeOffset for DatePicker binding
/// </summary>
public class NullableDateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            return new DateTimeOffset(dateTime);
        }

        // Return today's date as default if value is null
        return new DateTimeOffset(DateTime.Today);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.DateTime;
        }

        return null;
    }
}
