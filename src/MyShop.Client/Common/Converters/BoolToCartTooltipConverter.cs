using Microsoft.UI.Xaml.Data;

namespace MyShop.Client.Common.Converters
{
    /// <summary>
    /// Converts CanAddToCart boolean to tooltip message
    /// </summary>
    public class BoolToCartTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool canAddToCart)
            {
                return canAddToCart
                    ? "Add this product to your shopping cart"
                    : "Please verify your email to start shopping";
            }
            return "Add to Cart";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
