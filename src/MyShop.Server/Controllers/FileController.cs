using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller for handling file uploads
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileUploadService fileUploadService, ILogger<FileController> logger)
    {
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single image file
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="identifier">Optional identifier for the file (defaults to new GUID)</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file, [FromForm] string? identifier = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded", 400));
            }

            var id = string.IsNullOrEmpty(identifier) ? Guid.NewGuid().ToString() : identifier;

            var imageUrl = await _fileUploadService.UploadImageAsync(file, id);

            return Ok(ApiResponse<string>.SuccessResponse(
                imageUrl,
                "File uploaded successfully",
                200));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to upload file", 500));
        }
    }
}
