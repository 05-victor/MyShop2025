using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests
{
    /// <summary>
    /// DTO cho y�u c?u ??ng nh?p
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "T�n ??ng nh?p ho?c email kh�ng ???c ?? tr?ng")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u kh�ng ???c ?? tr?ng")]
        [MinLength(6, ErrorMessage = "M?t kh?u ph?i c� �t nh?t 6 k� t?")]
        public string Password { get; set; } = string.Empty;
    }
}
