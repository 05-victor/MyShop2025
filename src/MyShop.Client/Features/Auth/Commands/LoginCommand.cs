using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Commands;

/// <summary>
/// Command để thực hiện login
/// </summary>
public record LoginCommand(
    string Username, 
    string Password, 
    bool RememberMe
) : IRequest<Result<User>>;
