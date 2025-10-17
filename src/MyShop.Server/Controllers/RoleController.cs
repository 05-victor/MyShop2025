
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Responses;
using MyShop.Server.Services.Interfaces;

namespace MyShop.Server.Controllers;
[ApiController]
[Route("api/v1/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    private readonly ILogger<RoleController> _logger;


    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all available roles
    /// </summary>
    /// <returns>Standardized API response with list of roles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponse>>>> GetRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();

            return Ok(ApiResponse<IEnumerable<RoleResponse>>.SuccessResponse(
                roles,
                "Roles retrieved successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRoles endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }
}
