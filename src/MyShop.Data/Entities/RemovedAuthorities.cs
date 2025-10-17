using System;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity đại diện cho các quyền hạn bị loại bỏ của người dùng cụ thể.
    /// Hoạt động như một blacklist để ghi đè các quyền từ role.
    /// </summary>
    /// <remarks>
    /// Entity này ánh xạ đến bảng removed_authorities trong database.
    /// Nó cho phép loại bỏ các quyền cụ thể từ user mà không cần thay đổi role.
    /// 
    /// Logic quyền hạn hiệu lực:
    /// User's Effective Authorities = (Role Authorities) - (Removed Authorities)
    /// </remarks>
    public class RemovedAuthorities
    {
        /// <summary>
        /// Lấy hoặc đặt ID của người dùng bị hạn chế quyền.
        /// </summary>
        /// <value>UUID của user</value>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property đến User entity.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Lấy hoặc đặt tên quyền hạn bị loại bỏ.
        /// </summary>
        /// <value>Tên của authority bị loại bỏ</value>
        [Required]
        [MaxLength(100)]
        public string AuthorityName { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property đến Authority entity.
        /// </summary>
        public Authority Authority { get; set; } = null!;

        /// <summary>
        /// Lấy hoặc đặt lý do loại bỏ quyền hạn.
        /// </summary>
        /// <value>Mô tả lý do, có thể null</value>
        [MaxLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// Lấy hoặc đặt thời gian loại bỏ quyền hạn.
        /// </summary>
        /// <value>DateTime khi quyền bị loại bỏ, mặc định là UTC hiện tại</value>
        public DateTime RemovedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lấy hoặc đặt username của người thực hiện loại bỏ quyền.
        /// </summary>
        /// <value>Username của admin thực hiện hành động, có thể null</value>
        [MaxLength(100)]
        public string? RemovedBy { get; set; }
    }
}
