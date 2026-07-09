using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Availability;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize(Policy = "RequirePlanningModule")]
public class AvailabilityController : ApiControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IValidator<WeekAvailabilityRequest> _weekValidator;
    private readonly IValidator<UpsertDayPartAvailabilityRequest> _dayPartValidator;
    private readonly IValidator<UpsertTimeBlockAvailabilityRequest> _timeBlockValidator;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        IValidator<WeekAvailabilityRequest> weekValidator,
        IValidator<UpsertDayPartAvailabilityRequest> dayPartValidator,
        IValidator<UpsertTimeBlockAvailabilityRequest> timeBlockValidator)
    {
        _availabilityService = availabilityService;
        _weekValidator = weekValidator;
        _dayPartValidator = dayPartValidator;
        _timeBlockValidator = timeBlockValidator;
    }

    [HttpGet("week")]
    [ProducesResponseType(typeof(WeekAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetWeek([FromQuery] WeekAvailabilityRequest request) =>
        ValidateAndExecuteAsync(request, _weekValidator, async () =>
        {
            var result = await _availabilityService.GetWeekAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPut("day-parts")]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpsertDayPart([FromBody] UpsertDayPartAvailabilityRequest request) =>
        ValidateAndExecuteAsync(request, _dayPartValidator, async () =>
        {
            var result = await _availabilityService.UpsertDayPartAsync(request, HttpContext.RequestAborted);
            if (result.IsSuccess && result.Value is null)
            {
                return NoContent();
            }

            return result.ToActionResult(this);
        });

    [HttpPut("time-blocks")]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpsertTimeBlock([FromBody] UpsertTimeBlockAvailabilityRequest request) =>
        ValidateAndExecuteAsync(request, _timeBlockValidator, async () =>
        {
            var result = await _availabilityService.UpsertTimeBlockAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _availabilityService.DeleteAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }
}
