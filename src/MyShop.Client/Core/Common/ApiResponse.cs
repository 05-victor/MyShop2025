using System.Text.Json.Serialization;

namespace MyShop.Client.Core.Common;

/// <summary>
/// Response wrapper chuẩn từ API (theo quy chuẩn backend)
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu result</typeparam>
public class ApiResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

/// <summary>
/// Response wrapper không có result (cho các action thành công mà không trả data)
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}

/// <summary>
/// Pagination metadata theo quy chuẩn
/// </summary>
public class PaginationMetadata
{
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

/// <summary>
/// Response cho danh sách có phân trang
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của từng item</typeparam>
public class PaginatedResult<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationMetadata? Pagination { get; set; }
}
