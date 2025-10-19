using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.DTOs.Common;
using Refit;
using System.Threading.Tasks;

namespace MyShop.Client.ApiServer {
    [Headers("User-Agent: MyShop-Client/1.0")]
    public interface IAuthApi {
        [Post("/api/v1/auth/login")]
        Task<MyShop.Shared.DTOs.Common.ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);

        [Post("/api/v1/auth/register")]
        Task<MyShop.Shared.DTOs.Common.ApiResponse<CreateUserResponse>> RegisterAsync([Body] CreateUserRequest request);

        [Get("/api/v1/auth/me")]
        Task<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>> GetMeAsync();
    }
}