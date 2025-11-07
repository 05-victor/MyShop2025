namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// Represents the result of a service operation.
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Message describing the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional error code for failed operations.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Creates a successful service result.
        /// </summary>
        public static ServiceResult Success(string message = "Operation completed successfully")
        {
            return new ServiceResult
            {
                IsSuccess = true,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failed service result.
        /// </summary>
        public static ServiceResult Failure(string message, string? errorCode = null)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Represents the result of a service operation with a return value.
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// The data returned by the operation.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful service result with data.
        /// </summary>
        public static ServiceResult<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failed service result.
        /// </summary>
        public new static ServiceResult<T> Failure(string message, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
