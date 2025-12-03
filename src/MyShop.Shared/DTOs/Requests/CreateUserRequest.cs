using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Requests;

public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100, ErrorMessage = "Username must not exceed 100 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? Sdt { get; set; }

        public bool ActivateTrial { get; set; } = false;
        
    /// <summary>
    /// List of role names to assign to the user during registration.
    /// If empty or null, no roles will be assigned.
    /// </summary>
    public List<string> RoleNames { get; set; } = new List<string>();
}
