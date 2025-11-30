using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Commands;

/// <summary>
/// Command to perform user registration.
/// </summary>
/// <param name="Username">The username for the new account.</param>
/// <param name="Email">The email address for the new account.</param>
/// <param name="PhoneNumber">The phone number for the new account.</param>
/// <param name="Password">The password for the new account.</param>
/// <param name="Role">The role to assign to the new user.</param>
public record RegisterCommand(
    string Username,
    string Email,
    string PhoneNumber,
    string Password,
    string Role
) : IRequest<Result<User>>;
