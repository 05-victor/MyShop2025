using MyShop.Shared.DTOs.Responses;

namespace MyShop.Client.Adapters;

/// <summary>
/// Adapter for mapping Profile DTOs to Profile Models
/// </summary>
public static class ProfileAdapter
{
    /// <summary>
    /// UpdateProfileResponse (DTO) → User Profile (partial update)
    /// </summary>
    public static object ToModel(UpdateProfileResponse dto)
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
