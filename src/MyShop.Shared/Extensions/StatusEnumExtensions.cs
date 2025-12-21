using System.ComponentModel;

namespace MyShop.Shared.Extensions;

/// <summary>
/// Extension methods for status enums
/// Handles conversion between enum values (used in database/backend) and string values (used in API/frontend)
/// </summary>
public static class StatusEnumExtensions
{
    /// <summary>
    /// Convert enum to uppercase string for API/frontend
    /// Example: ProductStatus.Available -> "AVAILABLE"
    /// </summary>
    public static string ToApiString<TEnum>(this TEnum enumValue) where TEnum : Enum
    {
        return enumValue.ToString().ToUpperInvariant();
    }

    /// <summary>
    /// Convert string to enum, case-insensitive
    /// Example: "available" -> ProductStatus.Available
    /// </summary>
    public static TEnum ParseApiString<TEnum>(string value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        // Try exact match first
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        // Handle special cases for backward compatibility
        return default;
    }

    /// <summary>
    /// Try to convert string to enum
    /// </summary>
    public static bool TryParseApiString<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out result);
    }
}
