using System;

namespace MyShop.Shared.DTOs.Responses
{
    /// <summary>
    /// DTO cho ph?n h?i ??ng nh?p
    /// </summary>
    public class LoginResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
    }
}
