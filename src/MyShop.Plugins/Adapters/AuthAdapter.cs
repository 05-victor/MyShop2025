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
    /// </summary>
    public static User ToModel(LoginResponse dto)
    {
        return new User
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
    }

    /// <summary>
    /// UserInfoResponse (DTO) → User (Model)
    /// Used for /me endpoint responses
    /// </summary>
    public static User ToModel(UserInfoResponse dto, string token)
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
        System.Diagnostics.Debug.WriteLine($"  - RoleNames: {(dto.RoleNames != null ? string.Join(", ", dto.RoleNames) : "null")}");

        var roles = ParseRoles(dto.RoleNames);
        System.Diagnostics.Debug.WriteLine($"  - Parsed Roles: {string.Join(", ", roles)}");

        return new User
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
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
            else if (normalized == "SALEMAN" || normalized == "SALESMAN" || normalized == "SALESAGENT")
                roles.Add(UserRole.Salesman);
            else if (normalized == "CUSTOMER" || normalized == "USER")
                roles.Add(UserRole.Customer);
        }

        // Default to Customer if no roles found
        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        return roles;
    }
}
