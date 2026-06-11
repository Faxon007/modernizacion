using System;

namespace Backend.Models
{
    public class LinkEntity
    {
        public string NumCuenta { get; set; } = string.Empty;
        public string TipCuenta { get; set; } = string.Empty;
        public string MonCobro { get; set; } = string.Empty;
        public decimal Monto 
        {
            get => decimal.TryParse(MonCobro, out decimal m) ? m : 0;
            set => MonCobro = value.ToString("F2");
        }
        public string TipPago { get; set; } = string.Empty;
        public string EsDefault { get; set; } = string.Empty;
        public string TipEnvio { get; set; } = string.Empty;
        public string NumTelefono { get; set; } = string.Empty;
        public string NomCorreo { get; set; } = string.Empty;
        public string TipLink { get; set; } = string.Empty;
        public string DiaMes { get; set; } = string.Empty;
        public string UrlLink { get; set; } = string.Empty;
        public string LongLink
        {
            get => UrlLink;
            set => UrlLink = value;
        }
        public string UrlCorto { get; set; } = string.Empty;
        public string ShortLink
        {
            get => UrlCorto;
            set => UrlCorto = value;
        }
        public string IndEstado { get; set; } = string.Empty;
        public string CodSku { get; set; } = string.Empty;
        public string CodLink
        {
            get => CodSku;
            set => CodSku = value;
        }
        public string NomProducto { get; set; } = string.Empty;
        public string CodCliente { get; set; } = string.Empty;
        public string UsuIngreso { get; set; } = string.Empty;
    }

    public class PagoRequest
    {
        public string NumCta { get; set; } = string.Empty;
        public string CodSku { get; set; } = string.Empty;
        public string CodLink { get; set; } = string.Empty;
        public string MonPago { get; set; } = string.Empty;
        public string AutVisa { get; set; } = string.Empty;
    }

    public class SmsRequest
    {
        public string NumCta { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }

    public class MailRequest
    {
        public string Mail { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }

    public class LinkListItem
    {
        public string Correlativo { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Pago { get; set; } = string.Empty;
        public string EmisionLink { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Envio { get; set; } = string.Empty;
        public string TipoLink { get; set; } = string.Empty;
    }

    public class LinkVerificaItem
    {
        public string Correlativo { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string CodigoVisa { get; set; } = string.Empty;
        public string NumAuto { get; set; } = string.Empty;
        public string NumMov { get; set; } = string.Empty;
        public string Edit { get; set; } = string.Empty;
    }

    public class LinkParametroInfo
    {
        public string CodLink { get; set; } = string.Empty;
        public string NumCuenta { get; set; } = string.Empty;
        public string TipCuenta { get; set; } = string.Empty;
        public string TipPago { get; set; } = string.Empty;
        public string MonCobro { get; set; } = string.Empty;
    }

    public class LinkCtaInfo
    {
        public string CodParametro { get; set; } = string.Empty;
        public string DiaMes { get; set; } = string.Empty;
        public DateTime ProximaFecha { get; set; }
    }

    public class ShortLinkItem
    {
        public decimal CodConsecutivo { get; set; }
        public string LongLink { get; set; } = string.Empty;
    }
}
