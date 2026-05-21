using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/parameters")]
    public class ParametersController : ControllerBase
    {
        private readonly ISiteRepository _siteRepo;
        private readonly ILogger<ParametersController> _logger;

        public ParametersController(ISiteRepository siteRepo, ILogger<ParametersController> logger)
        {
            _siteRepo = siteRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetParameters()
        {
            try
            {
                var parameters = await _siteRepo.GetParametrosAsync();
                if (parameters == null)
                {
                    return NotFound(new { success = false, message = "No se encontraron parámetros del sistema." });
                }
                return Ok(new { success = true, data = parameters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener parámetros del sistema.");
                return StatusCode(500, new { success = false, message = "Error interno al obtener parámetros." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateParameters([FromBody] SystemParameters parameters)
        {
            if (parameters == null)
            {
                return BadRequest(new { success = false, message = "Datos de parámetros inválidos." });
            }

            try
            {
                string username = User.Identity?.Name ?? "SISTEMA";
                var result = await _siteRepo.UpdateParametrosAsync(parameters, username);
                if (result)
                {
                    // Registra bitácora de modificación de parámetros
                    try
                    {
                        var bitacora = new BitacoraRequest
                        {
                            CodLink = "",
                            CodParametro = "",
                            Descripcion = "Se modificaron parametros del sistema.",
                            TipProcesamiento = "S"
                        };
                        await _siteRepo.RegistraBitacoraAsync(bitacora);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo registrar la bitácora de actualización de parámetros.");
                    }

                    return Ok(new { success = true, message = "Parámetros actualizados con éxito." });
                }
                return BadRequest(new { success = false, message = "No se pudieron actualizar los parámetros." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parámetros del sistema.");
                return StatusCode(500, new { success = false, message = "Error interno al actualizar parámetros." });
            }
        }
    }
}
