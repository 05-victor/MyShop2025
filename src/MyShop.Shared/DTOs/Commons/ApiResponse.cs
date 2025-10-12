namespace MyShop.Shared.DTOs.Common;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Response message (success message or error description)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data/results being returned
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success => Code >= 200 && Code < 300;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int code = 200)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Result = data
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, int code = 400)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Result = default
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Code = 404,
            Message = message,
            Result = default
        };
    }

    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            Code = 401,
            Message = message,
            Result = default
        };
    }

    /// <summary>
    /// Creates a server error response
    /// </summary>
    public static ApiResponse<T> ServerErrorResponse(string message = "Internal server error")
    {
        return new ApiResponse<T>
        {
            Code = 500,
            Message = message,
            Result = default
        };
    }
}

/// <summary>
/// Non-generic version for responses without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Success", int code = 200)
    {
        return new ApiResponse
        {
            Code = code,
            Message = message,
            Result = null
        };
    }

    /// <summary>
    /// Creates an error response without data
    /// </summary>
    public static ApiResponse ErrorResponse(string message, int code = 400)
    {
        return new ApiResponse
        {
            Code = code,
            Message = message,
            Result = null
        };
    }
}
