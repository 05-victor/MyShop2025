using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Converters;

/// <summary>
/// Converts a status string to Visibility by comparing with a parameter value.
/// Example: Status="Shipped" + Parameter="Shipped" => Visibility.Visible
/// </summary>
public class StatusEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
        {
            return Visibility.Collapsed;
        }

        var statusValue = value.ToString();
        var targetStatus = parameter.ToString();

        return statusValue.Equals(targetStatus, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
