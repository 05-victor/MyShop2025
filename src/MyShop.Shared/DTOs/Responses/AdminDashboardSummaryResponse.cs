namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for Admin Dashboard summary
/// Contains platform-wide metrics for administrators
/// </summary>
public class AdminDashboardSummaryResponse
{
    /// <summary>
    /// Number of active sales agents on the platform
    /// </summary>
    public int ActiveSalesAgents { get; set; }

    /// <summary>
    /// Total number of products on the platform (all agents)
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Total GMV (Gross Merchandise Value) for the selected period
    /// Sum of all order amounts across all agents
    /// </summary>
    public decimal TotalGmv { get; set; }

    /// <summary>
    /// Admin commission (platform fee) for the selected period
    /// Typically 5% of Total GMV
    /// </summary>
    public decimal AdminCommission { get; set; }

    /// <summary>
    /// Total number of orders in the selected period
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total revenue in the selected period (same as TotalGmv for compatibility)
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Top 5 best-selling products across the entire platform
    /// </summary>
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

    /// <summary>
    /// Top 5 sales agents ranked by GMV in the selected period
    /// </summary>
    public List<TopSalesAgentDto> TopSalesAgents { get; set; } = new();
}

/// <summary>
/// DTO for top sales agent information in admin dashboard
/// </summary>
public class TopSalesAgentDto
{
    /// <summary>
    /// Sales Agent User ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Agent's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Agent's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Total GMV for this agent in the selected period
    /// </summary>
    public decimal TotalGmv { get; set; }

    /// <summary>
    /// Commission earned by this agent (GMV minus platform fee)
    /// </summary>
    public decimal Commission { get; set; }

    /// <summary>
    /// Number of products published by this agent
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Number of orders for this agent in the selected period
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Average rating for this agent (placeholder for future implementation)
    /// </summary>
    //public double Rating { get; set; }
}
