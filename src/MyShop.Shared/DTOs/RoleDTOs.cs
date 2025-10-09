using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho thông tin vai trò.
    /// </summary>
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public List<AuthorityDto> Authorities { get; set; } = new List<AuthorityDto>();
    }

    /// <summary>
    /// DTO cho vi?c t?o vai trò m?i.
    /// </summary>
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Tên vai trò không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên vai trò không ???c v??t quá 100 ký t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "Mô t? không ???c v??t quá 500 ký t?")]
        public string? Description { get; set; }
        
        public List<string> AuthorityNames { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO cho vi?c c?p nh?t vai trò.
    /// </summary>
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Tên vai trò không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên vai trò không ???c v??t quá 100 ký t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "Mô t? không ???c v??t quá 500 ký t?")]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public List<string> AuthorityNames { get; set; } = new List<string>();
    }
}