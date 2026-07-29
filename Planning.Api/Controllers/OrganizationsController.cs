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
    private readonly IValidator<UpdateOrganizationRequest> _updateValidator;
    private readonly IValidator<UpdateOrganizationPlanningSettingsRequest> _updatePlanningSettingsValidator;
    private readonly IValidator<UpdateModulesRequest> _updateModulesValidator;

    public OrganizationsController(
        IOrganizationService organizationService,
        IModuleService moduleService,
        IValidator<CreateOrganizationRequest> createValidator,
        IValidator<UpdateOrganizationRequest> updateValidator,
        IValidator<UpdateOrganizationPlanningSettingsRequest> updatePlanningSettingsValidator,
        IValidator<UpdateModulesRequest> updateModulesValidator)
    {
        _organizationService = organizationService;
        _moduleService = moduleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updatePlanningSettingsValidator = updatePlanningSettingsValidator;
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

    [HttpPut("current")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateCurrent([FromBody] UpdateOrganizationRequest request) =>
        ValidateAndExecuteAsync(request, _updateValidator, async () =>
        {
            var result = await _organizationService.UpdateCurrentAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPut("current/planning-settings")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateCurrentPlanningSettings(
        [FromBody] UpdateOrganizationPlanningSettingsRequest request) =>
        ValidateAndExecuteAsync(request, _updatePlanningSettingsValidator, async () =>
        {
            var result = await _organizationService.UpdateCurrentPlanningSettingsAsync(
                request,
                HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

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

    [HttpGet("current/logo")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentLogo()
    {
        var result = await _organizationService.GetCurrentLogoAsync(HttpContext.RequestAborted);

        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        return File(result.Value.Content, result.Value.ContentType);
    }

    [HttpPost("current/logo")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [RequestSizeLimit(2_097_152)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(OrganizationLogoUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCurrentLogo(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded.", errorCode = "VALIDATION_ERROR" });
        }

        await using var stream = file.OpenReadStream();
        var result = await _organizationService.UploadCurrentLogoAsync(
            stream,
            file.ContentType,
            file.Length,
            HttpContext.RequestAborted);

        return result.ToActionResult(this);
    }
}
