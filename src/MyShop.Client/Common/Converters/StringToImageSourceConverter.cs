using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter để chuyển đổi string URL thành ImageSource cho WinUI3
/// Hỗ trợ cả URL tuyệt đối (http/https) và relative paths
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
                // Nếu là absolute URL (http/https)
                if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    return new BitmapImage(new Uri(imageUrl));
                }

                // Nếu là relative path (ms-appx:///)
                if (!imageUrl.StartsWith("ms-appx:///"))
                {
                    imageUrl = $"ms-appx:///{imageUrl.TrimStart('/')}";
                }

                return new BitmapImage(new Uri(imageUrl));
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
