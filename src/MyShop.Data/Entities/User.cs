using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho người dùng trong hệ thống MyShop.
    /// Chứa thông tin cơ bản và thông tin xác thực của người dùng.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng Users trong database và chứa:
    /// - Thông tin định danh (Id, Username, Email)
    /// - Thông tin xác thực (Password)
    /// - Thông tin bổ sung (Avatar, ActivateTrial)
    /// - Metadata (CreatedAt)
    /// - Navigation properties đến các entity liên quan (Orders, Roles, RefreshTokens)
    /// 
    /// Mật khẩu được lưu dưới dạng hash để đảm bảo bảo mật.
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Lấy hoặc đặt ID duy nhất của người dùng.
        /// </summary>
        /// <value>UUID làm primary key của người dùng</value>
        public Guid Id { get; set; }

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
        /// Lấy hoặc đặt thời gian tạo tài khoản.
        /// </summary>
        /// <value>DateTime khi tài khoản được tạo, mặc định là thời gian hiện tại (UTC)</value>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// Lấy hoặc đặt trạng thái kích hoạt thử nghiệm.
        /// </summary>
        /// <value>True nếu được kích hoạt thử nghiệm, false nếu không</value>
        public bool IsTrialActive { get; set; } = false;
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }

        /// <summary>
        /// Lấy hoặc đặt trạng thái xác thực của người dùng.
        /// </summary>
        /// <value>True nếu người dùng đã xác thực, false nếu chưa</value>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// Lấy hoặc đặt thời gian cập nhật gần nhất.
        /// </summary>
        /// <value>DateTime khi tài khoản được cập nhật lần cuối, có thể null</value>
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        
        /// <summary>
        /// Lấy hoặc đặt danh sách các vai trò của người dùng.
        /// </summary>
        /// <value>Collection các Role entities được gán cho user này</value>
        public ICollection<Role> Roles { get; set; } = new List<Role>();

        /// <summary>
        /// Lấy hoặc đặt danh sách các quyền hạn bị loại bỏ cho người dùng này.
        /// </summary>
        /// <value>Collection các RemovedAuthorities entries - quyền bị hạn chế cho user này</value>
        /// <remarks>
        /// Các quyền trong collection này sẽ bị loại bỏ khỏi quyền hiệu lực của user,
        /// ngay cả khi user có các quyền đó thông qua role của mình.
        /// </remarks>
        public ICollection<RemovedAuthorities> RemovedAuthorities { get; set; } = new List<RemovedAuthorities>();

        /// <summary>
        /// Lấy hoặc đặt danh sách các refresh token của người dùng.
        /// </summary>
        /// <value>Collection các RefreshToken entities thuộc user này</value>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public Guid? ProfileId { get; set; }
        public Profile? Profile { get; set; }
    }
}