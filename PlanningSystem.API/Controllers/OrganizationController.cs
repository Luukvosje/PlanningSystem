using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Application.Common;

namespace PlanningSystem.API.Controllers;

[ApiController]
[Route("organization")]
[Authorize]
public class OrganizationController : BaseController
{
    private readonly IOrganizationApplicationService _organizationService;

    public OrganizationController(IOrganizationApplicationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultObject<List<Organization>>), 200)]
    public IActionResult GetUserOrganizations()
    {
        var res = _organizationService.GetUserOrganizations(UserId);
        return HandleResult(res);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetOrganization(int id)
    {
        var res = _organizationService.GetOrganization(id, UserId);
        return HandleResult(res);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
    [ProducesResponseType(400)]
    public IActionResult CreateOrganization([FromBody] OrganizationCreateRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _organizationService.CreateOrganization(request, UserId);
        return HandleResult(res);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResultObject<Organization>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult UpdateOrganization(int id, [FromBody] OrganizationUpdateRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _organizationService.UpdateOrganization(id, request, UserId);
        return HandleResult(res);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResultObject<bool>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult DeleteOrganization(int id)
    {
        var res = _organizationService.DeleteOrganization(id, UserId);
        return HandleResult(res);
    }

    [HttpGet("{id}/users")]
    [ProducesResponseType(typeof(ResultObject<List<Domain.Entities.User>>), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetOrganizationUsers(int id)
    {
        var res = _organizationService.GetOrganizationUsers(id, UserId);
        return HandleResult(res);
    }

    [HttpPost("{id}/users")]
    [ProducesResponseType(typeof(ResultObject<bool>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult AddUserToOrganization(int id, [FromBody] OrganizationAddUserRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _organizationService.AddUserToOrganization(id, request, UserId);
        return HandleResult(res);
    }

    [HttpDelete("{id}/users/{userId}")]
    [ProducesResponseType(typeof(ResultObject<bool>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult RemoveUserFromOrganization(int id, int userId)
    {
        var res = _organizationService.RemoveUserFromOrganization(id, userId, UserId);
        return HandleResult(res);
    }
}
