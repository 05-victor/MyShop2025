using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter to transform string URL into ImageSource for WinUI3.
/// Supports absolute URLs (http/https), ms-appx:/// paths, and local file paths.
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Input value: '{value}' (type: {value?.GetType().Name ?? "null"})");
        
        if (value is not string imageUrl || string.IsNullOrWhiteSpace(imageUrl))
        {
            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Returning null - empty or not string");
            return null;
        }

        try
        {
            // If absolute URL (http/https)
            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Loading HTTP URL: {imageUrl}");
                return new BitmapImage(new Uri(imageUrl));
            }

            // If local file path (absolute path like C:\...)
            if (Path.IsPathRooted(imageUrl) && File.Exists(imageUrl))
            {
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Loading local file: {imageUrl}");
                return new BitmapImage(new Uri(imageUrl));
            }

            // If ms-appx:/// path - convert to actual file path for unpackaged app
            if (imageUrl.StartsWith("ms-appx:///", StringComparison.OrdinalIgnoreCase))
            {
                // Extract relative path from ms-appx:///
                var relativePath = imageUrl.Substring("ms-appx:///".Length);
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var fullPath = Path.Combine(baseDir, relativePath.Replace('/', Path.DirectorySeparatorChar));

                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] ms-appx path detected");
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] BaseDir: {baseDir}");
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] FullPath: {fullPath}");
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] File.Exists: {File.Exists(fullPath)}");

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage(new Uri(fullPath));
                    System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] SUCCESS - Created BitmapImage");
                    return bitmap;
                }
                
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] File not found: {fullPath}");
                return null;
            }

            // If relative path, try to resolve from app directory
            var appRelativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(appRelativePath))
            {
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Loading relative path: {appRelativePath}");
                return new BitmapImage(new Uri(appRelativePath));
            }

            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Could not resolve path: {imageUrl}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] EXCEPTION: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack is not supported for StringToImageSourceConverter");
    }
}