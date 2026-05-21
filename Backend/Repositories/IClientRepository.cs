using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface IClientRepository
    {
        Task<ClientEntity?> GetClienteCtaAsync(string numCta);
        Task<PrestamoInfo?> GetTipoPrestamoAsync(string numCta);
        Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente);
        Task<string?> GetCorreoClienteAsync(string codCliente);
        Task<string?> GetTelefonoClienteAsync(string codCliente);
        Task<IEnumerable<CuentaInfo>> GetCuentasAsync(string codCliente);
    }
}
