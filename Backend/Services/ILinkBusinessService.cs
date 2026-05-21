using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Services
{
    public interface ILinkBusinessService
    {
        Task<string> EmitirLinkAsync(LinkEntity link, string imgPublicitaria, string username);
        Task<string> AcortarLinkPerifericoAsync(int codPeriferico, string urlLargo);
        Task<string> ProcesarAcortamientoMasivoAsync();
        Task<VisaLinkInfo?> ValidarYConsultaLinkAsync(string sku);
        Task<bool> CancelarLinkAsync(string sku, string nombre, double precio, string username);
    }
}
