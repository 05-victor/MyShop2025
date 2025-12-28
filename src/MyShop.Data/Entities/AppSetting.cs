using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities;

/// <summary>
/// Entity for global application settings (affects all users)
/// Stores shop information and app metadata
/// </summary>
public class AppSetting
{
    [Key]
    public int Id { get; set; }
    
    // General Settings
    [Required]
    [MaxLength(100)]
    public string ShopName { get; set; } = "MyShop 2025";
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    // App Info (read-only fields managed by system)
    [Required]
    [MaxLength(100)]
    public string AppName { get; set; } = "MyShop 2025";
    
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";
    
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(50)]
    public string License { get; set; } = "Commercial";
    
    [Required]
    [MaxLength(100)]
    public string Support { get; set; } = "support@myshop.com";
    
    // Metadata
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
}
