using System;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests
{
    /// <summary>
    /// Request DTO để thêm quyền hạn vào danh sách loại bỏ của user.
    /// </summary>
    public class AddRemovedAuthorityRequest
    {
        /// <summary>
        /// Tên quyền hạn cần loại bỏ.
        /// </summary>
        [Required(ErrorMessage = "Tên quyền hạn không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên quyền hạn không được vượt quá 100 ký tự")]
        public string AuthorityName { get; set; } = string.Empty;

        /// <summary>
        /// Lý do loại bỏ quyền hạn (optional).
        /// </summary>
        [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string? Reason { get; set; }

        /// <summary>
        /// Username của người thực hiện loại bỏ (optional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "Username không được vượt quá 100 ký tự")]
        public string? RemovedBy { get; set; }
    }
}
