using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI;

namespace MyShop.Client.Common.Converters
{
    /// <summary>
    /// Converts password strength string to color
    /// </summary>
    public class PasswordStrengthToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string strength)
            {
                return strength switch
                {
                    "Weak" => Color.FromArgb(255, 220, 38, 38),      // Red (#DC2626)
                    "Medium" => Color.FromArgb(255, 245, 158, 11),   // Orange (#F59E0B)
                    "Strong" => Color.FromArgb(255, 16, 185, 129),   // Green (#10B981)
                    _ => Color.FromArgb(255, 156, 163, 175)          // Gray (#9CA3AF)
                };
            }
            return Color.FromArgb(255, 156, 163, 175);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
