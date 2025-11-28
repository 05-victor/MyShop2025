namespace MyShop.Shared.Models;

/// <summary>
/// Pagination settings DTO for initialization and serialization.
/// Used for storing and loading pagination preferences.
/// </summary>
public class PaginationSettings
{
    public int DefaultPageSize { get; set; } = 10;
    public int ProductsPageSize { get; set; } = 10;
    public int OrdersPageSize { get; set; } = 10;
    public int CustomersPageSize { get; set; } = 10;
    public int UsersPageSize { get; set; } = 10;
    public int AgentRequestsPageSize { get; set; } = 10;
    public int CommissionsPageSize { get; set; } = 10;
}
