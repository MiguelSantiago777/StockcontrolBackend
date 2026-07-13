using Microsoft.AspNetCore.Mvc;
using StockControl.Domain.Common;

namespace StockControl.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result.Value);
        return result.Error.ToProblem(controller);
    }

    public static ActionResult ToCreatedResult<T>(this Result<T> result, ControllerBase controller,
        string actionName, Func<T, object> routeValues)
    {
        if (result.IsSuccess)
            return controller.CreatedAtAction(actionName, routeValues(result.Value), result.Value);
        return result.Error.ToProblem(controller);
    }

    public static ActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.NoContent();
        return result.Error.ToProblem(controller);
    }

    private static ActionResult ToProblem(this Error error, ControllerBase controller)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}
