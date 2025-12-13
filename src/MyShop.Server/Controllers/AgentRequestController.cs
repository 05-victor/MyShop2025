using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/agent-requests")]
public class AgentRequestController : ControllerBase
{
    private readonly IAgentRequestService _agentRequestService;
    private readonly ILogger<AgentRequestController> _logger;

    public AgentRequestController(IAgentRequestService agentRequestService, ILogger<AgentRequestController> logger)
    {
        _agentRequestService = agentRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new agent request (User sends request to become SalesAgent)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AgentRequestResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AgentRequestResponse>>> Create([FromBody] CreateAgentRequestRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid request data"));
            }

            var result = await _agentRequestService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                ApiResponse<AgentRequestResponse>.SuccessResponse(result, "Agent request created successfully", 201));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in Create agent request");
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in Create agent request");
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent request");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while creating the agent request"));
        }
    }

    /// <summary>
    /// Get all agent requests with pagination (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AgentRequestResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<AgentRequestResponse>>>> GetAll(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? status = null)
    {
        try
        {
            var pagedResult = await _agentRequestService.GetAllAsync(request, status);
            return Ok(ApiResponse<PagedResult<AgentRequestResponse>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent requests");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while retrieving agent requests"));
        }
    }

    /// <summary>
    /// Get current user's agent request
    /// </summary>
    [HttpGet("my-request")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AgentRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AgentRequestResponse>>> GetMyRequest()
    {
        try
        {
            var result = await _agentRequestService.GetMyRequestAsync();
            if (result == null)
            {
                return NotFound(ApiResponse.NotFoundResponse("No agent request found for current user"));
            }

            return Ok(ApiResponse<AgentRequestResponse>.SuccessResponse(result, "Agent request retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user's agent request");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while retrieving the agent request"));
        }
    }

    /// <summary>
    /// Get agent request by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AgentRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AgentRequestResponse>>> GetById(Guid id)
    {
        try
        {
            var result = await _agentRequestService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse.NotFoundResponse($"Agent request with ID {id} not found"));
            }

            return Ok(ApiResponse<AgentRequestResponse>.SuccessResponse(result, "Agent request retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent request {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while retrieving the agent request"));
        }
    }

    /// <summary>
    /// Approve an agent request (Admin only) - Promotes user to SalesAgent role
    /// </summary>
    [HttpPatch("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateUserResponse>>> Approve(Guid id)
    {
        try
        {
            var result = await _agentRequestService.ApproveAsync(id);
            if (result.Success)
            {
                return Ok(ApiResponse<ActivateUserResponse>.SuccessResponse(result));
            }
            else
            {
                return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Failed to approve agent request"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving agent request {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while approving the agent request"));
        }
    }

    /// <summary>
    /// Reject an agent request (Admin only)
    /// </summary>
    [HttpPatch("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateUserResponse>>> Reject(
        Guid id,
        [FromQuery] string? reason = null)
    {
        try
        {
            var result = await _agentRequestService.RejectAsync(id, reason);
            if (result.Success)
            {
                return Ok(ApiResponse<ActivateUserResponse>.SuccessResponse(result));
            }
            else
            {
                return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Failed to reject agent request"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting agent request {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while rejecting the agent request"));
        }
    }
}
