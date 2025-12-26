using System;
using System.Collections.Generic;

namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// DTO for login response payload.
/// Contains user information and JWT token after successful authentication.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username for display and login.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the trial period is currently active.
    /// </summary>
    public bool IsTrialActive { get; set; }

    /// <summary>
    /// Trial period start date.
    /// </summary>
    public DateTime? TrialStartDate { get; set; }

    /// <summary>
    /// Trial period end date.
    /// </summary>
    public DateTime? TrialEndDate { get; set; }

    /// <summary>
    /// Whether email has been verified.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// List of role names assigned to the user (e.g., "ADMIN", "SALESAGENT", "USER").
    /// </summary>
    public List<string> RoleNames { get; set; } = new List<string>();

    /// <summary>
    /// JWT authentication token for subsequent API calls.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token (long-lived, typically 7-30 days).
    /// Used to obtain new access tokens without re-authentication.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// When the access token expires (UTC).
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// When the refresh token expires (UTC).
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}
