namespace MyShop.Shared.DTOs.Responses
{
    /// <summary>
    /// Response DTO for successful token refresh.
    /// Contains new access token and optionally a new refresh token (rotation).
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        /// New JWT access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// New refresh token (if rotation is enabled).
        /// If null, the old refresh token is still valid.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// When the new access token expires (UTC).
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// When the new refresh token expires (UTC).
        /// Null if refresh token wasn't rotated.
        /// </summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }
    }
}
