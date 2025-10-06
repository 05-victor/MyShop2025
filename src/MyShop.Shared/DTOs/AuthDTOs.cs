namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO chứa thông tin yêu cầu đăng nhập từ client.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để truyền thông tin đăng nhập từ client đến server.
    /// Trường Email có thể chứa email hoặc username để hỗ trợ đăng nhập linh hoạt.
    /// </remarks>
    public class LoginRequest
    {
        /// <summary>
        /// Lấy hoặc đặt email hoặc username để đăng nhập.
        /// </summary>
        /// <value>Chuỗi chứa email hoặc username, mặc định là chuỗi rỗng</value>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu để đăng nhập.
        /// </summary>
        /// <value>Chuỗi chứa mật khẩu, mặc định là chuỗi rỗng</value>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO chứa thông tin yêu cầu đăng ký tài khoản mới từ client.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để truyền thông tin đăng ký từ client đến server.
    /// Chứa tất cả thông tin cần thiết để tạo tài khoản người dùng mới.
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
        /// <value>ID số nguyên của người dùng</value>
        public int Id { get; set; }
        
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
        /// Lấy hoặc đặt thời gian tạo tài khoản.
        /// </summary>
        /// <value>DateTime chứa thời gian tạo tài khoản</value>
        public DateTime CreatedAt { get; set; }
    }
}