using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IUrlShortenerService
    {
        Task<string> ShortenUrlAsync(string destinationUrl);
        Task<List<string>> ShortenUrlsBulkAsync(List<string> destinationUrls);
    }
}
