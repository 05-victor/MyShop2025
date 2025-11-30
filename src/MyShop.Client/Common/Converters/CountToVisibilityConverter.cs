using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converter to show empty state based on item count.
/// If count = 0, returns Visible (shows empty state).
/// If count > 0, returns Collapsed (hides empty state, shows list).
/// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Set to true to invert logic (show when items exist, hide when empty).
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
