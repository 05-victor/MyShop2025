using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho người dùng trong hệ thống MyShop.
    /// Chứa thông tin cơ bản và thông tin xác thực của người dùng.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng Users trong database và chứa:
    /// - Thông tin định danh (ID, Username, Email)
    /// - Thông tin xác thực (PasswordHash)
    /// - Metadata (CreatedAt, UpdatedAt)
    /// - Navigation properties đến các entity liên quan (Orders)
    /// 
    /// Mật khẩu được lưu dưới dạng hash để đảm bảo bảo mật.
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Lấy hoặc đặt ID duy nhất của người dùng.
        /// </summary>
        /// <value>Primary key tự động tăng của người dùng</value>
        public int Id { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập của người dùng.
        /// </summary>
        /// <value>Chuỗi username tối đa 100 ký tự, bắt buộc và duy nhất</value>
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt địa chỉ email của người dùng.
        /// </summary>
        /// <value>Chuỗi email tối đa 255 ký tự, bắt buộc và phải đúng định dạng email</value>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mật khẩu đã được mã hóa của người dùng.
        /// </summary>
        /// <value>Chuỗi chứa password hash (không phải plaintext password)</value>
        /// <remarks>
        /// Mật khẩu được mã hóa bằng BCrypt trước khi lưu vào trường này.
        /// Không bao giờ lưu mật khẩu dưới dạng plaintext.
        /// </remarks>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt thời gian tạo tài khoản.
        /// </summary>
        /// <value>DateTime khi tài khoản được tạo, mặc định là thời gian hiện tại (UTC)</value>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Lấy hoặc đặt thời gian cập nhật tài khoản lần cuối.
        /// </summary>
        /// <value>DateTime khi tài khoản được cập nhật lần cuối, null nếu chưa cập nhật</value>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt danh sách đơn hàng của người dùng.
        /// </summary>
        /// <value>Collection chứa tất cả đơn hàng mà người dùng đã tạo</value>
        /// <remarks>
        /// Navigation property đến entity Order.
        /// Sử dụng Entity Framework để load dữ liệu liên quan.
        /// </remarks>
        public List<Order> Orders { get; set; } = new();
    }
}