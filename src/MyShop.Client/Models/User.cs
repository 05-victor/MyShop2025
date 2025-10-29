using MyShop.Client.Models.Enums;
using System;
using System.Collections.Generic;

namespace MyShop.Client.Models
{
    /// <summary>
    /// Domain model cho User (tách biệt khỏi DTOs)
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsTrialActive { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsEmailVerified { get; set; }
        public List<UserRole> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Check xem user có role cụ thể không
        /// </summary>
        public bool HasRole(UserRole role) => Roles.Contains(role);

        /// <summary>
        /// Lấy role cao nhất (Admin > Salesman > Customer)
        /// </summary>
        public UserRole GetPrimaryRole()
        {
            if (Roles.Contains(UserRole.Admin)) return UserRole.Admin;
            if (Roles.Contains(UserRole.Salesman)) return UserRole.Salesman;
            return UserRole.Customer;
        }
    }
}
