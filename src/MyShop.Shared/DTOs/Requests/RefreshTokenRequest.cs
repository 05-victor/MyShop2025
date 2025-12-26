namespace MyShop.Shared.DTOs.Requests
{
    /// <summary>
    /// Request to refresh an access token using a refresh token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token obtained during login or previous refresh.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
