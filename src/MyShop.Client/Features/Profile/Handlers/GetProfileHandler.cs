using MediatR;
using MyShop.Client.Features.Profile.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Profile.Handlers;

/// <summary>
/// Handler for GetProfileQuery
/// </summary>
public class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<User>>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IAuthRepository _authRepository;

    public GetProfileHandler(IProfileRepository profileRepository, IAuthRepository authRepository)
    {
        _profileRepository = profileRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<User>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID from auth
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<User>.Failure("User not authenticated");
            }

            var profileData = await _profileRepository.GetByUserIdAsync(userResult.Data.Id);
            if (profileData == null)
            {
                return Result<User>.Failure("Profile not found");
            }

            // Map ProfileData to User model
            var user = new User
            {
                Id = profileData.UserId,
                Email = profileData.Email,
                FullName = profileData.FullName,
                PhoneNumber = profileData.PhoneNumber,
                Address = profileData.Address,
                Avatar = profileData.Avatar,
                CreatedAt = profileData.CreatedAt
            };

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure($"Error loading profile: {ex.Message}");
        }
    }
}
