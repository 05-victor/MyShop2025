using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Linq;

namespace MyShop.Server.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IRoleRepository roleRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<CreateUserResponse> RegisterAsync(CreateUserRequest request)
    {
        try
        {
            // Log incoming request
            _logger.LogInformation("✅ Register request received for username: {Username}, email: {Email}", 
                request.Username, request.Email);

            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Username, request.Email))
            {
                throw new InvalidOperationException("Username or email already exists");
            }

            // Validate and fetch roles if provided
            var roles = new List<Role>();
            if (request.RoleNames != null && request.RoleNames.Any())
            {
                foreach (var roleName in request.RoleNames)
                {
                    var role = await _roleRepository.GetByNameAsync(roleName);
                    if (role == null)
                    {
                        throw new InvalidOperationException($"Role '{roleName}' does not exist");
                    }
                    roles.Add(role);
                }
            }

            // Create new user with Profile
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsEmailVerified = false,
                IsTrialActive = request.ActivateTrial,
                TrialStartDate = request.ActivateTrial ? DateTime.UtcNow : null,
                TrialEndDate = request.ActivateTrial ? DateTime.UtcNow.AddDays(15) : null,
                Roles = roles,
                Profile = new Profile
                {
                    PhoneNumber = request.Sdt,
                    Avatar = request.Avatar,
                    FullName = request.Username // Default to username
                }
            };

            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("✅ User registered successfully: {Username} (ID: {UserId}) with roles: {Roles}",
                createdUser.Username,
                createdUser.Id,
                string.Join(", ", createdUser.Roles.Select(r => r.Name)));

            return new CreateUserResponse
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                PhoneNumber = createdUser.Profile?.PhoneNumber,
                CreatedAt = createdUser.CreatedAt,
                Avatar = createdUser.Profile?.Avatar,
                ActivateTrial = createdUser.IsTrialActive,
                IsVerified = createdUser.IsEmailVerified,
                RoleNames = createdUser.Roles.Select(r => r.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during user registration for {Username}", request.Username);
            throw;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("✅ Login request received for: {UsernameOrEmail}", request.UsernameOrEmail);

            // Find user by username or email
            var user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);

            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail);
            }

            if (user == null)
            {
                _logger.LogWarning("⚠️ Login failed - User not found: {UsernameOrEmail}", request.UsernameOrEmail);
                throw new UnauthorizedAccessException("Invalid username/email or password");
            }

            // Verify hashed password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("⚠️ Login failed - Invalid password for user: {Username}", user.Username);
                throw new UnauthorizedAccessException("Invalid username/email or password");
            }

            _logger.LogInformation("✅ User logged in successfully: {Username} (ID: {UserId})", 
                user.Username, user.Id);

            // Generate JWT token
            var token = await _jwtService.GenerateAccessTokenAsync(user);

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.Profile?.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Avatar = user.Profile?.Avatar,
                ActivateTrial = user.IsTrialActive,
                IsVerified = user.IsEmailVerified,
                RoleNames = user.Roles.Select(r => r.Name).ToList(),
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during user login for {UsernameOrEmail}", request.UsernameOrEmail);
            throw;
        }
    }

    public async Task<UserInfoResponse?> GetMeAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("GetMe called for userId: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            return new UserInfoResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
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
