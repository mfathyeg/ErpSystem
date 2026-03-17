using Asp.Versioning;
using ErpSystem.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return HandleError(result.Error);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleError(result.Error);
    }

    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Value);
        }

        return HandleError(result.Error);
    }

    private IActionResult HandleError(Error error)
    {
        return error.Code switch
        {
            "Error.NotFound" => NotFound(new { error.Code, error.Message }),
            "Error.Validation" => BadRequest(new { error.Code, error.Message }),
            "Error.Conflict" => Conflict(new { error.Code, error.Message }),
            "Error.Unauthorized" => Unauthorized(new { error.Code, error.Message }),
            "Error.Forbidden" => Forbid(),
            _ when error.Code.StartsWith("Validation.") => BadRequest(new { error.Code, error.Message }),
            _ => BadRequest(new { error.Code, error.Message })
        };
    }
}
