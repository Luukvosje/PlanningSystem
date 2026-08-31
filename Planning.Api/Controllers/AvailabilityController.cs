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
    private readonly IAvailabilityRuleService _availabilityRuleService;
    private readonly IValidator<CreateAvailabilityRuleRequest> _createValidator;
    private readonly IValidator<UpdateAvailabilityRuleRequest> _updateValidator;
    private readonly IValidator<PlanningAvailabilityRequest> _planningValidator;

    public AvailabilityController(
        IAvailabilityRuleService availabilityRuleService,
        IValidator<CreateAvailabilityRuleRequest> createValidator,
        IValidator<UpdateAvailabilityRuleRequest> updateValidator,
        IValidator<PlanningAvailabilityRequest> planningValidator)
    {
        _availabilityRuleService = availabilityRuleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _planningValidator = planningValidator;
    }

    [HttpGet("rules")]
    [ProducesResponseType(typeof(AvailabilityRulesListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListRules([FromQuery] Guid employeeId)
    {
        var result = await _availabilityRuleService.ListByEmployeeAsync(employeeId, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet("rules/for-planning")]
    [ProducesResponseType(typeof(PlanningAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetForPlanning([FromQuery] PlanningAvailabilityRequest request) =>
        ValidateAndExecuteAsync(request, _planningValidator, async () =>
        {
            var result = await _availabilityRuleService.GetForPlanningAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    // Deliberately no action policy: recording your own absence is self-service, so authorization
    // depends on the EmployeeId in the body and is enforced in AvailabilityRuleService.
    [HttpPost("rules")]
    [ProducesResponseType(typeof(AvailabilityRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CreateRule([FromBody] CreateAvailabilityRuleRequest request) =>
        ValidateAndExecuteAsync(request, _createValidator, async () =>
        {
            var result = await _availabilityRuleService.CreateAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    // No action policy: changing your own absence is self-service too, and whether this caller may
    // touch this rule depends on the loaded rule - AvailabilityRuleService decides.
    [HttpPut("rules/{id:guid}")]
    [ProducesResponseType(typeof(AvailabilityRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateAvailabilityRuleRequest request) =>
        ValidateAndExecuteAsync(request, _updateValidator, async () =>
        {
            var result = await _availabilityRuleService.UpdateAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    // No action policy either: deleting is planner-only except for withdrawing your own pending
    // request, which depends on the loaded rule and is enforced in AvailabilityRuleService.
    [HttpDelete("rules/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        var result = await _availabilityRuleService.DeleteAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }
}
