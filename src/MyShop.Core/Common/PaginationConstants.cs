namespace MyShop.Core.Common;

/// <summary>
/// Default pagination values for the application.
/// 
/// IMPORTANT: These are COMPILE-TIME DEFAULTS only!
/// - Used as default parameter values in method signatures
/// - Used as fallback when settings haven't loaded yet
/// 
/// For RUNTIME values (user-configurable page sizes):
/// - Inject IPaginationService 
/// - Call Initialize() after settings load
/// - Use GetPageSize()/SetPageSize() for dynamic values
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Default page size for all entity types (fallback value)
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Default page size for product lists
    /// </summary>
    public const int ProductsPageSize = 10;

    /// <summary>
    /// Default page size for order lists
    /// </summary>
    public const int OrdersPageSize = 10;

    /// <summary>
    /// Default page size for agent requests
    /// </summary>
    public const int AgentRequestsPageSize = 10;

    /// <summary>
    /// Maximum allowed page size to prevent performance issues
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Available page size options for UI dropdowns
    /// </summary>
    public static readonly int[] PageSizeOptions = [10, 15, 20, 25, 50, 100];
}
