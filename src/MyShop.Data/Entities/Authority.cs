using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho quyền hạn trong hệ thống MyShop.
    /// Chứa thông tin về các quyền cụ thể mà người dùng có thể có.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng Authorities trong database và chứa:
    /// - Tên quyền hạn (Name) - là primary key
    /// - Mô tả quyền hạn (Description) - optional
    /// 
    /// Quyền hạn được gán cho Role và thông qua Role được gán cho User.
    /// </remarks>
    public class Authority
    {
        /// <summary>
        /// Lấy hoặc đặt tên quyền hạn.
        /// </summary>
        /// <value>Chuỗi tên quyền hạn, đóng vai trò là primary key</value>
        [Key]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mô tả chi tiết về quyền hạn.
        /// </summary>
        /// <value>Chuỗi mô tả quyền hạn, có thể null</value>
        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Lấy hoặc đặt danh sách các vai trò có quyền hạn này.
        /// </summary>
        /// <value>Collection chứa tất cả các Role có quyền hạn này</value>
        /// <remarks>
        /// Navigation property đến entity Role.
        /// Many-to-many relationship thông qua bảng trung gian.
        /// </remarks>
        // Add this property to fix CS1061
        public ICollection<RoleAuthorities> RoleAuthorities { get; set; } = new List<RoleAuthorities>();
    }
}