namespace Backend.Common
{
    public record AppError(int Code, string Message, int HttpStatus);
}
