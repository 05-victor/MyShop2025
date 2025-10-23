using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// DTO cho th�ng tin quy?n h?n.
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
        [Required(ErrorMessage = "T�n quy?n h?n kh�ng ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "T�n quy?n h?n kh�ng ???c v??t qu� 100 k� t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "M� t? kh�ng ???c v??t qu� 500 k� t?")]
        public string? Description { get; set; }
        
        [MaxLength(100, ErrorMessage = "T�n module kh�ng ???c v??t qu� 100 k� t?")]
        public string? Module { get; set; }
    }

    /// <summary>
    /// DTO cho vi?c c?p nh?t quy?n h?n.
    /// </summary>
    public class UpdateAuthorityRequest
    {
        [Required(ErrorMessage = "T�n quy?n h?n kh�ng ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "T�n quy?n h?n kh�ng ???c v??t qu� 100 k� t?")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "M� t? kh�ng ???c v??t qu� 500 k� t?")]
        public string? Description { get; set; }
        
        [MaxLength(100, ErrorMessage = "T�n module kh�ng ???c v??t qu� 100 k� t?")]
        public string? Module { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}