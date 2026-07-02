using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.DataProtection;
using Backend.Models;

namespace Backend.Infrastructure.Security
{
    public interface IJwtService
    {
        LoginResponse GenerateToken(string username, string role, string password);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IDataProtector _protector;

        public JwtService(IConfiguration config, IDataProtectionProvider dataProtectionProvider)
        {
            _config = config;
            _protector = dataProtectionProvider.CreateProtector("Backend.Database.UserCredentials");
        }

        public LoginResponse GenerateToken(string username, string role, string password)
        {
            var secretKey = _config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey no configurado. Agregue la clave en user-secrets o variable de entorno.");

            var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Corregido: Leer la configuración en horas para que coincida con appsettings.json
            var expHours = double.Parse(_config["Jwt:ExpirationHours"] ?? "2"); // Fallback a 2 horas si no está configurado
            var expiry = DateTime.UtcNow.AddHours(expHours);

            // Encriptamos la clave del usuario para que viaje en el JWT pero no sea legible
            var encryptedPwd = _protector.Protect(password);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,          username),
                new Claim(JwtRegisteredClaimNames.Sub,        username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Name,                    username),
                new Claim(ClaimTypes.Role,                    role),
                new Claim("db_pwd",                           encryptedPwd),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            expiry,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponse(tokenString, expiry, username, role);
        }
    }
}
