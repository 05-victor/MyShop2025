using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Commands;

/// <summary>
/// Command to perform user login.
/// </summary>
/// <param name="Username">The username to authenticate.</param>
/// <param name="Password">The password for authentication.</param>
/// <param name="RememberMe">Whether to persist credentials for future sessions.</param>
public record LoginCommand(
    string Username, 
    string Password, 
    bool RememberMe
) : IRequest<Result<User>>;
