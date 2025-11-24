namespace MyShop.Shared.Models;

/// <summary>
/// Extended user profile data
/// </summary>
public class ProfileData
{
    public Guid UserId { get; set; }
    public string? Avatar { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? JobTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
