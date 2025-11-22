using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Commands;

/// <summary>
/// Command để thực hiện registration
/// </summary>
public record RegisterCommand(
    string Username,
    string Email,
    string PhoneNumber,
    string Password,
    string Role
) : IRequest<Result<User>>;
