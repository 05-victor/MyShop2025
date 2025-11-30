namespace MyShop.Core.Common;

/// <summary>
/// FALLBACK pagination values - used ONLY when settings haven't loaded yet.
/// 
/// ┌──────────────────────────────────────────────────────────────────────┐
/// │  PAGINATION ARCHITECTURE                                             │
/// ├──────────────────────────────────────────────────────────────────────┤
/// │                                                                      │
/// │  RUNTIME FLOW:                                                       │
/// │  1. App starts → Load settings from FileSettingsStorage              │
/// │  2. Initialize IPaginationService with loaded values                 │
/// │  3. All ViewModels inject IPaginationService and use its values      │
/// │  4. User changes settings → Save to storage + sync service           │
/// │  5. All ViewModels automatically get updated values                  │
/// │                                                                      │
/// │  COMPONENTS:                                                         │
/// │                                                                      │
/// │  ► PaginationConstants (this file) - FALLBACK ONLY                   │
/// │    - Used as default when file doesn't exist                         │
/// │    - Used as default parameter in method signatures                  │
/// │    - DO NOT use directly in ViewModels for runtime values            │
/// │                                                                      │
/// │  ► IPaginationService (Singleton)                                    │
/// │    - RUNTIME source of truth for page sizes                          │
/// │    - Inject this in ViewModels                                       │
/// │    - Initialized from AppSettings.Pagination on startup              │
/// │    - Synced when user saves settings                                 │
/// │                                                                      │
/// │  ► FileSettingsStorage (AppData/Local/MyShop2025)                    │
/// │    - Persists user preferences to JSON file                          │
/// │    - Contains AppSettings with Pagination property                   │
/// │                                                                      │
/// └──────────────────────────────────────────────────────────────────────┘
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
