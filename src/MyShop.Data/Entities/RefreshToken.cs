using System.ComponentModel.DataAnnotations;

namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity representing a refresh token for JWT authentication.
    /// Implements token rotation and revocation for security.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Unique identifier for the refresh token.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The refresh token string (cryptographically secure random value).
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The user ID this token belongs to.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// When the token was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the token expires (typically 7-30 days).
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the token was revoked (if applicable).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation (e.g., "Replaced by new token", "User logout", "Security breach").
        /// </summary>
        [MaxLength(200)]
        public string? RevokedReason { get; set; }

        /// <summary>
        /// ID of the token that replaced this one (for token rotation tracking).
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// IP address from which the token was created.
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// IP address from which the token was revoked.
        /// </summary>
        [MaxLength(45)]
        public string? RevokedByIp { get; set; }

        // Navigation property
        public User? User { get; set; }

        /// <summary>
        /// Check if the token is currently active (not expired and not revoked).
        /// </summary>
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        /// <summary>
        /// Check if the token has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Check if the token has been revoked.
        /// </summary>
        public bool IsRevoked => RevokedAt != null;
    }
}
