using Microsoft.AspNetCore.Mvc;
using PlanningSystem.Application.Common;
using PlanningSystem.Domain.Exceptions;
using System.Security.Claims;

namespace PlanningSystem.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int UserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }

    protected IActionResult HandleResult<T>(ResultObject<T> result)
    {
        if (result == null)
            return StatusCode(500, "An unexpected error occurred");

        if (result.Success)
            return Ok(result);

        if (result.Exception != null)
        {
            if (result.Exception is UnauthorizedException)
                return Unauthorized(result.Message ?? "Unauthorized access");

            if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(result.Message);

            if (result.Message?.Contains("permission", StringComparison.OrdinalIgnoreCase) == true ||
                result.Message?.Contains("access", StringComparison.OrdinalIgnoreCase) == true)
                return StatusCode(403, result.Message ?? "Access denied");
        }

        return BadRequest(result.Message ?? "An error occurred processing your request");
    }
}
