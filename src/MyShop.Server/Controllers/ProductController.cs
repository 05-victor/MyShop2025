using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Search products with advanced filtering, sorting, and pagination
    /// </summary>
    /// <param name="request">Search criteria including filters, sort options, and pagination</param>
    /// <returns>Paginated list of products matching the search criteria</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductResponse>>>> SearchAsync([FromBody] SearchProductsRequest request)
    {
        _logger.LogInformation("Search products endpoint called with filters");
        
        var pagedResult = await _productService.SearchAsync(request);
        
        return Ok(ApiResponse<PagedResult<ProductResponse>>.SuccessResponse(
            pagedResult, 
            $"Found {pagedResult.TotalCount} products", 
            200));
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
    [Authorize(Roles = "Admin,SalesAgent")]
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

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,SalesAgent")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateProductsResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BulkCreateProductsResponse>>> BulkCreateAsync([FromBody] BulkCreateProductsRequest request)
    {
        _logger.LogInformation("Bulk create endpoint called for {Count} products", request.Products.Count);

        var result = await _productService.BulkCreateAsync(request);

        var message = $"Bulk creation completed: {result.SuccessCount} succeeded, {result.FailureCount} failed";
        
        // Return 201 if at least one product was created, otherwise 400
        if (result.SuccessCount > 0)
        {
            return CreatedAtRoute(
                "GetProductById", 
                new { id = result.CreatedProducts.FirstOrDefault()?.Id ?? Guid.Empty }, 
                ApiResponse<BulkCreateProductsResponse>.SuccessResponse(result, message, 201));
        }
        
        return BadRequest(new ApiResponse<BulkCreateProductsResponse>
        {
            Code = 400,
            Message = message,
            Result = result
        });
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,SalesAgent")]
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
    [Authorize(Roles = "Admin,SalesAgent")]
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
    [Authorize(Roles = "Admin,SalesAgent")]
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

