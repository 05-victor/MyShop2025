using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests
{
    /// <summary>
    /// DTO cho yêu c?u ??ng nh?p
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên ??ng nh?p ho?c email không ???c ?? tr?ng")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u không ???c ?? tr?ng")]
        [MinLength(6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        public string Password { get; set; } = string.Empty;
    }
}
