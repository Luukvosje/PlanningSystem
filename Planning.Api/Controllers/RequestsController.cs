using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Requests;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize(Policy = "RequirePlanningModule")]
public class RequestsController : ApiControllerBase
{
    private readonly IRequestService _requestService;
    private readonly IValidator<DecideRequestsRequest> _decideValidator;

    public RequestsController(
        IRequestService requestService,
        IValidator<DecideRequestsRequest> decideValidator)
    {
        _requestService = requestService;
        _decideValidator = decideValidator;
    }

    // Deliberately no action policy: a member may see their own requests, a planner sees the whole
    // organization, and that split is enforced in RequestService.
    [HttpGet]
    [ProducesResponseType(typeof(RequestListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List([FromQuery] bool includeDecided = false)
    {
        var result = await _requestService.ListAsync(includeDecided, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpPost("approve")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(DecideRequestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IActionResult> Approve([FromBody] DecideRequestsRequest request) =>
        ValidateAndExecuteAsync(request, _decideValidator, async () =>
        {
            var result = await _requestService.ApproveAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPost("reject")]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(DecideRequestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IActionResult> Reject([FromBody] DecideRequestsRequest request) =>
        ValidateAndExecuteAsync(request, _decideValidator, async () =>
        {
            var result = await _requestService.RejectAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });
}
