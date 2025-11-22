namespace MyShop.Shared.DTOs.Commons;

/// <summary>
/// Generic paged result for pagination support
/// Used for API responses and repository layer
/// </summary>
/// <typeparam name="T">Type of items in the page</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Index of the first item in the current page (1-based)
    /// </summary>
    public int FirstItemIndex => (Page - 1) * PageSize + 1;

    /// <summary>
    /// Index of the last item in the current page (1-based)
    /// </summary>
    public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);
}
