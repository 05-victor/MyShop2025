using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IAuthService
{
    Task<CreateUserResponse> RegisterAsync(CreateUserRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserInfoResponse?> GetMeAsync(int userId);
}
