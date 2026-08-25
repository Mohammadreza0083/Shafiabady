using System.Text.Json.Serialization;

namespace ShafieiAbadi.Application.Common;

/// <summary>
/// Standardized result pattern response wrapper for API endpoints.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the business operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// HTTP status code associated with the result.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Optional human-readable error description when an operation fails.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Collection of specific field validation error messages.
    /// </summary>
    public List<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    [JsonConstructor]
    public Result(bool isSuccess, int statusCode, string? errorMessage = null, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful Result.
    /// </summary>
    /// <param name="statusCode">HTTP status code (defaults to 200 OK).</param>
    public static Result Success(int statusCode = 200) => new(true, statusCode);

    /// <summary>
    /// Creates a failed Result with an error message.
    /// </summary>
    /// <param name="errorMessage">Detailed error message.</param>
    /// <param name="statusCode">HTTP status code (defaults to 400 Bad Request).</param>
    public static Result Failure(string errorMessage, int statusCode = 400) =>
        new(false, statusCode, errorMessage);

    /// <summary>
    /// Creates a validation failure Result with specific error entries.
    /// </summary>
    /// <param name="errors">List of validation errors.</param>
    public static Result ValidationFailure(List<string> errors) =>
        new(false, 422, "One or more validation errors occurred.", errors);

    /// <summary>
    /// Creates a 404 Not Found Result.
    /// </summary>
    /// <param name="message">Optional not found explanation.</param>
    public static Result NotFound(string message = "The requested resource was not found.") =>
        new(false, 404, message);

    /// <summary>
    /// Creates a 401 Unauthorized Result.
    /// </summary>
    /// <param name="message">Optional unauthorized explanation.</param>
    public static Result Unauthorized(string message = "Access is unauthorized.") =>
        new(false, 401, message);
}
