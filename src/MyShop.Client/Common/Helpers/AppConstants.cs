namespace MyShop.Client.Common.Helpers;

/// <summary>
/// Application-wide constants for currency conversion, rates, and other fixed values
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Exchange rate: VND to USD (Vietnamese Dong to US Dollar)
    /// Used to convert prices from client (VND) to API (USD)
    /// Example: 29,990,000 VND / 26265 = $1140.88 USD
    /// </summary>
    public const double VND_TO_USD_RATE = 26265.0;

    /// <summary>
    /// Exchange rate: USD to VND (US Dollar to Vietnamese Dong)
    /// Used to convert API responses (USD) back to client display (VND)
    /// Example: 1140.88 USD * 26265 = 29,989,992 VND
    /// </summary>
    public const double USD_TO_VND_RATE = 26265.0;

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public const int DEFAULT_PAGE_SIZE = 10;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public const int MAX_PAGE_SIZE = 100;
}
