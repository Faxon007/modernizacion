using System;

namespace Backend.Models
{
    public record LoginRequest(string Username, string Password);

    public record LoginResponse(
        string AccessToken,
        DateTime ExpiresAt,
        string Username,
        string Role);
}
