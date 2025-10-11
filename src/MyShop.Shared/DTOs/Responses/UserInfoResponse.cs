using System;

namespace MyShop.Shared.DTOs.Responses
{
    /// <summary>
    /// DTO cho thông tin ng??i dùng
    /// </summary>
    public class UserInfoResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Avatar { get; set; }
        public bool ActivateTrial { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
