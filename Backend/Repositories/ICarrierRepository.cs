using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface ICarrierRepository
    {
        Task<bool> InsertUsuarioAsync(CarrierModel carrier, string username);
        Task<bool> UpdateUsuarioAsync(CarrierModel carrier, string username);
        Task<bool> InsertTransportadoraAsync(CarrierModel carrier);
        Task<bool> UpdateTransportadoraAsync(CarrierModel carrier);
        Task<CarrierModel?> GetTransportadoraAsync(string usuario);
        Task<IEnumerable<CarrierModel>> GetTransportadorasAsync();
        Task<IEnumerable<CarrierDropdownItem>> GetTransportadorasDLLAsync(string codCliAci = "");
    }
}
