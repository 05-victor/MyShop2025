using MyShop.Shared.Models.Enums;
using System;
using System.Collections.Generic;

namespace MyShop.Shared.Models
{
    /// <summary>
    /// Domain model for User entity (separate from DTOs).
    /// Contains user profile information, authentication state, and role assignments.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username for login.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address (used for verification and communication).
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number for contact purposes.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// URL or path to user's avatar image.
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Physical address for shipping/contact.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Account creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Last login timestamp.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Whether trial period is currently active.
        /// </summary>
        public bool IsTrialActive { get; set; }

        /// <summary>
        /// Trial period start date.
        /// </summary>
        public DateTime? TrialStartDate { get; set; }

        /// <summary>
        /// Trial period end date.
        /// </summary>
        public DateTime? TrialEndDate { get; set; }

        /// <summary>
        /// Whether email has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// List of roles assigned to this user.
        /// </summary>
        public List<UserRole> Roles { get; set; } = new();

        /// <summary>
        /// JWT authentication token (stored in memory during session).
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Check if user has a specific role.
        /// </summary>
        /// <param name="role">The role to check for.</param>
        /// <returns>True if user has the specified role.</returns>
        public bool HasRole(UserRole role) => Roles.Contains(role);

        /// <summary>
        /// Get the highest priority role (Admin > Salesman > Customer).
        /// Used for navigation and permission determination.
        /// </summary>
        /// <returns>The primary role for this user.</returns>
        public UserRole GetPrimaryRole()
        {
            if (Roles.Contains(UserRole.Admin)) return UserRole.Admin;
            if (Roles.Contains(UserRole.Salesman)) return UserRole.Salesman;
            return UserRole.Customer;
        }
    }
}
