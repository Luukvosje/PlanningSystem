using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanningSystem.DAL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;

namespace PlanningSystem.API.Controllers
{
    /// <summary>
    /// Controller for organization management
    /// </summary>
    [ApiController]
    [Route("organization")]
    [Authorize]
    public class OrganizationController : BaseController
    {
        public OrganizationController(AppDbContext dbContext, IConfiguration configuration, IBllFactory bllFactory)
            : base(dbContext, configuration, bllFactory)
        {
        }

        /// <summary>
        /// Get all organizations for the current user
        /// </summary>
        /// <returns>List of organizations the user belongs to</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResultObject<List<Organization>>), 200)]
        public IActionResult GetUserOrganizations()
        {
            var res = BllFactory.OrganizationLogic.GetUserOrganizations(UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>Organization details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetOrganization(int id)
        {
            var res = BllFactory.OrganizationLogic.GetOrganization(id, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Create a new organization
        /// </summary>
        /// <param name="request">Organization creation data</param>
        /// <returns>Created organization</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
        [ProducesResponseType(400)]
        public IActionResult CreateOrganization([FromBody] OrganizationCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.OrganizationLogic.CreateOrganization(request, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Update an organization (requires Admin or Manager role)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <param name="request">Organization update data</param>
        /// <returns>Updated organization</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateOrganization(int id, [FromBody] OrganizationUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.OrganizationLogic.UpdateOrganization(id, request, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Delete an organization (requires Admin role)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResultObject<bool>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteOrganization(int id)
        {
            var res = BllFactory.OrganizationLogic.DeleteOrganization(id, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Get all users in an organization
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>List of users in the organization</returns>
        [HttpGet("{id}/users")]
        [ProducesResponseType(typeof(ResultObject<List<User>>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetOrganizationUsers(int id)
        {
            var res = BllFactory.OrganizationLogic.GetOrganizationUsers(id, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Add a user to an organization (requires Admin or Manager role)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <param name="request">User and role information</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/users")]
        [ProducesResponseType(typeof(ResultObject<bool>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult AddUserToOrganization(int id, [FromBody] OrganizationAddUserRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.OrganizationLogic.AddUserToOrganization(id, request, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Remove a user from an organization (requires Admin or Manager role)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <param name="userId">User ID to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/users/{userId}")]
        [ProducesResponseType(typeof(ResultObject<bool>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult RemoveUserFromOrganization(int id, int userId)
        {
            var res = BllFactory.OrganizationLogic.RemoveUserFromOrganization(id, userId, UserId);
            return HandleResult(res);
        }
    }
}
