using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho thông tin quy?n h?n.
    /// </summary>
    public class AuthorityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Module { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO cho vi?c t?o quy?n h?n m?i.
    /// </summary>
    public class CreateAuthorityRequest
    {
        [Required(ErrorMessage = "Tên quy?n h?n không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên quy?n h?n không ???c v??t quá 100 ký t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "Mô t? không ???c v??t quá 500 ký t?")]
        public string? Description { get; set; }
        
        [MaxLength(100, ErrorMessage = "Tên module không ???c v??t quá 100 ký t?")]
        public string? Module { get; set; }
    }

    /// <summary>
    /// DTO cho vi?c c?p nh?t quy?n h?n.
    /// </summary>
    public class UpdateAuthorityRequest
    {
        [Required(ErrorMessage = "Tên quy?n h?n không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên quy?n h?n không ???c v??t quá 100 ký t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "Mô t? không ???c v??t quá 500 ký t?")]
        public string? Description { get; set; }
        
        [MaxLength(100, ErrorMessage = "Tên module không ???c v??t quá 100 ký t?")]
        public string? Module { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}