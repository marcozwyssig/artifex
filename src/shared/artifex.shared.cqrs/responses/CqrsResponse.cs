namespace Artifex.Shared.Ui.Responses;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class CqrsResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public string? Message { get; init; }

    public static CqrsResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new CqrsResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static CqrsResponse<T> FailureResponse(IReadOnlyCollection<string> errors, string? message = null)
    {
        return new CqrsResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = message ?? "An error occurred"
        };
    }

    public static CqrsResponse<T> FailureResponse(string error, string? message = null)
    {
        return FailureResponse(new[] { error }, message);
    }
}

/// <summary>
/// Standard API response without data
/// </summary>
public class CqrsResponse
{
    public bool Success { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public string? Message { get; init; }

    public static CqrsResponse SuccessResponse(string? message = null)
    {
        return new CqrsResponse
        {
            Success = true,
            Message = message
        };
    }

    public static CqrsResponse FailureResponse(IReadOnlyCollection<string> errors, string? message = null)
    {
        return new CqrsResponse
        {
            Success = false,
            Errors = errors,
            Message = message ?? "An error occurred"
        };
    }

    public static CqrsResponse FailureResponse(string error, string? message = null)
    {
        return FailureResponse(new[] { error }, message);
    }
}
