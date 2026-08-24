using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface ISiteRepository
    {
        Task<SystemParameters?> GetParametrosAsync();
        Task<long> ObtenerCodigoInternoAsync();
        Task<bool> UpdateParametrosAsync(SystemParameters parameters, string username);
        Task<bool> InsertTokenAsync(string token);
        Task<string?> GetTokenInternoAsync();
        Task<bool> RegistraBitacoraAsync(BitacoraRequest request);
        Task<bool> RegistraBitacoraCoreAsync(BitCoreRequest request); // Se mantiene como método general
    }
}
