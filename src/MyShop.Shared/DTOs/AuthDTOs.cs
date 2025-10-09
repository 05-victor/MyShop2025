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
        public string UsernameOrEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu để đăng nhập.
        /// </summary>
        /// <value>Chuỗi chứa mật khẩu, mặc định là chuỗi rỗng</value>
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
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt địa chỉ email cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa email, mặc định là chuỗi rỗng</value>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa mật khẩu, mặc định là chuỗi rỗng</value>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt số điện thoại cho tài khoản mới.
        /// </summary>
        /// <value>Chuỗi chứa số điện thoại, mặc định là chuỗi rỗng</value>
        public string Sdt { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho việc tạo người dùng với đầy đủ thông tin.
    /// </summary>
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public bool ActivateTrial { get; set; } = false;
        public string? Avatar { get; set; }
        public List<string>? RoleNames { get; set; }
    }

    /// <summary>
    /// DTO cho việc tạo vai trò mới.
    /// </summary>
    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string>? AuthorityNames { get; set; }
    }

    /// <summary>
    /// DTO cho việc tạo quyền hạn mới.
    /// </summary>
    public class CreateAuthorityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO chứa phản hồi từ server cho các hoạt động xác thực.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để trả về kết quả của các thao tác đăng nhập và đăng ký.
    /// Chứa thông tin về trạng thái thành công/thất bại, thông báo, token và thông tin người dùng.
    /// </remarks>
    public class AuthResponse
    {
        /// <summary>
        /// Lấy hoặc đặt trạng thái thành công của hoạt động xác thực.
        /// </summary>
        /// <value>True nếu thành công, false nếu thất bại</value>
        public bool Success { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt thông báo mô tả kết quả hoạt động.
        /// </summary>
        /// <value>Chuỗi chứa thông báo thành công hoặc lỗi, mặc định là chuỗi rỗng</value>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt token xác thực cho client.
        /// </summary>
        /// <value>Chuỗi chứa authentication token, mặc định là chuỗi rỗng</value>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt thông tin người dùng sau khi xác thực thành công.
        /// </summary>
        /// <value>UserDto chứa thông tin người dùng, null nếu xác thực thất bại</value>
        public UserDto? User { get; set; }
    }

    /// <summary>
    /// DTO chứa thông tin cơ bản của người dùng.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để truyền thông tin người dùng giữa client và server
    /// mà không bao gồm các thông tin nhạy cảm như mật khẩu đã mã hóa.
    /// </remarks>
    public class UserDto
    {
        /// <summary>
        /// Lấy hoặc đặt ID duy nhất của người dùng.
        /// </summary>
        /// <value>UUID của người dùng</value>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập của người dùng.
        /// </summary>
        /// <value>Chuỗi chứa username, mặc định là chuỗi rỗng</value>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt địa chỉ email của người dùng.
        /// </summary>
        /// <value>Chuỗi chứa email, mặc định là chuỗi rỗng</value>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt số điện thoại của người dùng.
        /// </summary>
        /// <value>Chuỗi chứa số điện thoại, mặc định là chuỗi rỗng</value>
        public string Sdt { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt thời gian tạo tài khoản.
        /// </summary>
        /// <value>DateTime chứa thời gian tạo tài khoản</value>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt trạng thái kích hoạt thử nghiệm.
        /// </summary>
        /// <value>True nếu tài khoản được kích hoạt thử nghiệm, false nếu không</value>
        public bool ActivateTrial { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt đường dẫn đến ảnh đại diện của người dùng.
        /// </summary>
        /// <value>Chuỗi URL hoặc đường dẫn đến ảnh đại diện, mặc định là chuỗi rỗng</value>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt danh sách vai trò của người dùng.
        /// </summary>
        /// <value>Collection chứa các RoleDto</value>
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }

    /// <summary>
    /// DTO chứa thông tin vai trò.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để truyền thông tin vai trò giữa client và server.
    /// </remarks>
    public class RoleDto
    {
        /// <summary>
        /// Lấy hoặc đặt tên vai trò.
        /// </summary>
        /// <value>Chuỗi tên vai trò</value>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mô tả vai trò.
        /// </summary>
        /// <value>Chuỗi mô tả vai trò, có thể null</value>
        public string? Description { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt danh sách quyền hạn của vai trò.
        /// </summary>
        /// <value>Collection chứa các AuthorityDto</value>
        public List<AuthorityDto> Authorities { get; set; } = new List<AuthorityDto>();
    }

    /// <summary>
    /// DTO chứa thông tin quyền hạn.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để truyền thông tin quyền hạn giữa client và server.
    /// </remarks>
    public class AuthorityDto
    {
        /// <summary>
        /// Lấy hoặc đặt tên quyền hạn.
        /// </summary>
        /// <value>Chuỗi tên quyền hạn</value>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mô tả quyền hạn.
        /// </summary>
        /// <value>Chuỗi mô tả quyền hạn, có thể null</value>
        public string? Description { get; set; }
    }
}