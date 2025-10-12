namespace MyShop.Server.Configuration;

/// <summary>
/// Configuration settings for JWT authentication
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key used for signing JWT tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// The issuer of the JWT token
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// The audience for the JWT token
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiry time in minutes
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 60;
    
    /// <summary>
    /// Refresh token expiry time in days
    /// </summary>
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}