using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IProfileService
{
    Task<UpdateProfileResponse?> UpdateMyProfileAsync(UpdateProfileRequest request);
}