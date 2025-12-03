using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter for button background - active (true) = blue, inactive (false) = transparent
/// </summary>
public class BoolToButtonBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isActive = value is bool boolValue && boolValue;
        
        if (isActive)
        {
            // Active state - blue background
            return new SolidColorBrush(Color.FromArgb(255, 26, 77, 143)); // #1A4D8F
        }
        else
        {
            // Inactive state - transparent
            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for button foreground - active (true) = white, inactive (false) = gray
/// </summary>
public class BoolToButtonForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isActive = value is bool boolValue && boolValue;
        
        if (isActive)
        {
            // Active state - white text
            return new SolidColorBrush(Colors.White);
        }
        else
        {
            // Inactive state - gray text
            return new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // #6B7280
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
