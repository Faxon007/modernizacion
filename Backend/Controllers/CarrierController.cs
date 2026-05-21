using System;
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
    [Route("api/carriers")]
    public class CarrierController : ControllerBase
    {
        private readonly ICarrierRepository _carrierRepo;
        private readonly ILogger<CarrierController> _logger;

        public CarrierController(ICarrierRepository carrierRepo, ILogger<CarrierController> logger)
        {
            _carrierRepo = carrierRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCarriers()
        {
            try
            {
                var carriers = await _carrierRepo.GetTransportadorasAsync();
                return Ok(new { success = true, data = carriers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transportadoras.");
                return StatusCode(500, new { success = false, message = "Error al obtener transportadoras." });
            }
        }

        [HttpGet("{usuario}")]
        public async Task<IActionResult> GetCarrier(string usuario)
        {
            try
            {
                var carrier = await _carrierRepo.GetTransportadoraAsync(usuario);
                if (carrier == null)
                {
                    return NotFound(new { success = false, message = "Transportadora no encontrada." });
                }
                return Ok(new { success = true, data = carrier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transportadora para {Usuario}.", usuario);
                return StatusCode(500, new { success = false, message = "Error al obtener transportadora." });
            }
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetCarriersDropdown([FromQuery] string codCliAci = "")
        {
            try
            {
                var list = await _carrierRepo.GetTransportadorasDLLAsync(codCliAci);
                return Ok(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dropdown de transportadoras.");
                return StatusCode(500, new { success = false, message = "Error al obtener dropdown." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCarrier([FromBody] CarrierModel carrier)
        {
            if (carrier == null) return BadRequest(new { success = false, message = "Datos inválidos." });

            try
            {
                var result = await _carrierRepo.InsertTransportadoraAsync(carrier);
                if (result)
                {
                    return Ok(new { success = true, message = "Transportadora creada exitosamente." });
                }
                return BadRequest(new { success = false, message = "No se pudo crear la transportadora." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar transportadora.");
                return StatusCode(500, new { success = false, message = "Error al guardar transportadora." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCarrier([FromBody] CarrierModel carrier)
        {
            if (carrier == null) return BadRequest(new { success = false, message = "Datos inválidos." });

            try
            {
                var result = await _carrierRepo.UpdateTransportadoraAsync(carrier);
                if (result)
                {
                    return Ok(new { success = true, message = "Transportadora actualizada exitosamente." });
                }
                return BadRequest(new { success = false, message = "No se pudo actualizar la transportadora." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar transportadora.");
                return StatusCode(500, new { success = false, message = "Error al actualizar transportadora." });
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateCarrierUser([FromBody] CarrierModel carrier)
        {
            if (carrier == null || string.IsNullOrEmpty(carrier.CodTranspo) || string.IsNullOrEmpty(carrier.Clave))
            {
                return BadRequest(new { success = false, message = "El código y la clave de la transportadora son requeridos." });
            }

            try
            {
                string creator = User.Identity?.Name ?? "SISTEMA";
                var result = await _carrierRepo.InsertUsuarioAsync(carrier, creator);
                if (result)
                {
                    return Ok(new { success = true, message = "Usuario de transportadora configurado exitosamente." });
                }
                return BadRequest(new { success = false, message = "No se pudo crear el usuario." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario de transportadora.");
                return StatusCode(500, new { success = false, message = "Error al registrar credenciales de transportadora." });
            }
        }

        [HttpPut("user")]
        public async Task<IActionResult> UpdateCarrierUser([FromBody] CarrierModel carrier)
        {
            if (carrier == null || string.IsNullOrEmpty(carrier.CodTranspo))
            {
                return BadRequest(new { success = false, message = "El código de la transportadora es requerido." });
            }

            try
            {
                string modifier = User.Identity?.Name ?? "SISTEMA";
                var result = await _carrierRepo.UpdateUsuarioAsync(carrier, modifier);
                if (result)
                {
                    return Ok(new { success = true, message = "Usuario de transportadora actualizado exitosamente." });
                }
                return BadRequest(new { success = false, message = "No se pudo actualizar el usuario." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario de transportadora.");
                return StatusCode(500, new { success = false, message = "Error al actualizar credenciales de transportadora." });
            }
        }
    }
}
