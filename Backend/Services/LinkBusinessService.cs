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

            // 2. Registrar producto/link en Neo
            // Monto formateado para la API de Neo
            string montoStr = link.Monto.ToString("F2");
            var (visaSku, visaUrl) = await _visaService.CrearLinkAsync(link.NomProducto, montoStr, imgPublicitaria, sku);

            link.LongLink = visaUrl;

            // 3. Acortar el URL de Neo obtenido
            string shortUrl = await _shortenerService.ShortenUrlAsync(visaUrl);
            link.ShortLink = shortUrl;

            // 4. Registrar en base de datos local
            string? resultMsg = await _linkRepository.InsertLinkAsync(link);
            if (!string.IsNullOrEmpty(resultMsg))
            {
                throw new Exception($"Error al insertar link localmente: {resultMsg}");
            }

            // 5. Notificar al cliente si no es un link programado
            if (link.TipLink != "1") // '1' es programado, '2' es único
            {
                try
                {
                    var parametros = await _siteRepository.GetParametrosAsync();
                    if (parametros == null)
                    {
                        _logger.LogWarning("No se pudieron obtener los parámetros del sistema para enviar la notificación.");
                    }
                    else if (link.TipEnvio == "1" && !string.IsNullOrEmpty(link.NumTelefono)) // Enviar SMS
                    {
                        string mensajeSms = parametros.MsgSms +" "+shortUrl;
                        string? errorNotificacion = await _linkRepository.NotificaSMSAsync(new SmsRequest { NumCta = link.NumCuenta, Telefono = link.NumTelefono, Mensaje = mensajeSms });
                        if (!string.IsNullOrEmpty(errorNotificacion))
                        {
                            _logger.LogWarning("La notificación por SMS para el SKU {Sku} retornó un mensaje: {Error}", sku, errorNotificacion);
                        }
                        _logger.LogInformation("Notificación SMS enviada a {Telefono} para el SKU {Sku}", link.NumTelefono, sku);
                    }
                    else if (link.TipEnvio == "2" && !string.IsNullOrEmpty(link.NomCorreo)) // Enviar Correo
                    {
                        
                        //string asunto = string.IsNullOrWhiteSpace(parametros.MsgRemitente) ? "Notificación de Link de Pago" : parametros.MsgRemitente;
                        string asunto = "Pago En Link";
                        string cuerpo = $"{parametros.MsgHeader ?? "Estimado cliente, su link de pago es:"}\n\n{shortUrl}\n\n{parametros.MsgFooter ?? "Gracias por su preferencia."}";
                        await _linkRepository.NotificaMailAsync(new MailRequest { Mail = link.NomCorreo, Asunto = asunto, Link = cuerpo });
                        _logger.LogInformation("Notificación por correo enviada a {Correo} para el SKU {Sku}", link.NomCorreo, sku);
                    }
                }
                catch (Exception ex)
                {
                    // Se registra la advertencia pero no se detiene el flujo principal,
                    // ya que el link ya fue creado exitosamente.
                    _logger.LogWarning(ex, "El link se creó correctamente, pero falló el envío de la notificación para el SKU {Sku}.", sku);
                }
            }

            // 6. Registrar bitácoras de creación y notificación.
            await _registrarBitacorasDeEmisionAsync(link, sku);

            return shortUrl;
        }

        /// <summary>
        /// Encapsula la lógica de los dos registros de bitácora (creación y notificación)
        /// para replicar el comportamiento del método original.
        /// </summary>
        private async Task _registrarBitacorasDeEmisionAsync(LinkEntity link, string sku)
        {
            // 1. Bitácora de Creación
            // Equivalente a la primera parte de `RegistraBitacora`
            string creationDescription = $"Se creo link ({sku}) asociado al número de cuenta ({link.TipCuenta}) No.{link.NumCuenta}";
            await _siteRepository.RegistraBitacoraAsync(new BitacoraRequest
            {
                CodLink = "", // El original no enviaba el cod_link
                CodParametro = "", // El original no enviaba el cod_parametro
                Descripcion = creationDescription,
                TipProcesamiento = "C" // 'C' para Creación
            });

            // 2. Bitácora del Core (Notificación)
            // Equivalente a la segunda parte de `RegistraBitacora`
            // Solo se ejecuta si el link no es programado (es decir, si se envió una notificación)
            if (link.TipLink != "1")
            {
                string notificacionStr = "";
                if (link.TipEnvio == "1") // SMS
                {
                    notificacionStr = $"Teléfono: {link.NumTelefono}";
                }
                else if (link.TipEnvio == "2") // Correo
                {
                    notificacionStr = $"Correo: {link.NomCorreo}";
                }

                string coreDescription = $"Se realiza envío de Link para pago al {notificacionStr} por {(link.TipPago == "1" ? "$" : "Q")}{link.Monto:F2}";
                var bitCore = new BitCoreRequest
                {
                    CodPersona = link.CodCliente,
                    Descripcion = coreDescription,
                    NumCtaCredito = link.TipCuenta == "TC" ? link.NumCuenta : null,
                    NumCtaPrestamo = link.TipCuenta == "PR" ? link.NumCuenta : null
                };

                await _siteRepository.RegistraBitacoraCoreAsync(bitCore);
            }
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

                var updates = new List<(decimal NumConsecutivo, string UrlCorto)>();
                for (int i = 0; i < links.Count; i++)
                {
                    string shortUrl = urlsCortos.ElementAtOrDefault(i) ?? string.Empty;
                    if (!string.IsNullOrEmpty(shortUrl))
                    {
                        updates.Add((links[i].CodConsecutivo, shortUrl));
                    }
                }

                if (updates.Any())
                {
                    await _linkRepository.UpdateURLCortosBulkAsync(updates);
                    totalProcesados += updates.Count;
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
            // 1. Consultar estado en Neo
            var info = await _visaService.ConsultaLinkAsync(sku);
            return info;
        }

        public async Task<bool> CancelarLinkAsync(string sku, string nombre, double precio, string username)
        {
            // 1. Cambiar estado a inactivo ('NO' / 'I') en Neo
            bool visaOk = await _visaService.CambioEstadoAsync(sku, nombre, precio, "I");
            if (!visaOk)
            {
                return false;
            }

            // 2. Actualizar estado local
            bool dbOk = await _linkRepository.UpdateEstadoLinkAsync(sku);
            if (!dbOk)
            {
                throw new Exception("Se inactivó el link en Neo pero no se pudo actualizar el estado local en la base de datos.");
            }

            // 3. Registrar en Bitácora
            await _siteRepository.RegistraBitacoraAsync(new BitacoraRequest
            {
                CodLink = sku,
                CodParametro = "CANCELACION",
                Descripcion = $"Se canceló/inactivó el link de pago {sku} por el usuario {username}",
                TipProcesamiento = "M" // Cambiado de "MANUAL" a "M"
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
