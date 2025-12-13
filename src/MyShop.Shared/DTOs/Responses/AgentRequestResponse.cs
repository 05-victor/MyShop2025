namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for agent request
/// </summary>
public class AgentRequestResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string Experience { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? BusinessName { get; set; }
    public string? TaxId { get; set; }
}
