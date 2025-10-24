using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Entities;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Responses;
using System.Security.Claims;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("activate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateUserResponse>>> ActivateMe([FromQuery] string activateCode)
    {
        _logger.LogInformation("ActivateMe called with activateCode: {ActivateCode}", activateCode);
        var result = await _userService.ActivateUserAsync(activateCode);

        if (result.Success)
        {
            return Ok(ApiResponse<ActivateUserResponse>.SuccessResponse(result));
        }
        else
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Activation failed"));
        }
    }
}
