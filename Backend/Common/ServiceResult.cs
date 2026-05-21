namespace Backend.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; private init; }
        public bool Failed => !Success;
        public T? Data { get; private init; }
        public string? ErrorCode { get; private init; }
        public string? ErrorMessage { get; private init; }
        public int HttpStatus { get; private init; }

        private ServiceResult() { }

        public static ServiceResult<T> Ok(T data) => new()
        {
            Success = true,
            Data = data,
            HttpStatus = 200
        };

        public static ServiceResult<T> Fail(string code, string message, int status = 400) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message,
            HttpStatus = status
        };
    }

    public class ServiceResult
    {
        public bool Success { get; private init; }
        public bool Failed => !Success;
        public string? ErrorCode { get; private init; }
        public string? ErrorMessage { get; private init; }
        public int HttpStatus { get; private init; }

        private ServiceResult() { }

        public static ServiceResult Ok() => new()
        {
            Success = true,
            HttpStatus = 204
        };

        public static ServiceResult Fail(string code, string message, int status = 400) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message,
            HttpStatus = status
        };
    }
}
