using Microsoft.AspNetCore.Mvc;
using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;

namespace PlanningSystem.API.Controllers;

[ApiController]
[Route("user")]
public class UserController : BaseController
{
    private readonly IUserApplicationService _userService;
    private readonly IConfiguration _configuration;

    public UserController(IUserApplicationService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResultObject<User>), 200)]
    [ProducesResponseType(400)]
    public IActionResult CreateUser([FromBody] UserAddRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var res = _userService.CreateUser(request);
        return HandleResult(res);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ResultObject<UserAuthResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public IActionResult AuthenticateUser([FromBody] UserAuthRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            return StatusCode(500, "JWT configuration is missing");

        var res = _userService.AuthenticateUser(request, jwtKey);
        return HandleResult(res);
    }
}
