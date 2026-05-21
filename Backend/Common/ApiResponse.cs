namespace Backend.Common
{
    public record ApiResponse<T>(
        bool Success,
        T? Data,
        string? ErrorCode,
        string? ErrorMessage)
    {
        public static ApiResponse<T> Ok(T data) =>
            new(true, data, null, null);

        public static ApiResponse<T> Fail(string code, string message) =>
            new(false, default, code, message);
    }
}
