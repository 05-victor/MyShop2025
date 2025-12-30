namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request to activate a trial or premium license code
/// </summary>
public class ActivateTrialRequest
{
    /// <summary>
    /// Admin/License activation code (e.g., TRL-XXXX-XXXX or PRM-XXXX-XXXX)
    /// </summary>
    public string AdminCode { get; set; } = string.Empty;
}
