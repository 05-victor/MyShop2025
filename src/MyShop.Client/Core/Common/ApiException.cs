using System;

namespace MyShop.Client.Core.Common;

/// <summary>
/// Custom exception cho API errors với status code chuẩn
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseContent { get; }
    public ApiErrorType ErrorType { get; }

    public ApiException(int statusCode, string message, string? responseContent = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        ErrorType = GetErrorType(statusCode);
    }

    private static ApiErrorType GetErrorType(int statusCode)
    {
        return statusCode switch
        {
            400 => ApiErrorType.BadRequest,
            401 => ApiErrorType.Unauthorized,
            403 => ApiErrorType.Forbidden,
            404 => ApiErrorType.NotFound,
            500 => ApiErrorType.ServerError,
            _ => ApiErrorType.Unknown
        };
    }

    public string GetUserFriendlyMessage()
    {
        return ErrorType switch
        {
            ApiErrorType.BadRequest => "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.",
            ApiErrorType.Unauthorized => "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
            ApiErrorType.Forbidden => "Bạn không có quyền thực hiện thao tác này.",
            ApiErrorType.NotFound => "Không tìm thấy dữ liệu yêu cầu.",
            ApiErrorType.ServerError => "Lỗi server. Vui lòng thử lại sau.",
            _ => "Đã xảy ra lỗi. Vui lòng thử lại."
        };
    }
}

public enum ApiErrorType
{
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
    ServerError,
    Unknown
}
