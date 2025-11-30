namespace MyShop.Core.Common;

/// <summary>
/// Result pattern wrapper for repository responses.
/// Used to convey Success/Failure results from Plugins layer to Client layer.
/// Provides a clean way to handle errors without throwing exceptions.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The data returned by the operation. Null if operation failed.
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Error message describing the failure. Empty string if successful.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// The exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Private constructor to enforce factory method usage.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="data">The result data.</param>
    /// <param name="errorMessage">The error message if failed.</param>
    /// <param name="exception">The exception if any.</param>
    private Result(bool isSuccess, T? data, string errorMessage, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful result with the specified data.
    /// </summary>
    /// <param name="data">The data to wrap in the result.</param>
    /// <returns>A successful Result containing the data.</returns>
    public static Result<T> Success(T data) => new(true, data, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A failed Result containing the error information.</returns>
    public static Result<T> Failure(string errorMessage, Exception? exception = null) 
        => new(false, default, errorMessage, exception);
}
