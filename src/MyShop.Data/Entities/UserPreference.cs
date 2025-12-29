using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities;

/// <summary>
/// Entity for user-specific preference settings
/// Each user has their own preferences (e.g., theme)
/// </summary>
public class UserPreference
{
    [Key]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Theme { get; set; } = "Light"; // 'Light' or 'Dark'
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User? User { get; set; }
}
