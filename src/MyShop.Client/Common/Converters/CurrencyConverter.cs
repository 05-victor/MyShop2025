using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace MyShop.Client.Common.Converters
{
    /// <summary>
    /// Converts decimal values to formatted currency strings (VND)
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "₫0";

            try
            {
                decimal amount;
                
                if (value is decimal decimalValue)
                    amount = decimalValue;
                else if (value is double doubleValue)
                    amount = (decimal)doubleValue;
                else if (value is float floatValue)
                    amount = (decimal)floatValue;
                else if (value is int intValue)
                    amount = intValue;
                else if (value is long longValue)
                    amount = longValue;
                else if (decimal.TryParse(value.ToString(), out var parsed))
                    amount = parsed;
                else
                    return "₫0";

                // Format with Vietnamese Dong symbol and thousand separators
                // Example: 1234567.89 -> "₫1,234,568"
                return amount.ToString("₫#,##0", CultureInfo.InvariantCulture);
            }
            catch
            {
                return "₫0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return 0m;

            try
            {
                // Remove currency symbol and parse
                var cleanValue = value.ToString()!
                    .Replace("₫", "")
                    .Replace(",", "")
                    .Replace(" ", "")
                    .Trim();

                if (decimal.TryParse(cleanValue, out var result))
                    return result;

                return 0m;
            }
            catch
            {
                return 0m;
            }
        }
    }
}
