namespace Maestro.Shared.Presentation.Responses;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public string? Message { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> FailureResponse(IReadOnlyCollection<string> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = message ?? "An error occurred"
        };
    }

    public static ApiResponse<T> FailureResponse(string error, string? message = null)
    {
        return FailureResponse(new[] { error }, message);
    }
}

/// <summary>
/// Standard API response without data
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public string? Message { get; init; }

    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse FailureResponse(IReadOnlyCollection<string> errors, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Errors = errors,
            Message = message ?? "An error occurred"
        };
    }

    public static ApiResponse FailureResponse(string error, string? message = null)
    {
        return FailureResponse(new[] { error }, message);
    }
}
