using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.User;

namespace MyShop.Server.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Username, request.Email))
            {
                return new AuthResponseDto
                {
                    Message = "Username or email already exists"
                };
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Password = request.Password, // TODO: Hash password in production
                FullName = request.FullName,
                Email = request.Email,
                Photo = request.Photo ?? string.Empty,
                Role = "user", // Default role
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = DateTime.MinValue
            };

            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully: {Username}", createdUser.Username);

            return new AuthResponseDto
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                Photo = createdUser.Photo,
                Role = createdUser.Role,
                Token = string.Empty, // TODO: Generate JWT token when authentication is implemented
                Message = "User registered successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // Find user by username
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid username or password"
                };
            }

            // TODO: Verify hashed password in production
            if (user.Password != request.Password)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid username or password"
                };
            }

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);

            return new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Photo = user.Photo,
                Role = user.Role,
                Token = string.Empty, // TODO: Generate JWT token when authentication is implemented
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            throw;
        }
    }

    public async Task<UserDto?> GetMeAsync(int userId)
    {
        try
        {
            // TODO: This will be populated when authentication is implemented
            // For now, return null as authentication is not yet implemented
            _logger.LogInformation("GetMe called for userId: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Photo = user.Photo,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return null;
        }
    }
}
