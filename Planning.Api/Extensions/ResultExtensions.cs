using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Planning.Application.Common;

namespace Planning.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return MapFailure(controller, result.Error, result.ErrorCode);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return MapFailure(controller, result.Error, result.ErrorCode);
    }

    public static IActionResult ToCreatedActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string actionName,
        Func<T, object> routeValues)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return controller.CreatedAtAction(actionName, routeValues(result.Value), result.Value);
        }

        return result.ToActionResult(controller);
    }

    public static IActionResult ValidationProblem(this ControllerBase controller, ValidationResult validationResult) =>
        controller.ValidationProblem(new ValidationProblemDetails(
            validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray())));

    private static IActionResult MapFailure(ControllerBase controller, string? error, string? errorCode)
    {
        return errorCode switch
        {
            "NOT_FOUND" => controller.NotFound(new { error, errorCode }),
            "VALIDATION_ERROR" => controller.BadRequest(new { error, errorCode }),
            "UNAUTHORIZED" => controller.Unauthorized(new { error, errorCode }),
            "CONFLICT" => controller.Conflict(new { error, errorCode }),
            "FORBIDDEN" => controller.StatusCode(StatusCodes.Status403Forbidden, new { error, errorCode }),
            "EXPIRED" => controller.StatusCode(StatusCodes.Status410Gone, new { error, errorCode }),
            "NO_ORGANIZATION" => controller.BadRequest(new { error, errorCode }),
            "MODULE_DISABLED" => controller.StatusCode(StatusCodes.Status403Forbidden, new { error, errorCode }),
            _ => controller.BadRequest(new { error, errorCode })
        };
    }
}

public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> ValidateAndExecuteAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<Task<IActionResult>> executeAsync)
    {
        var validationResult = await validator.ValidateAsync(request, HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult);
        }

        return await executeAsync();
    }
}
