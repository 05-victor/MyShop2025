using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for system activation and licensing
/// Manages admin codes, trial activation, and license information
/// </summary>
public interface ISystemActivationRepository
{
    /// <summary>
    /// Check if any admin exists in the system
    /// </summary>
    Task<Result<bool>> HasAnyAdminAsync();

    /// <summary>
    /// Validate an activation code without using it
    /// </summary>
    Task<Result<AdminCodeInfo>> ValidateCodeAsync(string code);

    /// <summary>
    /// Activate an admin code for a user
    /// This will promote the user to Admin role
    /// </summary>
    Task<Result<LicenseInfo>> ActivateCodeAsync(string code, Guid userId);

    /// <summary>
    /// Get current license information
    /// </summary>
    Task<Result<LicenseInfo?>> GetCurrentLicenseAsync();

    /// <summary>
    /// Get remaining trial days
    /// Returns -1 for permanent license, 0 or positive for trial
    /// </summary>
    Task<Result<int>> GetRemainingTrialDaysAsync();

    /// <summary>
    /// Check if trial is about to expire (within warning period)
    /// </summary>
    Task<Result<bool>> IsTrialExpiringAsync();

    /// <summary>
    /// Check if trial has expired
    /// </summary>
    Task<Result<bool>> IsTrialExpiredAsync();

    /// <summary>
    /// Demote admin to customer when trial expires
    /// </summary>
    Task<Result<bool>> DemoteExpiredAdminAsync();
}

#region DTOs

/// <summary>
/// Information about an admin activation code
/// </summary>
public class AdminCodeInfo
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "trial"; // "trial" or "permanent"
    public int? DurationDays { get; set; }
    public string? Description { get; set; }
    public bool IsValid { get; set; }
}

/// <summary>
/// Current license information
/// </summary>
public class LicenseInfo
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = "trial"; // "trial" or "permanent"
    public DateTime ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? DurationDays { get; set; }
    public int RemainingDays { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiring { get; set; }
    public bool IsPermanent => Type.Equals("permanent", StringComparison.OrdinalIgnoreCase);
}

#endregion
