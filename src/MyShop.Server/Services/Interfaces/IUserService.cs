using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IUserService
{
    Task<UserInfoResponse?> GetMeAsync();
    Task<PagedResult<UserInfoResponse>> GetAllUsersAsync(PaginationRequest request);
    Task<ActivateUserResponse> ActivateUserAsync(string activateCode);
}