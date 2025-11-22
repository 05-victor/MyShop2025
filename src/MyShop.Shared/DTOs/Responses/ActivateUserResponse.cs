

namespace MyShop.Shared.DTOs.Responses;

public class ActivateUserResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public ActivateUserResponse(bool success, string? message)
    {
        Success = success;
        Message = message;
    }
}
