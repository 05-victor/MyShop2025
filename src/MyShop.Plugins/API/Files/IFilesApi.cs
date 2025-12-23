using Refit;

namespace MyShop.Plugins.API.Files;

/// <summary>
/// DTO for file upload response
/// </summary>
public class FileUploadResponse
{
    public bool Success { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Refit interface for Files API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IFilesApi
{
    /// <summary>
    /// Upload an image file
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="identifier">Optional identifier for the file</param>
    /// <returns>The URL of the uploaded image</returns>
    [Multipart]
    [Post("/api/v1/files/upload")]
    Task<Refit.ApiResponse<FileUploadResponse>> UploadImageAsync([AliasAs("file")] StreamPart file, [AliasAs("identifier")] string? identifier = null);
}
