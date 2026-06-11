using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Database
{
    public interface IUserConnectionProvider
    {
        DatabaseKey DefaultKey { get; }
        /// <summary>
        /// Obtiene la cadena de conexión específica para el usuario autenticado.
        /// Si no hay un usuario autenticado, retorna la cadena de conexión genérica.
        /// </summary>
        string GetUserConnectionString(DatabaseKey key);
    }

    public class UserConnectionProvider : IUserConnectionProvider
    {
        private readonly IDatabaseConnectionProvider _baseProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;
        private readonly ILogger<UserConnectionProvider> _logger;

        public DatabaseKey DefaultKey => _baseProvider.DefaultKey;

        public UserConnectionProvider(
            IDatabaseConnectionProvider baseProvider,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserConnectionProvider> logger)
        {
            _baseProvider = baseProvider;
            _httpContextAccessor = httpContextAccessor;
            _protector = dataProtectionProvider.CreateProtector("Backend.Database.UserCredentials");
            _logger = logger;
        }

        public string GetUserConnectionString(DatabaseKey key)
        {
            var baseConnStr = _baseProvider.GetConnectionString(key);
            if (string.IsNullOrEmpty(baseConnStr))
            {
                throw new InvalidOperationException($"La conexión base para {key} no está disponible.");
            }

            var user = _httpContextAccessor.HttpContext?.User;

            // Si no hay contexto web (ej. tareas en background) o no está autenticado, 
            // usamos la conexión base del db.cef2 por defecto.
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogDebug("No hay usuario autenticado, se utilizará la conexión base genérica para {Key}.", key);
                return baseConnStr;
            }

            // Obtener claims específicos
            var username = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? user.FindFirst(ClaimTypes.Name)?.Value;
            var encryptedPwd = user.FindFirst("db_pwd")?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(encryptedPwd))
            {
                _logger.LogWarning("El token JWT no contiene las credenciales de BD para el usuario. Se usará la conexión genérica.");
                return baseConnStr;
            }

            try
            {
                // Desencriptar el password
                var plainPassword = _protector.Unprotect(encryptedPwd);

                // Reconstruir el string de conexión con los datos del usuario
                var builder = new OracleConnectionStringBuilder(baseConnStr)
                {
                    UserID = username,
                    Password = plainPassword
                };

                _logger.LogDebug("Cadena de conexión inyectada con éxito para el usuario {Username}.", username);
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desencriptar las credenciales del usuario o construir el string de conexión. Se usará la genérica.");
                return baseConnStr;
            }
        }
    }
}