using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShop.Client.Core.Common;

/// <summary>
/// Handler xử lý response từ API theo quy chuẩn status code
/// Tái sử dụng cho mọi API call
/// </summary>
public static class ApiResponseHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Xử lý response và deserialize theo quy chuẩn
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu result</typeparam>
    /// <param name="response">HttpResponseMessage từ API</param>
    /// <returns>Dữ liệu result nếu thành công</returns>
    /// <exception cref="ApiException">Throw nếu có lỗi</exception>
    public static async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        // Xử lý theo status code
        switch ((int)response.StatusCode)
        {
            case 200: // OK
            case 201: // Created
                return await DeserializeSuccessResponse<T>(content);

            case 400: // Bad Request
                throw new ApiException(400, "Dữ liệu không hợp lệ", content);

            case 401: // Unauthorized
                throw new ApiException(401, "Chưa xác thực hoặc token hết hạn", content);

            case 403: // Forbidden
                throw new ApiException(403, "Không đủ quyền truy cập", content);

            case 404: // Not Found
                throw new ApiException(404, "Không tìm thấy tài nguyên", content);

            case 500: // Server Error
                throw new ApiException(500, "Lỗi server", content);

            default:
                throw new ApiException((int)response.StatusCode, $"Lỗi không xác định: {response.StatusCode}", content);
        }
    }

    /// <summary>
    /// Deserialize response thành công (200/201)
    /// </summary>
    private static async Task<T?> DeserializeSuccessResponse<T>(string content)
    {
        try
        {
            // Deserialize ApiResponse wrapper
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);

            if (apiResponse == null)
            {
                throw new ApiException(500, "Response null từ server", content);
            }

            if (!apiResponse.Success)
            {
                throw new ApiException(apiResponse.Code, apiResponse.Message, content);
            }

            return apiResponse.Result;
        }
        catch (JsonException ex)
        {
            throw new ApiException(500, $"Lỗi parse JSON: {ex.Message}", content);
        }
    }

    /// <summary>
    /// Xử lý response không có result (chỉ success/message)
    /// </summary>
    public static async Task HandleResponseNoResultAsync(HttpResponseMessage response)
    {
        await HandleResponseAsync<object>(response);
    }

    /// <summary>
    /// Lấy error message từ response content (nếu có)
    /// </summary>
    public static string ExtractErrorMessage(string? responseContent)
    {
        if (string.IsNullOrEmpty(responseContent))
            return "Không có thông tin lỗi";

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, JsonOptions);
            return apiResponse?.Message ?? "Lỗi không xác định";
        }
        catch
        {
            return responseContent;
        }
    }
}
