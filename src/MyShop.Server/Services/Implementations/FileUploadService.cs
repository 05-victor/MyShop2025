using System.Text.Json;
using MyShop.Server.Services.Interfaces;
using SystemPath = System.IO.Path;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Implementation of file upload service using external Render-hosted file service
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileUploadService> _logger;
    private const string FileServiceUrl = "https://file-service-cdal.onrender.com/api/v1/file/uploads";

    public FileUploadService(IHttpClientFactory httpClientFactory, ILogger<FileUploadService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string identifier)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        // Validate file type (only images)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = SystemPath.GetExtension(file.FileName).ToLowerInvariant();
        
        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
        {
            throw new ArgumentException($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (max 5MB)
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
        }

        try
        {
            using var content = new MultipartFormDataContent();
            
            // Add identifier field
            content.Add(new StringContent(identifier), "id");
            
            // Add image file
            using var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "image", file.FileName);

            _logger.LogInformation("Uploading file {FileName} with identifier {Identifier} to external service", 
                file.FileName, identifier);

            var response = await _httpClient.PostAsync(FileServiceUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("File upload failed with status {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"File upload failed with status {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("File upload response: {Response}", responseContent);

            // Parse response to extract image URL
            var jsonResponse = JsonDocument.Parse(responseContent);
            var imageUrl = jsonResponse.RootElement
                .GetProperty("result")
                .GetProperty("image")
                .GetString();

            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new InvalidOperationException("Failed to extract image URL from response");
            }

            _logger.LogInformation("File uploaded successfully to {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} with identifier {Identifier}", 
                file.FileName, identifier);
            throw;
        }
    }
}
