using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter để hiển thị empty state dựa trên số lượng items
/// Nếu count = 0, trả về Visible (hiển thị empty state)
/// Nếu count > 0, trả về Collapsed (ẩn empty state, hiển thị danh sách)
/// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// True để invert logic (hiển thị khi có items, ẩn khi empty)
        /// </summary>
        public bool IsInverted { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count = 0;

            if (value is int intValue)
            {
                count = intValue;
            }
            else if (value is System.Collections.ICollection collection)
            {
                count = collection.Count;
            }
            else if (value != null)
            {
                // Try parse string to int
                if (int.TryParse(value.ToString(), out int parsedValue))
                {
                    count = parsedValue;
                }
            }

            bool isEmpty = count == 0;

            if (IsInverted)
            {
                isEmpty = !isEmpty;
            }

            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not supported for CountToVisibilityConverter");
        }
    }
