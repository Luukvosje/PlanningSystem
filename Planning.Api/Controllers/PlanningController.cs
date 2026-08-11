using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Common;
using Planning.Application.Planning;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/planning")]
[Authorize(Policy = "RequirePlanningModule")]
public class PlanningController : ApiControllerBase
{
    private readonly IPlanningService _planningService;
    private readonly IValidator<CreatePlanningRequest> _createValidator;
    private readonly IValidator<UpdatePlanningRequest> _updateValidator;
    private readonly IValidator<MovePlanningRequest> _moveValidator;
    private readonly IValidator<DuplicatePlanningRequest> _duplicateValidator;
    private readonly IValidator<PlanningListRequest> _listValidator;
    private readonly IValidator<WeekPlanningRequest> _weekValidator;

    public PlanningController(
        IPlanningService planningService,
        IValidator<CreatePlanningRequest> createValidator,
        IValidator<UpdatePlanningRequest> updateValidator,
        IValidator<MovePlanningRequest> moveValidator,
        IValidator<DuplicatePlanningRequest> duplicateValidator,
        IValidator<PlanningListRequest> listValidator,
        IValidator<WeekPlanningRequest> weekValidator)
    {
        _planningService = planningService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _moveValidator = moveValidator;
        _duplicateValidator = duplicateValidator;
        _listValidator = listValidator;
        _weekValidator = weekValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlanningListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetList([FromQuery] PlanningListRequest request) =>
        ValidateAndExecuteAsync(request, _listValidator, async () =>
        {
            var result = await _planningService.GetListAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPost]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] CreatePlanningRequest request) =>
        ValidateAndExecuteAsync(request, _createValidator, async () =>
        {
            var result = await _planningService.CreateAsync(request, HttpContext.RequestAborted);
            return result.ToCreatedActionResult(
                this,
                nameof(GetById),
                value => new { id = value!.Id });
        });

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdatePlanningRequest request) =>
        ValidateAndExecuteAsync(request, _updateValidator, async () =>
        {
            var result = await _planningService.UpdateAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPatch("{id:guid}/move")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Move(Guid id, [FromBody] MovePlanningRequest request) =>
        ValidateAndExecuteAsync(request, _moveValidator, async () =>
        {
            var result = await _planningService.MoveAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPatch("{id:guid}/confirm")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _planningService.ConfirmAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/duplicate")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Duplicate(Guid id, [FromBody] DuplicatePlanningRequest request) =>
        ValidateAndExecuteAsync(request, _duplicateValidator, async () =>
        {
            var result = await _planningService.DuplicateAsync(id, request, HttpContext.RequestAborted);
            return result.ToCreatedActionResult(
                this,
                nameof(GetById),
                value => new { id = value!.Id });
        });

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _planningService.DeleteAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _planningService.GetByIdAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet("week")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanningResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetWeekPlanning([FromQuery] WeekPlanningRequest request) =>
        ValidateAndExecuteAsync(request, _weekValidator, async () =>
        {
            var result = await _planningService.GetWeekPlanningAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });
}
