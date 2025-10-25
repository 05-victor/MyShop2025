using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IUserService
{
    Task<ActivateUserResponse> ActivateUserAsync(string activateCode);
    Task<ActivateUserResponse> ActivateSaleMode(Guid userId);
}