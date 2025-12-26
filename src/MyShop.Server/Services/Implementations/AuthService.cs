using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Exceptions;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Linq;
using System.Security.Claims;

namespace MyShop.Server.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IRoleRepository roleRepository,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _roleRepository = roleRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CreateUserResponse> RegisterAsync(CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw ValidationException.ForField("Username", "Username is required");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw ValidationException.ForField("Email", "Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw ValidationException.ForField("Password", "Password is required");
        }

        _logger.LogInformation("Register request received for username: {Username}, email: {Email}", 
            request.Username, request.Email);

        try
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Username, request.Email))
            {
                var validationEx = new ValidationException("User registration failed");
                
                // Check which field is duplicate
                var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
                if (existingByUsername != null)
                {
                    validationEx.AddError("Username", "Username is already taken");
                }

                var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (existingByEmail != null)
                {
                    validationEx.AddError("Email", "Email is already registered");
                }

                throw validationEx;
            }

            // Validate and fetch default role
            var roles = new List<Role>();
            var defaultRole = await _roleRepository.GetByNameAsync("User");

            if (defaultRole == null)
            {
                _logger.LogError("Default role 'User' does not exist in database");
                throw new InfrastructureException(
                    "System configuration error. Please contact administrator.",
                    null,
                    StatusCodes.Status500InternalServerError);
            }
            roles.Add(defaultRole);

            // Create new user with Profile
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Roles = roles
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Ensure profile is created
            var createdProfile = await _profileRepository.CreateAsync(new Profile
            {
                UserId = createdUser.Id
            });

            _logger.LogInformation("User registered successfully: {Username} (ID: {UserId}) with roles: {Roles}",
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
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during user registration for {Username}", request.Username);
            throw InfrastructureException.DatabaseError("Failed to register user", ex);
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
        {
            throw ValidationException.ForField("UsernameOrEmail", "Username or email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw ValidationException.ForField("Password", "Password is required");
        }

        _logger.LogInformation("Login request received for: {UsernameOrEmail}", request.UsernameOrEmail);

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
                _logger.LogWarning("Login failed - User not found: {UsernameOrEmail}", request.UsernameOrEmail);
                throw AuthenticationException.InvalidCredentials();
            }

            // Verify hashed password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed - Invalid password for user: {Username}", user.Username);
                throw AuthenticationException.InvalidCredentials();
            }

            _logger.LogInformation("User logged in successfully: {Username} (ID: {UserId})", 
                user.Username, user.Id);

            // Generate JWT access token
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpirationMinutes());

            // Generate refresh token
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user, ipAddress);

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsTrialActive = user.IsTrialActive,
                TrialStartDate = user.TrialStartDate,
                TrialEndDate = user.TrialEndDate,
                IsEmailVerified = user.IsEmailVerified,
                RoleNames = user.Roles.Select(r => r.Name).ToList(),
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt
            };
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during user login for {UsernameOrEmail}", request.UsernameOrEmail);
            throw InfrastructureException.DatabaseError("Failed to process login request", ex);
        }
    }
}
