using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Profile.Queries;

/// <summary>
/// Query to get user profile (skeleton - backend not ready)
/// </summary>
public record GetProfileQuery() : IRequest<Result<User>>;
