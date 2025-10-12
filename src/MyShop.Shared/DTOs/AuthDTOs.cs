/*
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO chứa thông tin đăng nhập từ client.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để nhận thông tin đăng nhập từ client,
    /// hỗ trợ đăng nhập bằng username hoặc email.
    /// </remarks>
    public class LoginRequest
    {
        /// <summary>
        /// Lấy hoặc đặt username hoặc email để đăng nhập.
        /// </summary>
        /// <value>Chuỗi chứa username hoặc email, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Email hoặc tên đăng nhập không được để trống")]
        public string UsernameOrEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu để đăng nhập.
        /// </summary>
        /// <value>Chuỗi chứa mật khẩu, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO chứa thông tin đăng ký từ client.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để nhận thông tin đăng ký tài khoản mới từ client.
    /// Bao gồm tất cả thông tin cần thiết để tạo tài khoản người dùng.
    /// </remarks>
    public class RegisterRequest
    {
        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa username, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt địa chỉ email cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa email, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa mật khẩu, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt số điện thoại cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa số điện thoại, mặc định là chuỗi rỗng</value>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string Sdt { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho việc tạo người dùng với đầy đủ thông tin.
    /// </summary>
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string Sdt { get; set; } = string.Empty;
        
        public string? Avatar { get; set; }
        public bool ActivateTrial { get; set; } = true;
        public List<string> RoleNames { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO cho phản hồi đăng nhập thành công.
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// DTO cho phản hồi đăng ký.
    /// </summary>
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// DTO chứa thông tin user cơ bản.
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO chung cho phản hồi xác thực.
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }
}
*/