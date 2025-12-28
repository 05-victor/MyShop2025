using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyShop.Client.Options;
using MyShop.Client.Services;
using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Client.Facades;

/// <summary>
/// Facade for Categories API operations
/// </summary>
public class CategoriesFacade
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public CategoriesFacade(HttpClient httpClient, IOptions<ApiOptions> apiOptions)
    {
        _httpClient = httpClient;
        _apiUrl = apiOptions.Value.BaseUrl;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<Result<List<CategoryResponse>>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/api/categories");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<List<CategoryResponse>>.Failure(error);
            }

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
            return Result<List<CategoryResponse>>.Success(categories ?? new List<CategoryResponse>());
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to get categories", ex);
            return Result<List<CategoryResponse>>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/api/categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<CategoryResponse>.Failure(error);
            }

            var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
            return category != null
                ? Result<CategoryResponse>.Success(category)
                : Result<CategoryResponse>.Failure("Category not found");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Failed to get category {id}", ex);
            return Result<CategoryResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Create new category
    /// </summary>
    public async Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}/api/categories", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<CategoryResponse>.Failure(error);
            }

            var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
            return category != null
                ? Result<CategoryResponse>.Success(category)
                : Result<CategoryResponse>.Failure("Failed to create category");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to create category", ex);
            return Result<CategoryResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Update category
    /// </summary>
    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"{_apiUrl}/api/categories/{id}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<CategoryResponse>.Failure(error);
            }

            var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
            return category != null
                ? Result<CategoryResponse>.Success(category)
                : Result<CategoryResponse>.Failure("Failed to update category");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Failed to update category {id}", ex);
            return Result<CategoryResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Delete category
    /// </summary>
    public async Task<Result<Unit>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/api/categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<Unit>.Failure(error);
            }

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Failed to delete category {id}", ex);
            return Result<Unit>.Failure(ex.Message);
        }
    }
}
