using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CreateUserResponse> RegisterAsync(CreateUserRequest request)
    {
        try
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Username, request.Email))
            {
                throw new InvalidOperationException("Username or email already exists");
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // TODO: Hash password in production
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Avatar = request.Avatar,
                ActivateTrial = request.ActivateTrial,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully: {Username}", createdUser.Username);

            return new CreateUserResponse
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
                Avatar = createdUser.Avatar,
                ActivateTrial = createdUser.ActivateTrial,
                IsVerified = createdUser.IsVerified,
                CreatedAt = createdUser.CreatedAt,
                RoleNames = createdUser.Roles.Select(r => r.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            throw;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Find user by username or email
            var user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);
            
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail);
            }

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username/email or password");
            }

            // TODO: Verify hashed password in production
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid username/email or password");
            }

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                ActivateTrial = user.ActivateTrial,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                RoleNames = user.Roles.Select(r => r.Name).ToList(),
                Token = string.Empty // TODO: Generate JWT token when authentication is implemented
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            throw;
        }
    }

    public async Task<UserInfoResponse?> GetMeAsync(int userId)
    {
        try
        {
            // TODO: This will be populated when authentication is implemented
            // For now, return null as authentication is not yet implemented
            _logger.LogInformation("GetMe called for userId: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return null;

            return new UserInfoResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                ActivateTrial = user.ActivateTrial,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                RoleNames = user.Roles.Select(r => r.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return null;
        }
    }
}
