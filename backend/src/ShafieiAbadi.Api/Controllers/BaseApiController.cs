using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null)
            return NotFound(Result<T>.NotFound());

        return StatusCode(result.StatusCode, result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result == null)
            return NotFound(Result.NotFound());

        return StatusCode(result.StatusCode, result);
    }
}
