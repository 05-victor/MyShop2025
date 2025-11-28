using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter để chuyển đổi string URL thành ImageSource cho WinUI3
/// Hỗ trợ cả URL tuyệt đối (http/https), ms-appx:/// và relative paths
/// Đặc biệt hỗ trợ unpackaged apps
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string imageUrl || string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        try
        {
            // Case 1: Absolute file path (C:\..., D:\...)
            if (Path.IsPathRooted(imageUrl) && File.Exists(imageUrl))
            {
                var bitmap = new BitmapImage();
                bitmap.UriSource = new Uri(imageUrl);
                return bitmap;
            }

            // Case 2: Absolute URL (http/https)
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                return new BitmapImage(new Uri(imageUrl));
            }

            // Case 3: ms-appx:/// URI - convert to file path for unpackaged app
            if (imageUrl.StartsWith("ms-appx:///"))
            {
                // For unpackaged apps, convert ms-appx:/// to actual file path
                var relativePath = imageUrl.Replace("ms-appx:///", "");
                var baseDir = AppContext.BaseDirectory;
                var fullPath = Path.Combine(baseDir, relativePath);

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.UriSource = new Uri(fullPath);
                    return bitmap;
                }

                // Fallback: try ms-appx URI directly (may work in some cases)
                System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] File not found at '{fullPath}', trying ms-appx URI directly");
                return new BitmapImage(new Uri(imageUrl));
            }

            // Case 4: Relative path without ms-appx prefix
            var baseDirectory = AppContext.BaseDirectory;
            var absolutePath = Path.Combine(baseDirectory, imageUrl.TrimStart('/').Replace('/', '\\'));
            
            if (File.Exists(absolutePath))
            {
                var bitmap = new BitmapImage();
                bitmap.UriSource = new Uri(absolutePath);
                return bitmap;
            }

            // Final fallback: try as ms-appx URI
            var msAppxUri = $"ms-appx:///{imageUrl.TrimStart('/')}";
            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Trying ms-appx URI: {msAppxUri}");
            return new BitmapImage(new Uri(msAppxUri));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StringToImageSourceConverter] Failed to convert '{imageUrl}': {ex.Message}");
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack is not supported for StringToImageSourceConverter");
    }
}
