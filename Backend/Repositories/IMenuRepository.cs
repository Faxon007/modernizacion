using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuItem>> GetMenuItemsAsync(string username, string systemCode);
        Task<string?> ValidateRRHHAsync(string username);
        Task<string?> ValidatePAAsync(string username);
        Task<IEnumerable<UserRoleInfo>> VerificarRolAsync(string username, string systemCode);
    }
}
