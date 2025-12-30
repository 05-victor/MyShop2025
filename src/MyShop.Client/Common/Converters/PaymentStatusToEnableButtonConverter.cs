using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter to enable/disable button based on payment status
/// Returns true (enabled) if PaymentStatus equals "UNPAID"
/// </summary>
public class PaymentStatusToEnableButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string paymentStatus)
        {
            // Enable button for "Unpaid" or "UNPAID" status
            return paymentStatus.Equals("UNPAID", StringComparison.OrdinalIgnoreCase) ||
                   paymentStatus.Equals("Unpaid", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
