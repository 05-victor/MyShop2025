using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;


namespace MyShop.Server.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProfileService> _logger;
    public ProfileService(
        IProfileRepository profileRepository,
        ICurrentUserService currentUserService,
        ILogger<ProfileService> logger)
    {
        _profileRepository = profileRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    public async Task<UpdateProfileResponse?> UpdateMyProfileAsync(UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            _logger.LogWarning("Invalid JWT: userId claim is missing.");
            return null;
        }
        var profile = await _profileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return null;
        }

        profile.FullName = request.FullName;
        profile.PhoneNumber = request.PhoneNumber;
        profile.Address = request.Address;
        profile.Avatar = request.Avatar;

        await _profileRepository.UpdateAsync(profile);

        var response = new UpdateProfileResponse
        {
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            Avatar = profile.Avatar
        };
        return response;
    }
}