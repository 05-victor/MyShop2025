namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service for uploading files to external file storage service
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload an image file to the external file service
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="identifier">Unique identifier for the file (e.g., userId, productId)</param>
    /// <returns>The URL of the uploaded image</returns>
    Task<string> UploadImageAsync(IFormFile file, string identifier);
}
