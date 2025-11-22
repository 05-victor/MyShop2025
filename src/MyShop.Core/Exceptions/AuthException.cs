namespace MyShop.Core.Exceptions;

/// <summary>
/// Exception for authentication and authorization errors
/// </summary>
public class AuthException : BaseException
{
    public AuthException(string message)
        : base(message, "AUTH_ERROR")
    {
    }

    public AuthException(string message, Exception innerException)
        : base(message, "AUTH_ERROR", innerException)
    {
    }

    public static AuthException InvalidCredentials()
        => new("Invalid username or password");

    public static AuthException Unauthorized(string resource = "")
        => new($"You are not authorized to access {resource}".Trim());

    public static AuthException TokenExpired()
        => new("Your session has expired. Please login again.");

    public static AuthException InvalidToken()
        => new("Invalid authentication token");
}
