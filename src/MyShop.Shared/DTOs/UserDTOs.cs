/*
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho th�ng tin chi ti?t c?a ng??i d�ng.
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
    /// DTO cho vi?c c?p nh?t th�ng tin ng??i d�ng.
    /// </summary>
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "T�n ??ng nh?p kh�ng ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "T�n ??ng nh?p kh�ng ???c v??t qu� 100 k� t?")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email kh�ng ???c ?? tr?ng")]
        [EmailAddress(ErrorMessage = "Email kh�ng ?�ng ??nh d?ng")]
        [MaxLength(255, ErrorMessage = "Email kh�ng ???c v??t qu� 255 k� t?")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "S? ?i?n tho?i kh�ng ???c ?? tr?ng")]
        [MaxLength(20, ErrorMessage = "S? ?i?n tho?i kh�ng ???c v??t qu� 20 k� t?")]
        public string Sdt { get; set; } = string.Empty;
        
        public string? Avatar { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ActivateTrial { get; set; } = true;
    }
}
*/