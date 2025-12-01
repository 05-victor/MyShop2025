using System;
using System.Collections.Generic;

namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// DTO for user information.
/// </summary>
public class UserInfoResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsTrialActive { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public List<string> RoleNames { get; set; } = new List<string>();
}
