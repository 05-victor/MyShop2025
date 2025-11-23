using System;
using System.Collections.Generic;

namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// DTO cho phản hồi đăng nhập
/// </summary>
public class LoginResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsTrialActive { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsEmailVerified { get; set; }
    public List<string> RoleNames { get; set; } = new List<string>();
    public string Token { get; set; } = string.Empty;
}
