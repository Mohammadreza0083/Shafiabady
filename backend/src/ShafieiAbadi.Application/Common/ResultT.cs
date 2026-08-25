using System.Text.Json.Serialization;

namespace ShafieiAbadi.Application.Common;

/// <summary>
/// Generic standardized result pattern wrapper carrying a typed data payload.
/// </summary>
/// <typeparam name="T">Payload data type.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Typed response data payload (null on failure).
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Initializes a new instance of the generic Result class.
    /// </summary>
    [JsonConstructor]
    public Result(bool isSuccess, T? data, int statusCode, string? errorMessage = null, List<string>? validationErrors = null)
        : base(isSuccess, statusCode, errorMessage, validationErrors)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a successful generic Result with data payload.
    /// </summary>
    /// <param name="data">Response data payload.</param>
    /// <param name="statusCode">HTTP status code (defaults to 200 OK).</param>
    public static Result<T> Success(T data, int statusCode = 200) =>
        new(true, data, statusCode);

    /// <summary>
    /// Creates a failed generic Result.
    /// </summary>
    /// <param name="errorMessage">Error description.</param>
    /// <param name="statusCode">HTTP status code (defaults to 400 Bad Request).</param>
    public static new Result<T> Failure(string errorMessage, int statusCode = 400) =>
        new(false, default, statusCode, errorMessage);

    /// <summary>
    /// Creates a 404 Not Found generic Result.
    /// </summary>
    /// <param name="message">Not found explanation.</param>
    public static new Result<T> NotFound(string message = "The requested resource was not found.") =>
        new(false, default, 404, message);

    /// <summary>
    /// Creates a 422 Validation Failure generic Result.
    /// </summary>
    /// <param name="errors">List of validation error entries.</param>
    public static new Result<T> ValidationFailure(List<string> errors) =>
        new(false, default, 422, "One or more validation errors occurred.", errors);

    /// <summary>
    /// Creates a 401 Unauthorized generic Result.
    /// </summary>
    /// <param name="message">Unauthorized explanation.</param>
    public static new Result<T> Unauthorized(string message = "Access is unauthorized.") =>
        new(false, default, 401, message);
}
