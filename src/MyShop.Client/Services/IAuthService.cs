using MyShop.Shared.DTOs;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức xác thực người dùng.
    /// </summary>
    /// <remarks>
    /// Interface này cung cấp các phương thức để:
    /// - Đăng nhập với username/email và password
    /// - Đăng ký tài khoản mới
    /// - Lấy thông tin người dùng hiện tại
    /// - Đăng xuất
    /// 
    /// Tất cả các phương thức đều bất đồng bộ và trả về DTO phù hợp.
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Thực hiện đăng nhập người dùng.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Kết quả đăng nhập</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Thực hiện đăng ký tài khoản mới.
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Kết quả đăng ký</returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Lấy thông tin người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin người dùng</returns>
        Task<UserInfo?> GetCurrentUserAsync();

        /// <summary>
        /// Đăng xuất người dùng hiện tại.
        /// </summary>
        /// <returns>Task</returns>
        Task LogoutAsync();
    }
}