using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/links")]
    public class LinkController : ControllerBase
    {
        private readonly ILinkBusinessService _linkBusinessService;
        private readonly ILinkRepository _linkRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IUrlShortenerService _shortenerService;
        private readonly ILogger<LinkController> _logger;

        public LinkController(
            ILinkBusinessService linkBusinessService,
            ILinkRepository linkRepository,
            ISiteRepository siteRepository,
            IUrlShortenerService shortenerService,
            ILogger<LinkController> logger)
        {
            _linkBusinessService = linkBusinessService;
            _linkRepository = linkRepository;
            _siteRepository = siteRepository;
            _shortenerService = shortenerService;
            _logger = logger;
        }

        [HttpPost("get-links")]
        public async Task<IActionResult> GetLinks([FromBody] DataTableRequest request)
        {
            try
            {
                string username = User.Identity?.Name ?? "SISTEMA";
                string search = request.Search?.Value?.Replace(' ', '%') ?? string.Empty;
                
                string orderCol = "FEC_EMISION";
                string orderDir = "desc";

                if (request.Order != null && request.Order.Count > 0)
                {
                    var order = request.Order[0];
                    if (request.Columns != null && order.Column < request.Columns.Count)
                    {
                        var col = request.Columns[order.Column];
                        if (!string.IsNullOrEmpty(col.Name))
                        {
                            orderCol = col.Name;
                        }
                        orderDir = order.Dir ?? "asc";
                    }
                }

                var (items, totalCount, filteredCount) = await _linkRepository.GetLinksPagedAsync(
                    request.Start,
                    request.Length,
                    orderCol,
                    orderDir,
                    search,
                    username
                );

                return Ok(new
                {
                    draw = request.Draw,
                    recordsTotal = totalCount,
                    recordsFiltered = filteredCount,
                    data = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de links paginados.");
                return StatusCode(500, new { success = false, message = "Error al consultar los links." });
            }
        }

        [HttpPost("get-links-verifica")]
        public async Task<IActionResult> GetLinksVerifica([FromBody] DataTableRequest request)
        {
            try
            {
                string username = User.Identity?.Name ?? "SISTEMA";
                string search = request.Search?.Value?.Replace(' ', '%') ?? string.Empty;

                string orderCol = "FEC_EMISION";
                string orderDir = "desc";

                if (request.Order != null && request.Order.Count > 0)
                {
                    var order = request.Order[0];
                    if (request.Columns != null && order.Column < request.Columns.Count)
                    {
                        var col = request.Columns[order.Column];
                        if (!string.IsNullOrEmpty(col.Name))
                        {
                            orderCol = col.Name;
                        }
                        orderDir = order.Dir ?? "asc";
                    }
                }

                var (items, totalCount, filteredCount) = await _linkRepository.GetLinksVerificaPagedAsync(
                    request.Start,
                    request.Length,
                    orderCol,
                    orderDir,
                    search,
                    username
                );

                return Ok(new
                {
                    draw = request.Draw,
                    recordsTotal = totalCount,
                    recordsFiltered = filteredCount,
                    data = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de verificación de links.");
                return StatusCode(500, new { success = false, message = "Error al verificar los links." });
            }
        }

        [HttpPost("acortar")]
        public async Task<IActionResult> AcortarLink([FromBody] AcortarLinkRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { success = false, message = "El URL es requerido." });
            }

            try
            {
                string shortUrl = await _shortenerService.ShortenUrlAsync(request.Url);
                return Ok(new { success = true, data = shortUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al acortar link genérico.");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("periferico")]
        public async Task<IActionResult> AcortarLinkPeriferico([FromBody] PerifericoLinkRequest request)
        {
            if (request == null || request.CodPeriferico <= 0 || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { success = false, message = "El código de periférico y URL son requeridos." });
            }

            try
            {
                string shortUrl = await _linkBusinessService.AcortarLinkPerifericoAsync(request.CodPeriferico, request.Url);
                return Ok(new { success = true, data = shortUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al acortar link periférico: {Periferico}", request.CodPeriferico);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("masivo")]
        public async Task<IActionResult> AcortarLinkMasivo()
        {
            try
            {
                string result = await _linkBusinessService.ProcesarAcortamientoMasivoAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en acortamiento masivo.");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirLink([FromBody] EmitirLinkRequest request)
        {
            if (request == null || request.Link == null)
            {
                return BadRequest(new { success = false, message = "El link es requerido." });
            }

            try
            {
                string username = User.Identity?.Name ?? "SISTEMA";
                string shortUrl = await _linkBusinessService.EmitirLinkAsync(request.Link, request.ImgPublicitaria ?? string.Empty, username);
                return Ok(new { success = true, data = shortUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al emitir link.");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("validar/{sku}")]
        public async Task<IActionResult> ValidarYConsultaLink(string sku)
        {
            try
            {
                var info = await _linkBusinessService.ValidarYConsultaLinkAsync(sku);
                if (info == null)
                {
                    return NotFound(new { success = false, message = "Link no encontrado en Visa." });
                }
                return Ok(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar link en Visa: {Sku}", sku);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarLink([FromBody] CancelarLinkRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Sku))
            {
                return BadRequest(new { success = false, message = "El SKU es requerido." });
            }

            try
            {
                string username = User.Identity?.Name ?? "SISTEMA";
                bool result = await _linkBusinessService.CancelarLinkAsync(request.Sku, request.Nombre, request.Precio, username);
                if (result)
                {
                    return Ok(new { success = true, message = "Link cancelado exitosamente." });
                }
                return BadRequest(new { success = false, message = "No se pudo cancelar el link en Visa." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar link: {Sku}", request.Sku);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("buscar-cta/{numCta}")]
        public async Task<IActionResult> BuscarCta(string numCta)
        {
            if (string.IsNullOrEmpty(numCta))
            {
                return BadRequest(new { success = false, message = "El número de cuenta es requerido." });
            }

            try
            {
                var info = await _linkRepository.GetLinkCtaAsync(numCta);
                if (info == null)
                {
                    return NotFound(new { success = false, message = "No se encontró información de link programado para la cuenta." });
                }
                return Ok(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar link por cuenta: {NumCta}", numCta);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("buscar-parametro/{codParametro}")]
        public async Task<IActionResult> BuscarParametro(string codParametro)
        {
            if (string.IsNullOrEmpty(codParametro))
            {
                return BadRequest(new { success = false, message = "El código de parámetro es requerido." });
            }

            try
            {
                var info = await _linkRepository.GetLinkParametroAsync(codParametro);
                if (info == null)
                {
                    return NotFound(new { success = false, message = "No se encontró información de link programado para el código." });
                }
                return Ok(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar link por parámetro: {CodParametro}", codParametro);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("update-estado")]
        public async Task<IActionResult> UpdateEstadoLink([FromBody] UpdateEstadoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CodParametro))
            {
                return BadRequest(new { success = false, message = "El parámetro es requerido." });
            }

            try
            {
                // Buscar los detalles antes de actualizar para armar la descripción de la bitácora
                var info = await _linkRepository.GetLinkParametroAsync(request.CodParametro);

                bool result = await _linkRepository.UpdateEstadoLinkAsync(request.CodParametro);
                if (result)
                {
                    try
                    {
                        string diaMes = info?.DiaMes ?? "N/A";
                        var bitacora = new BitacoraRequest
                        {
                            CodLink = "",
                            CodParametro = request.CodParametro,
                            Descripcion = $"Se da de baja al parametro   ({request.CodParametro})  que se encontraba configurado para generarse automaticamente el {diaMes} de cada mes.",
                            TipProcesamiento = "B"
                        };
                        await _siteRepository.RegistraBitacoraAsync(bitacora);
                    }
                    catch (Exception bitEx)
                    {
                        _logger.LogWarning(bitEx, "No se pudo registrar en la bitácora local de baja de parámetro.");
                    }
                }
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del link localmente: {CodParametro}", request.CodParametro);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("aplicar-pago")]
        public async Task<IActionResult> AplicarPago([FromBody] PagoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CodLink) || string.IsNullOrEmpty(request.NumCta) || string.IsNullOrEmpty(request.CodSku) || string.IsNullOrEmpty(request.AutVisa))
            {
                return BadRequest(new { success = false, message = "Los campos CodLink, NumCta, CodSku y AutVisa son requeridos." });
            }

            try
            {
                // 1. Obtener detalles del link desde la base de datos
                var linkInfo = await _linkRepository.GetParametroAsync(request.CodLink);
                if (linkInfo == null)
                {
                    return BadRequest(new { success = false, message = "No se encontraron los detalles del link en el sistema." });
                }

                // Asignar monto de cobro
                request.MonPago = linkInfo.MonCobro;

                // 2. Determinar tipo de cuenta (PR / TC) y aplicar pago con la moneda correcta
                // TipPago = "0" -> Quetzales (320), TipPago = "1" -> Dólares (840)
                string moneda = linkInfo.TipPago == "0" ? "320" : "840";
                bool result = false;

                if (linkInfo.TipCuenta == "PR")
                {
                    result = await _linkRepository.AplicaPagoPRAsync(request, moneda);
                }
                else if (linkInfo.TipCuenta == "TC")
                {
                    result = await _linkRepository.AplicaPagoTCAsync(request, moneda);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Tipo de cuenta no soportado para aplicar pagos." });
                }

                if (result)
                {
                    // 3. Registrar bitácora
                    try
                    {
                        var bitacora = new BitacoraRequest
                        {
                            CodLink = request.CodLink,
                            CodParametro = "",
                            Descripcion = $"MANUAL: Se procedio con el pago  ({request.CodSku}) según link No.{request.CodLink} asociado al número de cuenta de No.{request.NumCta}, Valor = {request.MonPago}",
                            TipProcesamiento = "P"
                        };
                        await _siteRepository.RegistraBitacoraAsync(bitacora);
                    }
                    catch (Exception bitEx)
                    {
                        _logger.LogWarning(bitEx, "No se pudo registrar en la bitácora de aplicación de pago manual.");
                    }

                    return Ok(new { success = true, message = "Se efectuó de forma exitosa el pago." });
                }

                return BadRequest(new { success = false, message = "Inconveniente al efectuar el pago en el core bancario." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar pago manual para link: {CodLink}", request.CodLink);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    #region Helper Models for Requests
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<DataTableColumn>? Columns { get; set; }
        public List<DataTableOrder>? Order { get; set; }
        public DataTableSearch? Search { get; set; }
    }

    public class DataTableColumn
    {
        public string? Data { get; set; }
        public string? Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTableSearch? Search { get; set; }
    }

    public class DataTableOrder
    {
        public int Column { get; set; }
        public string? Dir { get; set; }
    }

    public class DataTableSearch
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
    }

    public class AcortarLinkRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    public class PerifericoLinkRequest
    {
        public int CodPeriferico { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class EmitirLinkRequest
    {
        public LinkEntity? Link { get; set; }
        public string? ImgPublicitaria { get; set; }
    }

    public class CancelarLinkRequest
    {
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }

    public class UpdateEstadoRequest
    {
        public string CodParametro { get; set; } = string.Empty;
    }
    #endregion
}
