using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface ITransactionRepository
    {
        Task<PagedTransactionResult> GetTransaccionesAsync(TransactionQueryRequest request);
    }
}
