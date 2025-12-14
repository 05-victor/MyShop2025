using Microsoft.IdentityModel.Tokens;
using MyShop.Data.Entities;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Responses;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IUserAuthorityService _userAuthorityService;

        public JwtService(
            IConfiguration configuration, 
            ILogger<JwtService> logger, 
            IUserAuthorityService userAuthorityService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            _tokenHandler = new JwtSecurityTokenHandler();
            _userAuthorityService = userAuthorityService;
        }

        /// <summary>
        /// Generate a JWT access token for the specified user
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <returns>JWT token string</returns>
        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                };

                EffectiveAuthoritiesResponse? effectiveAuthoritiesResponse = await _userAuthorityService.GetEffectiveAuthoritiesDetailAsync(user.Id);
                // Add authority claims
                if (effectiveAuthoritiesResponse != null)
                {
                    foreach (var authority in effectiveAuthoritiesResponse.EffectiveAuthorities)
                    {
                        claims.Add(new Claim("authority", authority));
                    }

                    // add role claims
                    foreach (var roleName in effectiveAuthoritiesResponse.RoleNames)
                    {
                        claims.Add(new Claim("role", roleName)); // Use "role" directly
                    }
                }

                // Read JWT settings from configuration
                var secretKey = _configuration["JwtSettings:SecretKey"] 
                    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured");
                
                if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
                {
                    throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters long");
                }

                var issuer = _configuration["JwtSettings:Issuer"] ?? "MyShop.Server";
                var audience = _configuration["JwtSettings:Audience"] ?? "MyShop.Client";
                var expiryInMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryInMinutes", 60);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                    SigningCredentials = credentials,
                    Issuer = issuer,
                    Audience = audience
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
                // Read JWT settings from configuration
                var secretKey = _configuration["JwtSettings:SecretKey"] 
                    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured");
                
                var issuer = _configuration["JwtSettings:Issuer"] ?? "MyShop.Server";
                var audience = _configuration["JwtSettings:Audience"] ?? "MyShop.Client";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
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
    }
}