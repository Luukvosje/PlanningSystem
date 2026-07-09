using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Auth;
using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Application.Organizations;
using Planning.Domain.Organizations;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController : ApiControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IModuleService _moduleService;
    private readonly IValidator<CreateOrganizationRequest> _createValidator;
    private readonly IValidator<UpdateModulesRequest> _updateModulesValidator;

    public OrganizationsController(
        IOrganizationService organizationService,
        IModuleService moduleService,
        IValidator<CreateOrganizationRequest> createValidator,
        IValidator<UpdateModulesRequest> updateModulesValidator)
    {
        _organizationService = organizationService;
        _moduleService = moduleService;
        _createValidator = createValidator;
        _updateModulesValidator = updateModulesValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] CreateOrganizationRequest request) =>
        ValidateAndExecuteAsync(request, _createValidator, async () =>
        {
            var result = await _organizationService.CreateForAccountAsync(request, HttpContext.RequestAborted);
            return result.ToCreatedActionResult(
                this,
                nameof(GetById),
                value => new { id = value!.Organization.Id });
        });

    [HttpGet("current")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await _organizationService.GetCurrentAsync(HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationMembershipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
    {
        var result = await _organizationService.GetMineAsync(HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _organizationService.GetByIdAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpPut("current/modules")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateCurrentModules([FromBody] UpdateModulesRequest request) =>
        ValidateAndExecuteAsync(request, _updateModulesValidator, async () =>
        {
            var result = await _moduleService.UpdateOrganizationModulesAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });
}
