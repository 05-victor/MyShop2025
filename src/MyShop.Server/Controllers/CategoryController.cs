using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetAllAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse(categories));
        }
        [HttpGet("{id:guid}", Name = "GetCategoryById")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetByIdAsync([FromRoute] Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category is null)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found", 404));
            }
            return Ok(ApiResponse<CategoryResponse>.SuccessResponse(category));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), 201)]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateAsync([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            var createdCategory = await _categoryService.CreateAsync(createCategoryRequest);
            // Provide a URI for the created resource as the first argument
            return CreatedAtRoute("GetCategoryById",new { id = createdCategory.Id }, ApiResponse<CategoryResponse>.SuccessResponse(createdCategory, "Category created successfully", 201));
        }
    }
}