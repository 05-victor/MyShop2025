using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for mapping Profile DTOs to Profile Models
/// </summary>
public static class ProfileAdapter
{
    /// <summary>
    /// ProfileResponse (DTO) → ProfileData (Model)
    /// </summary>
    public static ProfileData ToModel(ProfileResponse dto)
    {
        return new ProfileData
        {
            UserId = dto.UserId,
            Avatar = dto.Avatar,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Address = dto.Address,
            JobTitle = dto.JobTitle,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// ProfileResponse (DTO) → User (Model)
    /// Used when profile update returns ProfileResponse but we need User object
    /// </summary>
    public static User ToUserModel(ProfileResponse dto)
    {
        return new User
        {
            Id = dto.UserId,
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Avatar = dto.Avatar,
            FullName = dto.FullName,
            Address = dto.Address,
            CreatedAt = dto.CreatedAt,
            IsEmailVerified = dto.IsEmailVerified
        };
    }

    /// <summary>
    /// UpdateProfileResponse (DTO) → User Profile (partial update)
    /// </summary>
    public static object ToPartialModel(UpdateProfileResponse dto)
    {
        // Return anonymous object for now - UpdateProfileResponse doesn't have Id, Username, Email, UpdatedAt
        return new
        {
            dto.Avatar,
            dto.FullName,
            dto.PhoneNumber,
            dto.Address
        };
    }
}
