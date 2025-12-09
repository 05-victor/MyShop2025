namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for pagination parameters with defaults
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based). Default is 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default is 10.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
