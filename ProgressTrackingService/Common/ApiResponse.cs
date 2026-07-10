namespace ProgressTrackingService.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(IEnumerable<string> errors, string message = "Operation failed.")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}