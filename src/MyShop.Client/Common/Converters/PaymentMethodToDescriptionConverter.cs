using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts payment method name to appropriate description text
/// </summary>
public class PaymentMethodToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string paymentMethod)
        {
            return paymentMethod switch
            {
                "Credit / Debit Card" => "You'll be redirected to secure card payment page",
                "QR Code / Banking App" => "Your order will be confirmed once seller verifies payment",
                "Cash on Delivery (COD)" => "You'll pay when receiving your order",
                _ => "Complete your order"
            };
        }
        return "Complete your order";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
