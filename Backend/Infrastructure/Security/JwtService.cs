using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Backend.Models;

namespace Backend.Infrastructure.Security
{
    public interface IJwtService
    {
        LoginResponse GenerateToken(string username, string role);
    }

    public class JwtService(IConfiguration config) : IJwtService
    {
        public LoginResponse GenerateToken(string username, string role)
        {
            var secretKey = config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey no configurado. Agregue la clave en user-secrets o variable de entorno.");

            var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expHrs = int.Parse(config["Jwt:ExpirationHours"] ?? "8");
            var expiry = DateTime.UtcNow.AddHours(expHrs);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,        username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Name,                    username),
                new Claim(ClaimTypes.Role,                    role),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer:             config["Jwt:Issuer"],
                audience:           config["Jwt:Audience"],
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            expiry,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponse(tokenString, expiry, username, role);
        }
    }
}
