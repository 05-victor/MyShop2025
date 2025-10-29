using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShop.Client.Core.Common;

/// <summary>
/// API Client tái sử dụng cho mọi endpoint
/// Tự động xử lý status code, token, serialization theo quy chuẩn
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    /// <summary>
    /// Set Bearer token cho các request
    /// </summary>
    public void SetToken(string? token)
    {
        _token = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// GET request
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu result</typeparam>
    /// <param name="endpoint">Endpoint (ví dụ: "/api/v1/products")</param>
    /// <param name="queryParams">Query parameters (optional)</param>
    public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var url = BuildUrl(endpoint, queryParams);
        var response = await _httpClient.GetAsync(url);
        return await ApiResponseHandler.HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// POST request
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu result</typeparam>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="data">Request body data</param>
    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var content = CreateJsonContent(data);
        var response = await _httpClient.PostAsync(endpoint, content);
        return await ApiResponseHandler.HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// POST request không có result (chỉ success/message)
    /// </summary>
    public async Task PostAsync(string endpoint, object data)
    {
        var content = CreateJsonContent(data);
        var response = await _httpClient.PostAsync(endpoint, content);
        await ApiResponseHandler.HandleResponseNoResultAsync(response);
    }

    /// <summary>
    /// PUT request
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu result</typeparam>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="data">Request body data</param>
    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var content = CreateJsonContent(data);
        var response = await _httpClient.PutAsync(endpoint, content);
        return await ApiResponseHandler.HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// PUT request không có result
    /// </summary>
    public async Task PutAsync(string endpoint, object data)
    {
        var content = CreateJsonContent(data);
        var response = await _httpClient.PutAsync(endpoint, content);
        await ApiResponseHandler.HandleResponseNoResultAsync(response);
    }

    /// <summary>
    /// DELETE request
    /// </summary>
    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        await ApiResponseHandler.HandleResponseNoResultAsync(response);
    }

    /// <summary>
    /// DELETE request với result
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu result</typeparam>
    public async Task<T?> DeleteAsync<T>(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        return await ApiResponseHandler.HandleResponseAsync<T>(response);
    }

    #region Helper Methods

    private string BuildUrl(string endpoint, Dictionary<string, string>? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
            return endpoint;

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{endpoint}?{queryString}";
    }

    private StringContent CreateJsonContent(object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    #endregion
}
