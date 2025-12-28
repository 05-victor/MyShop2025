using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for converting Auth DTOs to Domain Models.
/// Static class for stateless DTO-to-Model transformations.
/// Handles login response, user info, and role parsing.
/// </summary>
public static class AuthAdapter
{
    /// <summary>
    /// Convert LoginResponse (DTO) to User (Model).
    /// Includes both access token and refresh token.
    /// </summary>
    public static User ToModel(LoginResponse dto)
    {
        var user = new User
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            IsTrialActive = dto.IsTrialActive,
            TrialStartDate = dto.TrialStartDate,
            TrialEndDate = dto.TrialEndDate,
            IsEmailVerified = dto.IsEmailVerified,
            Token = dto.Token,
            Roles = ParseRoles(dto.RoleNames)
        };

        System.Diagnostics.Debug.WriteLine($"[AuthAdapter] Mapped LoginResponse: Access token present={!string.IsNullOrEmpty(dto.Token)}, Refresh token present={!string.IsNullOrEmpty(dto.RefreshToken)}");
        
        return user;
    }

    /// <summary>
    /// UserInfoResponse (DTO) → User (Model)
    /// Used for /me endpoint responses
    /// </summary>
    public static User ToModel(UserInfoResponse dto, string token)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthAdapter.ToModel] Converting UserInfoResponse to User model:");
            System.Diagnostics.Debug.WriteLine($"  - Id: {dto.Id}");
            System.Diagnostics.Debug.WriteLine($"  - Username: {dto.Username}");
            System.Diagnostics.Debug.WriteLine($"  - Email: {dto.Email}");
            System.Diagnostics.Debug.WriteLine($"  - FullName: {dto.FullName}");
            System.Diagnostics.Debug.WriteLine($"  - PhoneNumber: {dto.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"  - Address: {dto.Address}");
            System.Diagnostics.Debug.WriteLine($"  - Avatar: {dto.Avatar}");
            System.Diagnostics.Debug.WriteLine($"  - IsEmailVerified: {dto.IsEmailVerified}");
            System.Diagnostics.Debug.WriteLine($"  - IsTrialActive: {dto.IsTrialActive}");
            System.Diagnostics.Debug.WriteLine($"  - UpdatedAt: {dto.UpdatedAt}");
            System.Diagnostics.Debug.WriteLine($"  - RoleNames type: {dto.RoleNames?.GetType().FullName ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"  - RoleNames: {(dto.RoleNames != null ? string.Join(", ", dto.RoleNames) : "null")}");

            var roles = ParseRoles(dto.RoleNames);
            System.Diagnostics.Debug.WriteLine($"  - Parsed Roles: {string.Join(", ", roles)}");

            return new User
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                IsTrialActive = dto.IsTrialActive,
                TrialStartDate = dto.TrialStartDate,
                TrialEndDate = dto.TrialEndDate,
                IsEmailVerified = dto.IsEmailVerified,
                Avatar = dto.Avatar,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Token = token,
                Roles = roles
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthAdapter.ToModel] ✗ ERROR during mapping: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AuthAdapter.ToModel] StackTrace: {ex.StackTrace}");
            throw new InvalidOperationException($"Failed to convert UserInfoResponse to User: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parse role names (string[]) → UserRole enums
    /// Made public for reuse by other adapters (e.g., UserAdapter)
    /// </summary>
    public static List<UserRole> ParseRoles(IEnumerable<string>? roleNames)
    {
        if (roleNames == null)
            return new List<UserRole>();

        var roles = new List<UserRole>();

        foreach (var roleName in roleNames)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                continue;

            var normalized = roleName.Trim().ToUpperInvariant();

            if (normalized == "ADMIN")
                roles.Add(UserRole.Admin);
            // Support legacy role names (SALEMAN, SALESMAN) for backward compatibility
            else if (normalized == "SALEMAN" || normalized == "SALESMAN" || normalized == "SALESAGENT")
                roles.Add(UserRole.SalesAgent);
            else if (normalized == "CUSTOMER" || normalized == "USER")
                roles.Add(UserRole.Customer);
        }

        // Default to Customer if no roles found
        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        return roles;
    }
}
