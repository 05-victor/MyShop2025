using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Shared.Adapters;

/// <summary>
/// Adapter for mapping User DTOs to User Models
/// Extends AuthAdapter functionality
/// </summary>
public static class UserAdapter
{
    /// <summary>
    /// UserInfoResponse (DTO) → User (Model)
    /// </summary>
    public static User ToModel(UserInfoResponse dto)
    {
        return new User
        {
            Id = dto.Id,
            Username = dto.Username ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Avatar = dto.Avatar,
            CreatedAt = dto.CreatedAt,
            IsTrialActive = dto.IsTrialActive,
            TrialStartDate = dto.TrialStartDate,
            TrialEndDate = dto.TrialEndDate,
            IsEmailVerified = dto.IsEmailVerified,
            Roles = AuthAdapter.ParseRoles(dto.RoleNames)
        };
    }

    /// <summary>
    /// Convert list of UserInfoResponse to list of User
    /// </summary>
    public static List<User> ToModelList(IEnumerable<UserInfoResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }
}
