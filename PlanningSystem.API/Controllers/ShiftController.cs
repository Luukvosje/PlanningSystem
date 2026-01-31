using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanningSystem.DAL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;

namespace PlanningSystem.API.Controllers
{
    /// <summary>
    /// Controller for shift/roster management
    /// </summary>
    [ApiController]
    [Route("shift")]
    [Authorize]
    public class ShiftController : BaseController
    {
        public ShiftController(AppDbContext dbContext, IConfiguration configuration, IBllFactory bllFactory)
            : base(dbContext, configuration, bllFactory)
        {
        }

        /// <summary>
        /// Get shifts with optional filters (organization, worker, date range, status)
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>List of shifts matching the filters</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResultObject<List<Shift>>), 200)]
        public IActionResult GetShifts([FromQuery] ShiftFilterRequest filter)
        {
            var res = BllFactory.ShiftLogic.GetShifts(filter, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Get shift by ID
        /// </summary>
        /// <param name="id">Shift ID (Guid)</param>
        /// <returns>Shift details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetShift(Guid id)
        {
            var res = BllFactory.ShiftLogic.GetShift(id, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Create a new shift
        /// </summary>
        /// <param name="request">Shift creation data</param>
        /// <returns>Created shift</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
        [ProducesResponseType(400)]
        public IActionResult CreateShift([FromBody] ShiftCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.ShiftLogic.CreateShift(request, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Update a shift (requires Admin, Manager role, or be the assigned worker)
        /// </summary>
        /// <param name="id">Shift ID (Guid)</param>
        /// <param name="request">Shift update data</param>
        /// <returns>Updated shift</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateShift(Guid id, [FromBody] ShiftUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.ShiftLogic.UpdateShift(id, request, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Delete a shift (requires Admin, Manager role, or be the assigned worker)
        /// </summary>
        /// <param name="id">Shift ID (Guid)</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResultObject<bool>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteShift(Guid id)
        {
            var res = BllFactory.ShiftLogic.DeleteShift(id, UserId);
            return HandleResult(res);
        }

        /// <summary>
        /// Update shift status (scheduled, finished, missed)
        /// </summary>
        /// <param name="id">Shift ID (Guid)</param>
        /// <param name="request">Status update data</param>
        /// <returns>Updated shift</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateShiftStatus(Guid id, [FromBody] ShiftStatusUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var res = BllFactory.ShiftLogic.UpdateShiftStatus(id, request, UserId);
            return HandleResult(res);
        }
    }
}
