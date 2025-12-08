using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        IFileUploadService fileUploadService,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductResponse>>>> GetAllAsync([FromQuery] PaginationRequest request)
    {
        var pagedResult = await _productService.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<ProductResponse>>.SuccessResponse(pagedResult));
    }

    [HttpGet("{id:guid}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetByIdAsync([FromRoute] Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
       
        return Ok(ApiResponse<ProductResponse>.SuccessResponse(product));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> CreateAsync([FromBody] CreateProductRequest createProductRequest)
    {
        var createdProduct = await _productService.CreateAsync(createProductRequest);
        return CreatedAtRoute("GetProductById", new { id = createdProduct.Id }, 
            ApiResponse<ProductResponse>.SuccessResponse(createdProduct, "Product created successfully", 201));
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateProductRequest updateProductRequest)
    {
        var updatedProduct = await _productService.UpdateAsync(id, updateProductRequest);
        return Ok(ApiResponse<ProductResponse>.SuccessResponse(updatedProduct, "Product updated successfully", 200));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse>> DeleteAsync([FromRoute] Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse.ErrorResponse("Product not found", 404));
        }
        return NoContent();
    }

    [HttpPost("{id:guid}/uploadImage")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> UploadProductImage([FromRoute] Guid id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded", 400));
            }

            // Verify product exists
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Product not found", 404));
            }

            // Upload file to external service
            var imageUrl = await _fileUploadService.UploadImageAsync(file, $"product_{id}");

            // Update product with new image URL
            var updateRequest = new UpdateProductRequest
            {
                ImageUrl = imageUrl
            };

            var updatedProduct = await _productService.UpdateAsync(id, updateRequest);

            return Ok(ApiResponse<string>.SuccessResponse(
                imageUrl,
                "Product image uploaded successfully",
                200));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt for product {ProductId}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for product {ProductId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to upload product image", 500));
        }
    }
}

