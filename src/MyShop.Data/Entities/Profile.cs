using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Data.Entities
{
    public class Profile
    {
        public Guid UserId { get; set; }
        
        // Navigation property
        public User User { get; set; } = null!;
        
        public string? Avatar { get; set; }
        
        [MaxLength(100)]
        public string? FullName { get; set; }
        
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(200)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}