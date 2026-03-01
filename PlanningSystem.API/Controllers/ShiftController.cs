using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Application.Common;

namespace PlanningSystem.API.Controllers;

[ApiController]
[Route("shift")]
[Authorize]
public class ShiftController : BaseController
{
    private readonly IShiftApplicationService _shiftService;

    public ShiftController(IShiftApplicationService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultObject<List<Shift>>), 200)]
    public IActionResult GetShifts([FromQuery] ShiftFilterRequest? filter)
    {
        var res = _shiftService.GetShifts(filter, UserId);
        return HandleResult(res);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetShift(Guid id)
    {
        var res = _shiftService.GetShift(id, UserId);
        return HandleResult(res);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
    [ProducesResponseType(400)]
    public IActionResult CreateShift([FromBody] ShiftCreateRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _shiftService.CreateShift(request, UserId);
        return HandleResult(res);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult UpdateShift(Guid id, [FromBody] ShiftUpdateRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _shiftService.UpdateShift(id, request, UserId);
        return HandleResult(res);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResultObject<bool>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult DeleteShift(Guid id)
    {
        var res = _shiftService.DeleteShift(id, UserId);
        return HandleResult(res);
    }

    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ResultObject<Shift>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult UpdateShiftStatus(Guid id, [FromBody] ShiftStatusUpdateRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _shiftService.UpdateShiftStatus(id, request, UserId);
        return HandleResult(res);
    }
}
