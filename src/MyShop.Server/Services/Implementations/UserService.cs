
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, ICurrentUserService currentUserService, HttpClient httpClient, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUser = currentUserService;
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<ActivateUserResponse> ActivateUserAsync(string activateCode)
    {
        try
        {
            var userId = _currentUser.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("Invalid JWT: userId claim is missing.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for activation", userId);
                return new ActivateUserResponse(false, "User not found");
            }

            // TODO: Work in production
            //if (!user.IsEmailVerified)
            //{
            //    _logger.LogWarning("User with ID {UserId} attempted activation without verified email", userId);
            //    return new ActivateUserResponse(false, "Email not verified");
            //}

            if (user.Roles.Select(r => r.Name).ToList().Contains("Admin"))
            {
                _logger.LogInformation("Admin user with ID {UserId} cannot be activated", userId);
                return new ActivateUserResponse(false, "The account is already updated to Admin");
            }

            // Check code
            var checkResponse = await _httpClient.GetFromJsonAsync<CheckCodeResponse>($"https://backend-adminportal.onrender.com/api/code/check/{activateCode}");

            if (checkResponse == null || !checkResponse.Valid)
            {
                _logger.LogWarning("Failed to validate activation code {ActivateCode}", activateCode);
                return new ActivateUserResponse(false, checkResponse?.Reason);
            }

            // Deactivate code
            var data = new
            {
                Code = activateCode,
            };

            var deactivateResponse = await _httpClient.PostAsJsonAsync($"https://backend-adminportal.onrender.com/api/code/deactivate", data);

            if (deactivateResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Activation code {ActivateCode} deactivated successfully", activateCode);
            }
            else
            {
                _logger.LogWarning("Failed to deactivate activation code {ActivateCode}", activateCode);
                return new ActivateUserResponse(false, "Failed to deactivate activation code");
            }

            // Two mode: Trial and Premium
            var adminRole = await _roleRepository.GetByNameAsync("Admin");
            if (adminRole == null)
            {
                _logger.LogError("Admin role not found in the database");
                return new ActivateUserResponse(false, "Internal error: Admin role not found");
            }

            user.Roles.Add(adminRole);
            string message = string.Empty;
            if (activateCode.StartsWith("TRL-"))
            {
                user.IsTrialActive = true;
                user.TrialStartDate = DateTime.UtcNow;
                user.TrialEndDate = DateTime.UtcNow.AddDays(15);
                message = "You've been activated as Admin (15-day trial)";
                _logger.LogInformation("User with ID {UserId} activated as Admin (Trial)", userId);
            }
            else if (activateCode.StartsWith("PRM-"))
            {               
                message = "You've been activated as Admin (Premium)";
                _logger.LogInformation("User with ID {UserId} activated as Admin (Premium)", userId);
            }
            
            await _userRepository.UpdateAsync(user);
            return new ActivateUserResponse(true, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating user with code {ActivateCode}", activateCode);
            throw;
        }
    }
}