using System.Threading.Tasks;

namespace Backend.Repositories
{
    public interface IProductRepository
    {
        Task<decimal?> GetMontoPRAsync(string numCuenta);
        Task<decimal?> GetMontoTCAsync(string numCuenta);
        Task<bool> ExisteCuentaAsync(string numCta);
        Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente);
    }
}
