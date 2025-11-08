namespace MyShop.Core.Common;

/// <summary>
/// Result pattern để wrap responses từ repositories
/// Dùng để truyền kết quả Success/Failure từ Plugins lên Client
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, T? data, string errorMessage, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Success(T data) => new(true, data, string.Empty);

    public static Result<T> Failure(string errorMessage, Exception? exception = null) 
        => new(false, default, errorMessage, exception);
}
