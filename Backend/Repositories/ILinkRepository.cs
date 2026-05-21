using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public interface ILinkRepository
    {
        Task<string?> InsertLinkAsync(LinkEntity link);
        Task<bool> AplicaPagoPRAsync(PagoRequest pago, string moneda);
        Task<bool> AplicaPagoTCAsync(PagoRequest pago, string moneda);
        Task<LinkParametroInfo?> GetParametroAsync(string codLink);
        Task<LinkCtaInfo?> GetLinkCtaAsync(string numCta);
        Task<LinkCtaInfo?> GetLinkParametroAsync(string codParametro);
        Task<(IEnumerable<LinkListItem> Items, int TotalCount, int FilteredCount)> GetLinksPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username);
        Task<(IEnumerable<LinkVerificaItem> Items, int TotalCount, int FilteredCount)> GetLinksVerificaPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username);
        Task<string?> NotificaSMSAsync(SmsRequest sms);
        Task<string?> NotificaMailAsync(MailRequest mail);
        Task<bool> UpdateEstadoLinkAsync(string codParametro);
        Task<IEnumerable<ShortLinkItem>> ObtieneLinksAsync(int limit);
        Task<int> ExistenPendientesAsync();
        Task<bool> ExistePerifericoAsync(int codPeriferico);
        Task<bool> UpdateURLCortoAsync(decimal numConsecutivo, string urlCorto);
        Task<bool> RegistraBitacoraBDAsync(string urlLargo, string urlCorto, int codPeriferico);
    }
}
