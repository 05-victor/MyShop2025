/*
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho th�ng tin vai tr�.
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
    /// DTO cho vi?c t?o vai tr� m?i.
    /// </summary>
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "T�n vai tr� kh�ng ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "T�n vai tr� kh�ng ???c v??t qu� 100 k� t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "M� t? kh�ng ???c v??t qu� 500 k� t?")]
        public string? Description { get; set; }
        
        public List<string> AuthorityNames { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO cho vi?c c?p nh?t vai tr�.
    /// </summary>
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "T�n vai tr� kh�ng ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "T�n vai tr� kh�ng ???c v??t qu� 100 k� t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "M� t? kh�ng ???c v??t qu� 500 k� t?")]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public List<string> AuthorityNames { get; set; } = new List<string>();
    }
}
*/