using System.Threading.Tasks;

namespace Backend.Services
{
    public class VisaLinkInfo
    {
        public string Sku { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string Autorizacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double Monto { get; set; }
    }

    public interface IVisaEnLinkIntegrationService
    {
        Task<string> GetOrGenerateTokenAsync();
        Task<(string SKU, string LinkUrl)> CrearLinkAsync(string producto, string monto, string imgPublicitaria, string codigoInterno);
        Task<VisaLinkInfo?> ConsultaLinkAsync(string sku);
        Task<bool> CambioEstadoAsync(string sku, string nombre, double precio, string estado);
    }
}
