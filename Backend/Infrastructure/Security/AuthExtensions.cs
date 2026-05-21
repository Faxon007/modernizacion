using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Backend.Infrastructure.Security;

namespace Backend.Infrastructure.Security
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration          configuration)
        {
            var secretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey no configurado.");

            services.AddScoped<IJwtService, JwtService>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuer          = true,
                        ValidIssuer             = configuration["Jwt:Issuer"],
                        ValidateAudience        = true,
                        ValidAudience           = configuration["Jwt:Audience"],
                        ValidateLifetime        = true,
                        ClockSkew               = TimeSpan.Zero
                    };

                    opts.Events = new JwtBearerEvents
                    {
                        OnChallenge = ctx =>
                        {
                            ctx.HandleResponse();
                            ctx.Response.StatusCode  = 401;
                            ctx.Response.ContentType = "application/json";
                            return ctx.Response.WriteAsJsonAsync(new
                            {
                                success      = false,
                                data         = (object?)null,
                                errorCode    = "AUTH.TOKEN_REQUERIDO",
                                errorMessage = "Se requiere autenticación. Use POST /api/auth/token para obtener el token."
                            });
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
