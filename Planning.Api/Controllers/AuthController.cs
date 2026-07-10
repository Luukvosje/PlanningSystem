using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planning.Api.Extensions;
using Planning.Api.Models;
using Planning.Application.Auth;
using Planning.Application.Common;

namespace Planning.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<UpdateProfileRequest> updateProfileValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _updateProfileValidator = updateProfileValidator;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetInfo() => Ok("Api ok");

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Register([FromBody] RegisterRequest request) =>
        ValidateAndExecuteAsync(request, _registerValidator, async () =>
        {
            var result = await _authService.RegisterAsync(request, HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Created(string.Empty, result.Value);
            }

            return result.ToActionResult(this);
        });

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Login([FromBody] LoginRequest request) =>
        ValidateAndExecuteAsync(request, _loginValidator, async () =>
        {
            var result = await _authService.LoginAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync(HttpContext.RequestAborted);
        return result.ToActionResult(this);
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request) =>
        ValidateAndExecuteAsync(request, _updateProfileValidator, async () =>
        {
            var result = await _authService.UpdateProfileAsync(request, HttpContext.RequestAborted);
            return result.ToActionResult(this);
        });
}
