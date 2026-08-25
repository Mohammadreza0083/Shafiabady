using System.Text.Json.Serialization;

namespace ShafieiAbadi.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string? ErrorMessage { get; }
    public List<string> ValidationErrors { get; }

    [JsonConstructor]
    public Result(bool isSuccess, int statusCode, string? errorMessage = null, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public static Result Success(int statusCode = 200) => new(true, statusCode);

    public static Result Failure(string errorMessage, int statusCode = 400) =>
        new(false, statusCode, errorMessage);

    public static Result ValidationFailure(List<string> errors) =>
        new(false, 422, "یک یا چند خطای اعتبارسنجی رخ داده است.", errors);

    public static Result NotFound(string message = "مورد درخواستی یافت نشد.") =>
        new(false, 404, message);

    public static Result Unauthorized(string message = "دسترسی غیرمجاز است.") =>
        new(false, 401, message);
}
