using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho vai trò trong hệ thống MyShop.
    /// Chứa thông tin về các vai trò và quyền hạn tương ứng.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng Roles trong database và chứa:
    /// - Tên vai trò (Name) - là primary key
    /// - Mô tả vai trò (Description) - optional
    /// - Navigation properties đến Users và Authorities
    /// 
    /// Vai trò được gán cho User và chứa nhiều Authority.
    /// </remarks>
    public class Role
    {
        /// <summary>
        /// Lấy hoặc đặt tên vai trò.
        /// </summary>
        /// <value>Chuỗi tên vai trò, đóng vai trò là primary key</value>
        [Key]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Lấy hoặc đặt mô tả chi tiết về vai trò.
        /// </summary>
        /// <value>Chuỗi mô tả vai trò, có thể null</value>
        [MaxLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Lấy hoặc đặt danh sách các quyền hạn của vai trò này.
        /// </summary>
        /// <value>Collection chứa tất cả các Authority thuộc vai trò này</value>
        /// <remarks>
        /// Navigation property đến entity Authority.
        /// Many-to-many relationship thông qua bảng trung gian.
        /// </remarks>
        public ICollection<RoleAuthorities> RoleAuthorities { get; set; } = new List<RoleAuthorities>();


        /// <summary>
        /// Lấy hoặc đặt danh sách người dùng có vai trò này.
        /// </summary>
        /// <value>Collection chứa tất cả các User có vai trò này</value>
        /// <remarks>
        /// Navigation property đến entity User.
        /// Many-to-many relationship thông qua bảng trung gian.
        /// </remarks>
        public ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Lấy hoặc đặt danh sách các quyền hạn thuộc vai trò này.
        /// </summary>
        /// <value>Collection chứa tất cả các RoleAuthorities thuộc vai trò này</value>
        /// <remarks>
        /// Thuộc tính này được thêm vào để khắc phục lỗi CS1061.
        /// </remarks>
    }
}