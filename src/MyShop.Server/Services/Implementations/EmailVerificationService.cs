using Microsoft.IdentityModel.Tokens;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Service for handling email verification with secure JWT-based tokens.
    /// Implements token generation, email sending, and verification logic.
    /// </summary>
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailNotificationService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailVerificationService> _logger;

        // Token expires after 24 hours
        private const int TokenExpirationHours = 24;

        public EmailVerificationService(
            IUserRepository userRepository,
            ICurrentUserService currentUser,
            IEmailNotificationService emailService,
            IConfiguration configuration,
            ILogger<EmailVerificationService> logger)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generates a secure JWT token containing the user ID and expiration.
        /// Token is signed with the application's secret key.
        /// </summary>
        public string GenerateVerificationToken(Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] 
                ?? throw new InvalidOperationException("JWT Key not configured"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", userId.ToString()),
                    new Claim("purpose", "email_verification"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(TokenExpirationHours),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Sends a verification email to the user with a secure URL containing the verification token.
        /// </summary>
        public async Task<ServiceResult> SendVerificationEmailAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = _currentUser.UserId;

                if (!userId.HasValue)
                {
                    throw new UnauthorizedAccessException("Invalid JWT: userId claim is missing.");
                }

                // Get user from database
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    return ServiceResult.Failure("User not found");
                }

                // Check if already verified
                if (user.IsEmailVerified)
                {
                    _logger.LogInformation("User {UserId} email is already verified", userId);
                    return ServiceResult.Failure("Email is already verified");
                }

                // Generate verification token
                var token = GenerateVerificationToken(userId.Value);

                // Build verification URL
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5228";
                var verificationUrl = $"{baseUrl}/api/v1/email-verification/verify?token={token}";

                // Prepare email placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "USERNAME", user.Username },
                    { "EMAIL", user.Email },
                    { "VERIFICATION_URL", verificationUrl },
                    { "EXPIRATION_HOURS", TokenExpirationHours.ToString() }
                };

                // Send email using template
                var emailRequest = new SendEmailTemplateRequest
                {
                    To = user.Email,
                    Subject = "Verify Your Email Address",
                    TemplateName = "email-verification.html",
                    Placeholders = placeholders
                };

                var emailResult = await _emailService.SendEmailAsync(
                    user.Email,
                    user.Username,
                    emailRequest.Subject,
                    emailRequest.TemplateName,
                    emailRequest.Placeholders);

                if (!emailResult)
                {
                    _logger.LogError("Failed to send verification email to {Email}", user.Email);
                    return ServiceResult.Failure("Failed to send verification email");
                }

                _logger.LogInformation("Verification email sent successfully to {Email}", user.Email);
                return ServiceResult.Success("Verification email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email for user {UserId}", _currentUser.UserId.Value);
                return ServiceResult.Failure($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the verification token and extracts the user ID if valid.
        /// </summary>
        public Guid? ValidateVerificationToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] 
                    ?? throw new InvalidOperationException("JWT Key not configured"));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Verify token purpose
                var purposeClaim = principal.FindFirst("purpose");
                if (purposeClaim?.Value != "email_verification")
                {
                    _logger.LogWarning("Token validation failed: Invalid purpose claim");
                    return null;
                }

                // Extract user ID
                var userIdClaim = principal.FindFirst("userId");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("Token validation failed: Invalid userId claim");
                    return null;
                }

                return userId;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token validation failed: Token expired");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating verification token");
                return null;
            }
        }

        /// <summary>
        /// Verifies the user's email by validating the token and updating the database.
        /// </summary>
        public async Task<ServiceResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate token and extract user ID
                var userId = ValidateVerificationToken(token);
                if (userId == null)
                {
                    _logger.LogWarning("Email verification failed: Invalid or expired token");
                    return ServiceResult.Failure("Invalid or expired verification token");
                }

                // Get user from database
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found during verification", userId);
                    return ServiceResult.Failure("User not found");
                }

                // Check if already verified
                if (user.IsEmailVerified)
                {
                    _logger.LogInformation("User {UserId} email is already verified", userId);
                    return ServiceResult.Success("Email is already verified");
                }

                // Update user's verification status
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Email verified successfully for user {UserId}", userId);
                return ServiceResult.Success("Email verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return ServiceResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}
