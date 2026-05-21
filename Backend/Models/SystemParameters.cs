using System;

namespace Backend.Models
{
    public class SystemParameters
    {
        public string FreRevAutorizacion { get; set; } = string.Empty;
        public string FreRevHrsRepetir { get; set; } = string.Empty;
        public string FreGenLink { get; set; } = string.Empty;
        public string FreGenHora { get; set; } = string.Empty;
        public string TcTipTransac { get; set; } = string.Empty;
        public string TcSubtipTrans { get; set; } = string.Empty;
        public string NumCtaContaQtz { get; set; } = string.Empty;
        public string NumCtaContaDol { get; set; } = string.Empty;
        public string CodAgencia { get; set; } = string.Empty;
        public string CodTipoTc { get; set; } = string.Empty;
        public string CodSubtipoTc { get; set; } = string.Empty;
        public string CodTipoPr { get; set; } = string.Empty;
        public string CodSubtipoPr { get; set; } = string.Empty;
        public string CodDepartamento { get; set; } = string.Empty;
        public string CodDeptoPr { get; set; } = string.Empty;
        public string DesTransaccion { get; set; } = string.Empty;
        public string ApiImagenBase64 { get; set; } = string.Empty;
        public string MsgRemitente { get; set; } = string.Empty;
        public string MsgHeader { get; set; } = string.Empty;
        public string MsgFooter { get; set; } = string.Empty;
        public string MsgSms { get; set; } = string.Empty;
    }

    public class BitacoraRequest
    {
        public string CodLink { get; set; } = string.Empty;
        public string CodParametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipProcesamiento { get; set; } = string.Empty;
    }

    public class BitCoreRequest
    {
        public string CodPersona { get; set; } = string.Empty;
        public string NumCtaCredito { get; set; } = string.Empty;
        public string NumCtaPrestamo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipCuenta { get; set; } = string.Empty;
    }
}
