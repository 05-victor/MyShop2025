namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user profile management
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    Task<ProfileData?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create new user profile
    /// </summary>
    Task<ProfileData> CreateAsync(ProfileData profile);

    /// <summary>
    /// Update existing user profile
    /// </summary>
    Task<ProfileData> UpdateAsync(ProfileData profile);

    /// <summary>
    /// Delete user profile
    /// </summary>
    Task<bool> DeleteAsync(Guid userId);
}

// ===== Data Model for Profile =====

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
