namespace MyShop.Shared.Models;

/// <summary>
/// Represents a sales agent registration request
/// </summary>
public class AgentRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string Notes { get; set; } = string.Empty; // reason / experience combined
}
