using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Abstract base API controller providing standardized Result-to-IActionResult conversion helpers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Converts a generic Result object into the appropriate HTTP IActionResult with status code preservation.
    /// </summary>
    /// <typeparam name="T">Payload data type.</typeparam>
    /// <param name="result">Generic result instance.</param>
    /// <returns>HTTP action result.</returns>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null)
            return NotFound(Result<T>.NotFound());

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Converts a non-generic Result object into the appropriate HTTP IActionResult with status code preservation.
    /// </summary>
    /// <param name="result">Result instance.</param>
    /// <returns>HTTP action result.</returns>
    protected IActionResult HandleResult(Result result)
    {
        if (result == null)
            return NotFound(Result.NotFound());

        return StatusCode(result.StatusCode, result);
    }
}
