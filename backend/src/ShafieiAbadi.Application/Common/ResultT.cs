namespace ShafieiAbadi.Application.Common;

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, T? data, int statusCode, string? errorMessage = null, List<string>? validationErrors = null)
        : base(isSuccess, statusCode, errorMessage, validationErrors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, int statusCode = 200) =>
        new(true, data, statusCode);

    public static new Result<T> Failure(string errorMessage, int statusCode = 400) =>
        new(false, default, statusCode, errorMessage);

    public static new Result<T> NotFound(string message = "مورد درخواستی یافت نشد.") =>
        new(false, default, 404, message);

    public static new Result<T> ValidationFailure(List<string> errors) =>
        new(false, default, 422, "یک یا چند خطای اعتبارسنجی رخ داده است.", errors);

    public static new Result<T> Unauthorized(string message = "دسترسی غیرمجاز است.") =>
        new(false, default, 401, message);
}
