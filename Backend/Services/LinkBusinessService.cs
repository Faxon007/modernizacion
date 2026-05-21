using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class LinkBusinessService : ILinkBusinessService
    {
        private readonly ILinkRepository _linkRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IVisaEnLinkIntegrationService _visaService;
        private readonly IUrlShortenerService _shortenerService;
        private readonly ILogger<LinkBusinessService> _logger;

        public LinkBusinessService(
            ILinkRepository linkRepository,
            ISiteRepository siteRepository,
            IVisaEnLinkIntegrationService visaService,
            IUrlShortenerService shortenerService,
            ILogger<LinkBusinessService> logger)
        {
            _linkRepository = linkRepository;
            _siteRepository = siteRepository;
            _visaService = visaService;
            _shortenerService = shortenerService;
            _logger = logger;
        }

        public async Task<string> EmitirLinkAsync(LinkEntity link, string imgPublicitaria, string username)
        {
            // 1. Obtener correlativo interno
            long correlativo = await _siteRepository.ObtenerCodigoInternoAsync();
            string sku = correlativo.ToString();

            // Asignar datos del link
            link.CodLink = sku;
            link.UsuIngreso = username;

            // 2. Registrar producto/link en Visa
            // Monto formateado para la API de Visa
            string montoStr = link.Monto.ToString("F2");
            var (visaSku, visaUrl) = await _visaService.CrearLinkAsync(link.NomProducto, montoStr, imgPublicitaria, sku);

            link.LongLink = visaUrl;

            // 3. Acortar el URL de Visa obtenido
            string shortUrl = await _shortenerService.ShortenUrlAsync(visaUrl);
            link.ShortLink = shortUrl;

            // 4. Registrar en base de datos local
            string? resultMsg = await _linkRepository.InsertLinkAsync(link);
            if (!string.IsNullOrEmpty(resultMsg))
            {
                throw new Exception($"Error al insertar link localmente: {resultMsg}");
            }

            // 5. Registrar Bitácora de Auditoría
            await _siteRepository.RegistraBitacoraAsync(new BitacoraRequest
            {
                CodLink = sku,
                CodParametro = "EMISION",
                Descripcion = $"Se emitió link de pago {sku} para cliente {link.CodCliente} por monto {link.Monto}",
                TipProcesamiento = "AUTOMATICO"
            });

            return shortUrl;
        }

        public async Task<string> AcortarLinkPerifericoAsync(int codPeriferico, string urlLargo)
        {
            // 1. Validar periférico
            bool existe = await _linkRepository.ExistePerifericoAsync(codPeriferico);
            if (!existe)
            {
                throw new Exception($"El periférico {codPeriferico} no existe.");
            }

            // 2. Acortar URL
            string shortUrl = await _shortenerService.ShortenUrlAsync(urlLargo);

            // 3. Registrar Bitácora
            await _linkRepository.RegistraBitacoraBDAsync(urlLargo, shortUrl, codPeriferico);

            return shortUrl;
        }

        public async Task<string> ProcesarAcortamientoMasivoAsync()
        {
            int limit = 100;
            int totalProcesados = 0;
            bool tienePendientes = await _linkRepository.ExistenPendientesAsync() > 0;

            WriteLog("Iniciando proceso de acortamiento masivo.");

            while (tienePendientes)
            {
                var links = (await _linkRepository.ObtieneLinksAsync(limit)).ToList();
                if (links.Count == 0)
                {
                    break;
                }

                var urlsLargos = links.Select(l => l.LongLink).ToList();
                var urlsCortos = await _shortenerService.ShortenUrlsBulkAsync(urlsLargos);

                for (int i = 0; i < links.Count; i++)
                {
                    string shortUrl = urlsCortos.ElementAtOrDefault(i) ?? string.Empty;
                    if (!string.IsNullOrEmpty(shortUrl))
                    {
                        await _linkRepository.UpdateURLCortoAsync(links[i].CodConsecutivo, shortUrl);
                        totalProcesados++;
                    }
                }

                WriteLog($"Se procesó lote de {links.Count} enlaces.");

                tienePendientes = await _linkRepository.ExistenPendientesAsync() > 0;
            }

            string resultStr = $"EXITO. Total procesados: {totalProcesados}";
            WriteLog(resultStr);
            return resultStr;
        }

        public async Task<VisaLinkInfo?> ValidarYConsultaLinkAsync(string sku)
        {
            // 1. Consultar estado en Visa
            var info = await _visaService.ConsultaLinkAsync(sku);
            return info;
        }

        public async Task<bool> CancelarLinkAsync(string sku, string nombre, double precio, string username)
        {
            // 1. Cambiar estado a inactivo ('NO' / 'I') en Visa
            bool visaOk = await _visaService.CambioEstadoAsync(sku, nombre, precio, "I");
            if (!visaOk)
            {
                return false;
            }

            // 2. Actualizar estado local
            bool dbOk = await _linkRepository.UpdateEstadoLinkAsync(sku);
            if (!dbOk)
            {
                throw new Exception("Se inactivó el link en Visa pero no se pudo actualizar el estado local en la base de datos.");
            }

            // 3. Registrar en Bitácora
            await _siteRepository.RegistraBitacoraAsync(new BitacoraRequest
            {
                CodLink = sku,
                CodParametro = "CANCELACION",
                Descripcion = $"Se canceló/inactivó el link de pago {sku} por el usuario {username}",
                TipProcesamiento = "MANUAL"
            });

            return true;
        }

        private void WriteLog(string message)
        {
            _logger.LogInformation(message);
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory();
                string path = Path.Combine(baseDir, "Logs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filePath = Path.Combine(path, $"CreaLinks_Log_{DateTime.Now:yyyy_MM_dd}.txt");
                File.AppendAllText(filePath, $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} ==> {message}\r\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falla al escribir log de acortador masivo en archivo.");
            }
        }
    }
}
