using Microsoft.UI.Xaml.Data;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts a boolean value to a string based on parameter specification.
/// Parameter format: "FalseText|TrueText" (e.g., "Deactivate|Activate")
/// If no parameter provided: False="No", True="Yes"
/// </summary>
public partial class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        
        // Parse parameter if provided
        if (parameter is string textSpec && !string.IsNullOrEmpty(textSpec))
        {
            var texts = textSpec.Split('|');
            if (texts.Length == 2)
            {
                return boolValue ? texts[1] : texts[0];
            }
        }
        
        // Default text: False="No", True="Yes"
        return boolValue ? "Yes" : "No";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
