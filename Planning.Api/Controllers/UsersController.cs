using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Common;
using Planning.Application.Modules;
using Planning.Application.Users;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UpdateUserRoleRequest> _updateRoleValidator;
    private readonly IValidator<UpdateUserStatusRequest> _updateStatusValidator;
    private readonly IValidator<UpdateUserApprovalRequest> _updateApprovalValidator;
    private readonly IValidator<UpdateModulesRequest> _updateModulesValidator;

    public UsersController(
        IUserService userService,
        IValidator<UpdateUserRoleRequest> updateRoleValidator,
        IValidator<UpdateUserStatusRequest> updateStatusValidator,
        IValidator<UpdateUserApprovalRequest> updateApprovalValidator,
        IValidator<UpdateModulesRequest> updateModulesValidator)
    {
        _userService = userService;
        _updateRoleValidator = updateRoleValidator;
        _updateStatusValidator = updateStatusValidator;
        _updateApprovalValidator = updateApprovalValidator;
        _updateModulesValidator = updateModulesValidator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id, HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrganization()
    {
        var result = await _userService.GetByOrganizationAsync(HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [Authorize(Policy = "RequireBeheerModule")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request) =>
        ValidateAndExecuteAsync(request, _updateRoleValidator, async () =>
        {
            var result = await _userService.UpdateRoleAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [Authorize(Policy = "RequireBeheerModule")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request) =>
        ValidateAndExecuteAsync(request, _updateStatusValidator, async () =>
        {
            var result = await _userService.UpdateStatusAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPut("{id:guid}/approval")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [Authorize(Policy = "RequireBeheerModule")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateApproval(Guid id, [FromBody] UpdateUserApprovalRequest request) =>
        ValidateAndExecuteAsync(request, _updateApprovalValidator, async () =>
        {
            var result = await _userService.UpdateApprovalAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpPut("{id:guid}/modules")]
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateModules(Guid id, [FromBody] UpdateModulesRequest request) =>
        ValidateAndExecuteAsync(request, _updateModulesValidator, async () =>
        {
            var result = await _userService.UpdateModulesAsync(id, request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });
}
