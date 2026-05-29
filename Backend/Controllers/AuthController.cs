using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;
using Backend.Repositories;
using Backend.Infrastructure.Database;
using Backend.Infrastructure.Security;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IDatabaseConnectionProvider _dbProvider;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(
            IDatabaseConnectionProvider dbProvider,
            IJwtService jwtService,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _dbProvider = dbProvider;
            _jwtService = jwtService;
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Usuario y contraseña son requeridos." });
            }

            // 1. Obtener la cadena de conexión base de Oracle (DatabaseKey.TC)
            var baseConnStr = _dbProvider.GetConnectionString(DatabaseKey.TC);
            if (string.IsNullOrEmpty(baseConnStr))
            {
                _logger.LogError("La conexión base de Oracle (TC) no está disponible en db.cef2.");
                return StatusCode(500, new { success = false, message = "Error interno de base de datos." });
            }

            // 2. Construir la cadena de conexión específica para el usuario
            string userConnStr;
            try
            {
                var builder = new OracleConnectionStringBuilder(baseConnStr)
                {
                    UserID = request.Username.Trim().ToUpper(),
                    Password = request.Password
                };
                userConnStr = builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falla al construir cadena de conexión de usuario.");
                return BadRequest(new { success = false, message = "Credenciales o datos inválidos." });
            }

            // 3. Probar la conexión a Oracle
            using var conn = new OracleConnection(userConnStr);
            try
            {
                await conn.OpenAsync();
            }
            catch (OracleException ex)
            {
                _logger.LogWarning(ex, "Intento de login fallido para usuario: {Username}, ORA-{ErrorNum}", request.Username, ex.Number);
                
                string errorMsg = ex.Number switch
                {
                    1017 => "Usuario o Clave incorrecta.",
                    28000 => "Usuario Bloqueado, ha intentado demasiadas veces una clave incorrecta.",
                    28001 => "Su clave ha expirado, debe cambiarla para poder usar el sistema.",
                    _ => $"Error de base de datos ({ex.Number}): {ex.Message}"
                };

                return Unauthorized(new { success = false, message = errorMsg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error desconocido en inicio de sesión.");
                return StatusCode(500, new { success = false, message = "Ocurrió un error inesperado al intentar iniciar sesión." });
            }

            // 4. Conexión exitosa, validar usuario en RRHH y PA usando la conexión del usuario
            try
            {
                var menuRepo = new MenuRepository(userConnStr);
                string username = request.Username.Trim().ToUpper().Replace("PROMERICA\\", "");

                // Validar RRHH
                _logger.LogInformation("Validando estado en RRHH para el usuario: '{Username}'", username);
                var rrhhActivo = await menuRepo.ValidateRRHHAsync(username);
                _logger.LogInformation("Respuesta de RRHH_USUARIO.activo para '{Username}': '{Activo}'", username, rrhhActivo ?? "NULL");

                if (string.IsNullOrEmpty(rrhhActivo) || rrhhActivo != "S")
                {
                    _logger.LogWarning("Autenticación denegada: Usuario '{Username}' no está activo en RRHH. Valor actual: '{Activo}'", username, rrhhActivo ?? "NULL");
                    return Unauthorized(new { success = false, message = "El usuario no se encuentra activo en RRHH." });
                }

                // Validar PA
                var paActivo = await menuRepo.ValidatePAAsync(username);
                if (string.IsNullOrEmpty(paActivo) || paActivo != "S")
                {
                    return Unauthorized(new { success = false, message = "El usuario no se encuentra activo en PA." });
                }

                // Obtener roles y menú (NoSistema = 509)
                string systemCode = "509";
                var roles = (await menuRepo.VerificarRolAsync(username, systemCode)).ToList();
                var menuItems = (await menuRepo.GetMenuItemsAsync(username, systemCode)).ToList();

                string userRole = roles.FirstOrDefault()?.Rol.ToString() ?? "USUARIO";

                // 5. Generar token JWT
                var jwtResponse = _jwtService.GenerateToken(username, userRole);

                _logger.LogInformation("Usuario {Username} autenticado correctamente con el rol {Role}", username, userRole);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        accessToken = jwtResponse.AccessToken,
                        expiresAt = jwtResponse.ExpiresAt,
                        username = jwtResponse.Username,
                        role = jwtResponse.Role,
                        menu = menuItems
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar roles y menú del usuario: {Username}", request.Username);
                return StatusCode(500, new { success = false, message = "Error al recuperar privilegios de usuario.", detail = ex.Message });
            }
        }
    }
}
