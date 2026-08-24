using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace Backend.Services
{
    public class VentaData
    {
        public string? Autorizacion { get; set; }
    }

    public class VisaLinkInfo
    {
        public string Sku { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        // public string Autorizacion { get; set; } = string.Empty; // Replaced by Ventas list
        public string Estado { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double Monto { get; set; }
        public string Moneda { get; set; } = string.Empty;
        // Add Ventas to match the real API structure and frontend expectation
        public List<VentaData> Ventas { get; set; } = new();
    }

    public interface IVisaEnLinkIntegrationService
    {
        Task<string> GetOrGenerateTokenAsync();
        Task<(string SKU, string LinkUrl)> CrearLinkAsync(string producto, string monto, string imgPublicitaria, string codigoInterno);
        Task<VisaLinkInfo?> ConsultaLinkAsync(string sku);
        Task<bool> CambioEstadoAsync(string sku, string nombre, double precio, string estado);
    }
}
