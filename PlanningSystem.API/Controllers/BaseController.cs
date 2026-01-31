using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningSystem.DAL;
using PlanningSystem.Models.Models;
using System.Security.Claims;

namespace PlanningSystem.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected AppDbContext _dbContext;
        protected readonly string _adminApiKey;
        protected readonly string _jwtKey;
        protected IBllFactory BllFactory { get; set; }

        protected int UserId
        {
            get
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
        }

        public BaseController(AppDbContext dbContext, IConfiguration configuration, IBllFactory bllFactory)
        {
            this.BllFactory = bllFactory;
            _dbContext = dbContext;
            _adminApiKey = configuration.GetValue<string>("AppSettings:AdminApiKey");
            _jwtKey = configuration.GetValue<string>("Jwt:Key");
        }


        protected IActionResult HandleResult<T>(ResultObject<T> result)
        {
            if (result == null)
                return StatusCode(500, "An unexpected error occurred");

            if (result.Success)
                return Ok(result);

            // Handle specific exception types
            if (result.Exception != null)
            {
                // Rate limiting or too many requests
                if (result.Exception is UnauthorizedAccessException)
                    return Unauthorized(result.Message ?? "Unauthorized access");

                // Not found errors
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result.Message);

                // Forbidden errors
                if (result.Message?.Contains("permission", StringComparison.OrdinalIgnoreCase) == true ||
                    result.Message?.Contains("access", StringComparison.OrdinalIgnoreCase) == true)
                    return Forbid(result.Message ?? "Access denied");
            }

            return BadRequest(result.Message ?? "An error occurred processing your request");
        }

    }
}
