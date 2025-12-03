using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing pagination settings across the application.
/// Provides runtime page sizes that can be configured from user settings.
/// Register as Singleton in DI container.
/// </summary>
public interface IPaginationService
{
    #region Default/Max

    /// <summary>
    /// Default page size for general lists
    /// </summary>
    int DefaultPageSize { get; }

    /// <summary>
    /// Maximum allowed page size
    /// </summary>
    int MaxPageSize { get; }

    #endregion

    #region Entity Page Sizes

    /// <summary>
    /// Page size for product lists
    /// </summary>
    int ProductsPageSize { get; set; }

    /// <summary>
    /// Page size for order lists
    /// </summary>
    int OrdersPageSize { get; set; }

    /// <summary>
    /// Page size for customer lists
    /// </summary>
    int CustomersPageSize { get; set; }

    /// <summary>
    /// Page size for user/admin user lists
    /// </summary>
    int UsersPageSize { get; set; }

    /// <summary>
    /// Page size for agent requests
    /// </summary>
    int AgentRequestsPageSize { get; set; }

    /// <summary>
    /// Page size for commission lists
    /// </summary>
    int CommissionsPageSize { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Initialize pagination settings from stored values
    /// </summary>
    void Initialize(PaginationSettings settings);

    /// <summary>
    /// Reset all page sizes to defaults
    /// </summary>
    void Reset();

    /// <summary>
    /// Get page size for a specific entity type
    /// </summary>
    int GetPageSize(PaginationEntityType entityType);

    /// <summary>
    /// Set page size for a specific entity type
    /// </summary>
    void SetPageSize(PaginationEntityType entityType, int pageSize);

    /// <summary>
    /// Get current settings as a PaginationSettings object
    /// </summary>
    PaginationSettings GetSettings();

    #endregion
}

/// <summary>
/// Entity types for pagination
/// </summary>
public enum PaginationEntityType
{
    Default,
    Products,
    Orders,
    Customers,
    Users,
    AgentRequests,
    Commissions
}
