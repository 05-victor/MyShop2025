using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for creating a sales agent request
/// </summary>
public class CreateAgentRequestRequest
{
    [Required(ErrorMessage = "Sales experience is required")]
    [MaxLength(1000, ErrorMessage = "Sales experience must not exceed 1000 characters")]
    public string Experience { get; set; } = string.Empty;

    [Required(ErrorMessage = "Motivation/Reason is required")]
    [MaxLength(1000, ErrorMessage = "Motivation must not exceed 1000 characters")]
    public string Reason { get; set; } = string.Empty;
}
