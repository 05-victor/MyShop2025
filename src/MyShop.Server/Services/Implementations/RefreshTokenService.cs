using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Exceptions;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Security.Claims;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Service implementation for managing refresh tokens.
    /// Implements secure token rotation and revocation strategies.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ShopContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IUserAuthorityService _userAuthorityService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            ShopContext context,
            IJwtService jwtService,
            IUserRepository userRepository,
            IUserAuthorityService userAuthorityService,
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _userAuthorityService = userAuthorityService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Create and save a new refresh token for a user
        /// </summary>
        public async Task<RefreshToken> CreateRefreshTokenAsync(User user, string? ipAddress = null)
        {
            var tokenString = _jwtService.GenerateRefreshToken(ipAddress);
            var expiryDays = _jwtService.GetRefreshTokenExpirationDays();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = tokenString,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedByIp = ipAddress
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token created for user {Username} (expires in {Days} days)", 
                user.Username, expiryDays);

            return refreshToken;
        }

        /// <summary>
        /// Refresh access token using a refresh token
        /// Implements automatic token rotation
        /// </summary>
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token request missing token");
                throw ValidationException.ForField("RefreshToken", "Refresh token is required");
            }

            // Find the refresh token
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u!.Roles)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length)));
                throw AuthenticationException.InvalidCredentials();
            }

            // Validate token is active
            if (!refreshToken.IsActive)
            {
                var reason = refreshToken.IsRevoked ? "Token has been revoked" : "Token has expired";
                _logger.LogWarning("Inactive refresh token used: {Reason}", reason);
                throw AuthenticationException.ExpiredToken();
            }

            // Get user
            var user = refreshToken.User;
            if (user == null)
            {
                _logger.LogError("User not found for refresh token");
                throw AuthenticationException.Unauthenticated();
            }

            // Check if token rotation is enabled
            var enableRotation = _configuration.GetValue<bool>("JwtSettings:EnableRefreshTokenRotation", true);

            string? newRefreshTokenString = null;
            DateTime? newRefreshTokenExpiresAt = null;

            if (enableRotation)
            {
                // Revoke the old token
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.RevokedReason = "Replaced by new token";

                // Create new refresh token
                var newRefreshToken = await CreateRefreshTokenAsync(user, ipAddress);

                // Link old token to new token
                refreshToken.ReplacedByTokenId = newRefreshToken.Id;
                
                newRefreshTokenString = newRefreshToken.Token;
                newRefreshTokenExpiresAt = newRefreshToken.ExpiresAt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token rotated for user {Username}", user.Username);
            }

            // Generate new access token
            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpirationMinutes());

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString, // Null if rotation disabled
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt
            };
        }

        /// <summary>
        /// Revoke a single refresh token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string token, string? ipAddress = null, string? reason = null)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
            {
                _logger.LogWarning("Token not found for revocation");
                return false;
            }

            if (refreshToken.IsRevoked)
            {
                _logger.LogInformation("Token already revoked");
                return true;
            }

            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.RevokedReason = reason ?? "Manual revocation";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user {UserId}", refreshToken.UserId);
            return true;
        }

        /// <summary>
        /// Revoke all refresh tokens for a user (logout/password change)
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId, string? ipAddress = null, string? reason = null)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.RevokedReason = reason ?? "All tokens revoked";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", activeTokens.Count, userId);
            return activeTokens.Count;
        }

        /// <summary>
        /// Clean up expired and revoked tokens (maintenance task)
        /// </summary>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30); // Keep tokens for 30 days after expiration for audit

            var tokensToDelete = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < cutoffDate || (rt.RevokedAt != null && rt.RevokedAt < cutoffDate))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(tokensToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", tokensToDelete.Count);
            return tokensToDelete.Count;
        }
    }
}
