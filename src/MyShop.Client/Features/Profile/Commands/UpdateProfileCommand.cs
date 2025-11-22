using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Profile.Commands;

/// <summary>
/// Command to update user profile (skeleton - backend not ready)
/// </summary>
public record UpdateProfileCommand(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Address
) : IRequest<Result<User>>;
