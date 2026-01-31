using Microsoft.AspNetCore.Mvc;
using PlanningSystem.DAL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;

namespace PlanningSystem.API.Controllers
{
    /// <summary>
    /// Controller for user management and authentication
    /// </summary>
    [ApiController]
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController(AppDbContext dbContext, IConfiguration configuration, IBllFactory bllFactory)
          : base(dbContext, configuration, bllFactory)
        {
        }

        /// <summary>
        /// Create a new user account
        /// </summary>
        /// <param name="request">User registration information</param>
        /// <returns>Created user information</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ResultObject<Models.Models.User>), 200)]
        [ProducesResponseType(400)]
        public IActionResult CreateUser([FromBody] UserAddRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.UserLogic.CreateUser(request);
            return HandleResult(res);
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ResultObject<UserAuthResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult AuthenticateUser([FromBody] UserAuthRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var jwtKey = _jwtKey;
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, "JWT configuration is missing");
            }

            var res = BllFactory.UserLogic.AuthenticateUser(request, jwtKey);
            return HandleResult(res);
        }
    }
}
