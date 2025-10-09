using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho thông tin chi ti?t c?a ng??i dùng.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public bool ActivateTrial { get; set; } = true;
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }

    /// <summary>
    /// DTO cho vi?c c?p nh?t thông tin ng??i dùng.
    /// </summary>
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Tên ??ng nh?p không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên ??ng nh?p không ???c v??t quá 100 ký t?")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email không ???c ?? tr?ng")]
        [EmailAddress(ErrorMessage = "Email không ?úng ??nh d?ng")]
        [MaxLength(255, ErrorMessage = "Email không ???c v??t quá 255 ký t?")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "S? ?i?n tho?i không ???c ?? tr?ng")]
        [MaxLength(20, ErrorMessage = "S? ?i?n tho?i không ???c v??t quá 20 ký t?")]
        public string Sdt { get; set; } = string.Empty;
        
        public string? Avatar { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ActivateTrial { get; set; } = true;
    }
}