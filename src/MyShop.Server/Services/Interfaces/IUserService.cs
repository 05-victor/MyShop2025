using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IUserService
{
    Task<UserInfoResponse?> GetMeAsync();
    Task<PagedResult<UserInfoResponse>> GetAllUsersAsync(PaginationRequest request);
    Task<ActivateUserResponse> ActivateUserAsync(string activateCode);
    
    /// <summary>
    /// Change the current user's password
    /// </summary>
    /// <param name="request">Change password request containing current password, new password, and confirmation</param>
    /// <returns>True if password was changed successfully, false if current password is incorrect, null if user not found</returns>
    Task<bool?> ChangePasswordAsync(ChangePasswordRequest request);
}