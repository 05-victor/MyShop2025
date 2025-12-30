using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter to transform string URL into Uri for WinUI3.
/// Handles null/empty strings gracefully by returning null instead of crashing.
/// Supports absolute URLs (http/https) and local file paths.
/// </summary>
public class StringToUriConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        System.Diagnostics.Debug.WriteLine($"[StringToUriConverter] Input value: '{value}' (type: {value?.GetType().Name ?? "null"})");

        if (value is not string urlString || string.IsNullOrWhiteSpace(urlString))
        {
            System.Diagnostics.Debug.WriteLine($"[StringToUriConverter] Returning null - empty or not string");
            return null;
        }

        try
        {
            var uri = new Uri(urlString);
            System.Diagnostics.Debug.WriteLine($"[StringToUriConverter] Successfully converted to Uri: {uri.AbsoluteUri}");
            return uri;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StringToUriConverter] ❌ Failed to convert '{urlString}' to Uri: {ex.Message}");
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Uri uri)
        {
            return uri.AbsoluteUri;
        }
        return string.Empty;
    }
}
