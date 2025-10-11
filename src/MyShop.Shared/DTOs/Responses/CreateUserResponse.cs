using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Responses
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Avatar { get; set; }
        public bool ActivateTrial { get; set; }
        public bool IsVerified { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
