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
    [Route("api/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepo;
        private readonly IProductRepository _productRepo;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientRepository clientRepo, IProductRepository productRepo, ILogger<ClientController> logger)
        {
            _clientRepo = clientRepo;
            _productRepo = productRepo;
            _logger = logger;
        }

        [HttpGet("{numCta}")]
        public async Task<IActionResult> GetClienteCta(string numCta)
        {
            try
            {
                var client = await _clientRepo.GetClienteCtaAsync(numCta);
                if (client == null)
                {
                    return NotFound(new { success = false, message = "Cliente no encontrado para la cuenta especificada." });
                }
                return Ok(new { success = true, data = client });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por cuenta: {NumCta}", numCta);
                return StatusCode(500, new { success = false, message = "Error interno al buscar cliente." });
            }
        }

        [HttpGet("{numCta}/prestamo")]
        public async Task<IActionResult> GetTipoPrestamo(string numCta)
        {
            try
            {
                var prestamo = await _clientRepo.GetTipoPrestamoAsync(numCta);
                if (prestamo == null)
                {
                    return NotFound(new { success = false, message = "Información de préstamo no encontrada." });
                }
                return Ok(new { success = true, data = prestamo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de préstamo para la cuenta: {NumCta}", numCta);
                return StatusCode(500, new { success = false, message = "Error interno al obtener préstamo." });
            }
        }

        [HttpGet("blacklist/{codEmpresa}/{codCliente}")]
        public async Task<IActionResult> IsClienteListaNegra(string codEmpresa, string codCliente)
        {
            try
            {
                var isBlacklisted = await _clientRepo.IsClienteListaNegraAsync(codEmpresa, codCliente);
                return Ok(new { success = true, data = isBlacklisted });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar lista negra para cliente: {CodCliente}", codCliente);
                return StatusCode(500, new { success = false, message = "Error interno al verificar lista negra." });
            }
        }

        [HttpGet("{codCliente}/correo")]
        public async Task<IActionResult> GetCorreoCliente(string codCliente)
        {
            try
            {
                var email = await _clientRepo.GetCorreoClienteAsync(codCliente);
                return Ok(new { success = true, data = email ?? "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener correo para cliente: {CodCliente}", codCliente);
                return StatusCode(500, new { success = false, message = "Error interno al obtener correo." });
            }
        }

        [HttpGet("{codCliente}/telefono")]
        public async Task<IActionResult> GetTelefonoCliente(string codCliente)
        {
            try
            {
                var phone = await _clientRepo.GetTelefonoClienteAsync(codCliente);
                return Ok(new { success = true, data = phone ?? "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener teléfono para cliente: {CodCliente}", codCliente);
                return StatusCode(500, new { success = false, message = "Error interno al obtener teléfono." });
            }
        }

        [HttpGet("{codCliente}/cuentas")]
        public async Task<IActionResult> GetCuentas(string codCliente)
        {
            try
            {
                var accounts = await _clientRepo.GetCuentasAsync(codCliente);
                return Ok(new { success = true, data = accounts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas para cliente: {CodCliente}", codCliente);
                return StatusCode(500, new { success = false, message = "Error interno al obtener cuentas." });
            }
        }

        [HttpGet("monto-pr/{numCuenta}")]
        public async Task<IActionResult> GetMontoPR(string numCuenta)
        {
            try
            {
                var valor = await _productRepo.GetMontoPRAsync(numCuenta);
                return Ok(new { success = true, data = valor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener monto máximo préstamo para cuenta: {NumCuenta}", numCuenta);
                return StatusCode(500, new { success = false, message = "Error interno al obtener monto." });
            }
        }

        [HttpGet("monto-tc/{numCuenta}")]
        public async Task<IActionResult> GetMontoTC(string numCuenta)
        {
            try
            {
                var valor = await _productRepo.GetMontoTCAsync(numCuenta);
                return Ok(new { success = true, data = valor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener monto máximo tarjeta para cuenta: {NumCuenta}", numCuenta);
                return StatusCode(500, new { success = false, message = "Error interno al obtener monto." });
            }
        }
    }
}
