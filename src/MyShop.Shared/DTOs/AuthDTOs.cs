using System;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs {
    /// <summary>
    /// DTO chứa thông tin user cơ bản.
    /// </summary>
    public class UserInfo {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO chung cho phản hồi xác thực.
    /// </summary>
    public class AuthResponse {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }
}
