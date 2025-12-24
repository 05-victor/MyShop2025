using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for managing application settings stored in LocalSettings
/// </summary>
public class SettingsService
{
    private const string PageSizeKey = "Pagination_DefaultPageSize";
    private const int DefaultPageSize = 25;

    /// <summary>
    /// Gets the default page size for pagination controls
    /// </summary>
    /// <returns>The saved page size, or 25 if not set</returns>
    public int GetDefaultPageSize()
    {
        var settings = ApplicationData.Current.LocalSettings;
        if (settings.Values.TryGetValue(PageSizeKey, out var value) && value is int pageSize)
        {
            // Validate that page size is one of the allowed values
            if (pageSize is 10 or 25 or 50 or 100)
            {
                return pageSize;
            }
        }
        return DefaultPageSize;
    }

    /// <summary>
    /// Sets the default page size for pagination controls
    /// </summary>
    /// <param name="pageSize">The page size to save (must be 10, 15, 20, or 50)</param>
    public void SetDefaultPageSize(int pageSize)
    {
        // Validate input - must match PaginationControl options
        if (pageSize is not (10 or 15 or 20 or 50))
        {
            throw new ArgumentException("Page size must be 10, 15, 20, or 50", nameof(pageSize));
        }

        var settings = ApplicationData.Current.LocalSettings;
        settings.Values[PageSizeKey] = pageSize;
    }
}
