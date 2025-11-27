namespace MyShop.Core.Common;

/// <summary>
/// Centralized pagination constants for the application
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Default page size for general lists (users, commissions, etc.)
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Page size for product lists
    /// </summary>
    public const int ProductsPageSize = 10;

    /// <summary>
    /// Page size for order lists
    /// </summary>
    public const int OrdersPageSize = 10;

    /// <summary>
    /// Page size for agent requests lists
    /// </summary>
    public const int AgentRequestsPageSize = 10;

    /// <summary>
    /// Maximum allowed page size to prevent performance issues
    /// </summary>
    public const int MaxPageSize = 100;
}
