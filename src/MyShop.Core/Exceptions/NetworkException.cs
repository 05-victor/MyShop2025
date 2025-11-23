namespace MyShop.Core.Exceptions;

/// <summary>
/// Exception for network-related errors (connection, timeout, etc.)
/// </summary>
public class NetworkException : BaseException
{
    public NetworkException(string message)
        : base(message, "NETWORK_ERROR")
    {
    }

    public NetworkException(string message, Exception innerException)
        : base(message, "NETWORK_ERROR", innerException)
    {
    }

    public static NetworkException ConnectionFailed(string details = "")
        => new($"Cannot connect to server. {details}".Trim());

    public static NetworkException Timeout()
        => new("Request timeout. Please try again.");

    public static NetworkException NoInternet()
        => new("No internet connection. Please check your network.");
}
