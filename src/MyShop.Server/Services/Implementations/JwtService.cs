using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyShop.Data.Entities;
using MyShop.Server.Configuration;
using MyShop.Server.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Service for handling JWT token generation and validation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        /// <summary>
        /// Generate a JWT access token for the specified user
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <returns>JWT token string</returns>
        public string GenerateAccessToken(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new("phone_number", user.PhoneNumber),
                    new("is_verified", user.IsVerified.ToString().ToLower()),
                    new("activate_trial", user.ActivateTrial.ToString().ToLower()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                   // new(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add role claims
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                    SigningCredentials = credentials,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT token generated successfully for user: {Username}", user.Username);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {Username}", user.Username);
                throw;
            }
        }

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        public string GenerateRefreshToken()
        {
            try
            {
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var refreshToken = Convert.ToBase64String(randomBytes);

                _logger.LogDebug("Refresh token generated successfully");
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token");
                throw;
            }
        }

        /// <summary>
        /// Validate a JWT token and extract claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
                };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Ensure the token is a JWT token
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid JWT token format or algorithm");
                    return null;
                }

                _logger.LogDebug("JWT token validated successfully");
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("JWT token has expired: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("JWT token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during JWT token validation");
                return null;
            }
        }

        /// <summary>
        /// Get user ID from JWT token claims
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from validated token</param>
        /// <returns>User ID if found, null otherwise</returns>
        public Guid? GetUserIdFromClaims(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID claim not found in token");
                    return null;
                }

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                _logger.LogWarning("Invalid User ID format in token: {UserIdClaim}", userIdClaim);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from claims");
                return null;
            }
        }

        /// <summary>
        /// Get username from JWT token claims
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from validated token</param>
        /// <returns>Username if found, null otherwise</returns>
        public string? GetUsernameFromClaims(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var username = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username claim not found in token");
                    return null;
                }

                return username;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting username from claims");
                return null;
            }
        }
    }
}