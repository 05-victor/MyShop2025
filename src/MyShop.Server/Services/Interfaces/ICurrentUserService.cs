
namespace MyShop.Server.Services.Implementations;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }

    string? Email { get; }
}