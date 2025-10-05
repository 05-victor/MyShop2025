using System.Threading.Tasks;
using MyShop.Shared.DTOs;

namespace MyShop.Client.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string email, string password);
        Task<AuthResponse> RegisterAsync(string username, string email, string password);
        Task<UserDto?> GetCurrentUserAsync();
    }
}