using Microsoft.UI.Xaml.Data;
using MyShop.Client.Views.Components.Badges;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts status strings to StatusBadgeVariant enum values.
/// Maps Vietnamese and English status texts to appropriate badge variants.
/// </summary>
public class StatusToVariantConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var status = value?.ToString()?.ToLowerInvariant();

        if (string.IsNullOrEmpty(status))
        {
            return StatusBadgeVariant.Default;
        }

        return status switch
        {
            // Pending patterns
            "pending" or "created" or "waiting" => StatusBadgeVariant.Pending,
            
            // Approved patterns
            "approved" or "active" or "verified" => StatusBadgeVariant.Approved,
            
            // Rejected patterns
            "rejected" or "inactive" or "cancelled" or "canceled" => StatusBadgeVariant.Rejected,
            
            // Processing patterns
            "processing" or "confirmed" or "shipped" or "in progress" or "shipping" => StatusBadgeVariant.Processing,
            
            // Completed patterns
            "completed" or "delivered" or "done" or "paid" or "success" => StatusBadgeVariant.Completed,
            
            // Info patterns
            "info" or "information" or "unpaid" => StatusBadgeVariant.Info,
            
            // Default
            _ => StatusBadgeVariant.Default
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("StatusToVariantConverter does not support ConvertBack.");
    }
}
