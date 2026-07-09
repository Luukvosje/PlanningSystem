using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Common;
using Planning.Application.Invites;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/invites")]
[Authorize]
public class InvitesController : ApiControllerBase
{
    private readonly IInviteService _inviteService;
    private readonly IValidator<CreateInviteRequest> _createValidator;
    private readonly IValidator<AcceptInviteRequest> _acceptValidator;

    public InvitesController(
        IInviteService inviteService,
        IValidator<CreateInviteRequest> createValidator,
        IValidator<AcceptInviteRequest> acceptValidator)
    {
        _inviteService = inviteService;
        _createValidator = createValidator;
        _acceptValidator = acceptValidator;
    }

    [HttpPost]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [Authorize(Policy = "RequireBeheerModule")]
    [ProducesResponseType(typeof(InviteResponse), StatusCodes.Status201Created)]
    public Task<IActionResult> Create([FromBody] CreateInviteRequest request) =>
        ValidateAndExecuteAsync(request, _createValidator, async () =>
        {
            var result = await _inviteService.CreateAsync(request, HttpContext.RequestAborted);
            return result.ToCreatedActionResult(this, nameof(GetPreview), value => new { code = value!.Code });
        });

    [HttpPost("accept")]
    [ProducesResponseType(typeof(AcceptInviteResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> Accept([FromBody] AcceptInviteRequest request) =>
        ValidateAndExecuteAsync(request, _acceptValidator, async () =>
        {
            var result = await _inviteService.AcceptAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(InvitePreviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreview(string code)
    {
        var result = await _inviteService.GetPreviewAsync(code, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }
}
