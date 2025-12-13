using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities;

/// <summary>
/// Entity representing a sales agent registration request
/// </summary>
public class AgentRequest
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public User? User { get; set; }
    
    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    
    public Guid? ReviewedBy { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Experience { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? BusinessName { get; set; }
    
    [MaxLength(50)]
    public string? TaxId { get; set; }
}
