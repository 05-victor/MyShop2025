using System.ComponentModel.DataAnnotations;
using MyShop.Shared.Enums;

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
    public AgentRequestStatus Status { get; set; } = AgentRequestStatus.Pending;
    
    public Guid? ReviewedBy { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    [MaxLength(1000)]
    public string? Experience { get; set; }
    
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
