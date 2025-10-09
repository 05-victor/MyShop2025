using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho người dùng trong hệ thống MyShop.
    /// Chứa thông tin cơ bản và thông tin xác thực của người dùng.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng Users trong database và chứa:
    /// - Thông tin định danh (Id, Username, Email, Sdt)
    /// - Thông tin xác thực (Password)
    /// - Thông tin bổ sung (Avatar, ActivateTrial)
    /// - Metadata (CreatedAt)
    /// - Navigation properties đến các entity liên quan (Orders, Roles)
    /// 
    /// Mật khẩu được lưu dưới dạng hash để đảm bảo bảo mật.
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Lấy hoặc đặt ID duy nhất của người dùng.
        /// </summary>
        /// <value>UUID làm primary key của người dùng</value>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập của người dùng.
        /// </summary>
        /// <value>Chuỗi username tối đa 100 ký tự, bắt buộc và duy nhất</value>
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt mật khẩu đã được mã hóa của người dùng.
        /// </summary>
        /// <value>Chuỗi chứa password hash (không phải plaintext password)</value>
        /// <remarks>
        /// Mật khẩu được mã hóa bằng BCrypt trước khi lưu vào trường này.
        /// Không bao giờ lưu mật khẩu dưới dạng plaintext.
        /// </remarks>
        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt địa chỉ email của người dùng.
        /// </summary>
        /// <value>Chuỗi email tối đa 255 ký tự, bắt buộc và phải đúng định dạng email</value>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt số điện thoại của người dùng.
        /// </summary>
        /// <value>Chuỗi số điện thoại tối đa 20 ký tự, bắt buộc</value>
        [Required]
        [MaxLength(20)]
        public string Sdt { get; set; } = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thời gian tạo tài khoản.
        /// </summary>
        /// <value>DateTime khi tài khoản được tạo, mặc định là thời gian hiện tại (UTC)</value>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lấy hoặc đặt đường dẫn đến ảnh đại diện của người dùng.
        /// </summary>
        /// <value>Chuỗi đường dẫn đến ảnh đại diện, có thể null</value>
        public string? Avatar { get; set; }

        /// <summary>
        /// Lấy hoặc đặt trạng thái kích hoạt thử nghiệm.
        /// </summary>
        /// <value>True nếu được kích hoạt thử nghiệm, false nếu không</value>
        public bool ActivateTrial { get; set; } = true;

        /// <summary>
        /// Lấy hoặc đặt thời gian cập nhật gần nhất.
        /// </summary>
        /// <value>DateTime khi tài khoản được cập nhật lần cuối, có thể null</value>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Lấy hoặc đặt trạng thái kích hoạt tài khoản.
        /// </summary>
        /// <value>True nếu tài khoản được kích hoạt, false nếu không</value>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Lấy hoặc đặt thời gian đăng nhập lần cuối.
        /// </summary>
        /// <value>DateTime khi người dùng đăng nhập lần cuối, có thể null</value>
        public DateTime? LastLoginAt { get; set; }

        // Navigation Properties
        
        /// <summary>
        /// Lấy hoặc đặt danh sách các đơn hàng của người dùng.
        /// </summary>
        /// <value>Collection các Order entities liên quan đến user này</value>
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Lấy hoặc đặt danh sách các vai trò của người dùng.
        /// </summary>
        /// <value>Collection các Role entities được gán cho user này</value>
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}