using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repositories
{
    public class MockClientRepository : IClientRepository
    {
        public Task<ClientEntity?> GetClienteCtaAsync(string numCta) =>
            Task.FromResult<ClientEntity?>(new ClientEntity { CodCliente = "C-99281", NomCliente = "PÉREZ HERNÁNDEZ, JUAN CARLOS" });

        public Task<PrestamoInfo?> GetTipoPrestamoAsync(string numCta) =>
            Task.FromResult<PrestamoInfo?>(new PrestamoInfo { NumCuenta = numCta, Moneda = "GTQ" });

        public Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente) =>
            Task.FromResult(false);

        public Task<string?> GetCorreoClienteAsync(string codCliente) =>
            Task.FromResult<string?>("cliente.demostracion@bancopromerica.com.gt");

        public Task<string?> GetTelefonoClienteAsync(string codCliente) =>
            Task.FromResult<string?>("55667788");

        public Task<IEnumerable<CuentaInfo>> GetCuentasAsync(string codCliente) =>
            Task.FromResult<IEnumerable<CuentaInfo>>(new List<CuentaInfo>
            {
                new CuentaInfo { NumCuenta = "4019283746", Tipo = "TC", Estado = "Activo" },
                new CuentaInfo { NumCuenta = "2023948576", Tipo = "PR", Estado = "Activo" }
            });
    }

    public class MockLinkRepository : ILinkRepository
    {
        public Task<string?> InsertLinkAsync(LinkEntity link) =>
            Task.FromResult<string?>("MOCK-LINK-ID-12345");

        public Task<bool> AplicaPagoPRAsync(PagoRequest pago, string moneda) =>
            Task.FromResult(true);

        public Task<bool> AplicaPagoTCAsync(PagoRequest pago, string moneda) =>
            Task.FromResult(true);

        public Task<LinkParametroInfo?> GetParametroAsync(string codLink) =>
            Task.FromResult<LinkParametroInfo?>(new LinkParametroInfo
            {
                NumCuenta = "4019283746",
                TipCuenta = "TC",
                TipPago = "Contado",
                MonCobro = "350.00"
            });

        public Task<LinkCtaInfo?> GetLinkCtaAsync(string numCta) =>
            Task.FromResult<LinkCtaInfo?>(new LinkCtaInfo { CodParametro = "VISA-ENL-101", DiaMes = "28", ProximaFecha = DateTime.Today.AddDays(7) });

        public Task<LinkCtaInfo?> GetLinkParametroAsync(string codParametro) =>
            Task.FromResult<LinkCtaInfo?>(new LinkCtaInfo { CodParametro = codParametro, DiaMes = "15", ProximaFecha = DateTime.Today.AddDays(15) });

        public Task<(IEnumerable<LinkListItem> Items, int TotalCount, int FilteredCount)> GetLinksPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username)
        {
            var list = new List<LinkListItem>
            {
                new LinkListItem
                {
                    Correlativo = "1",
                    Producto = "Pago Tarjeta Juan Pérez",
                    Monto = 350.00m,
                    Pago = "Contado",
                    EmisionLink = DateTime.Today.ToString("yyyy-MM-dd"),
                    Usuario = username,
                    Envio = "Correo",
                    TipoLink = "U"
                }
            };
            return Task.FromResult((list.AsEnumerable(), 1, 1));
        }

        public Task<(IEnumerable<LinkVerificaItem> Items, int TotalCount, int FilteredCount)> GetLinksVerificaPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username)
        {
            var list = new List<LinkVerificaItem>
            {
                new LinkVerificaItem { Correlativo = "1", Producto = "Pago Mínimo Tarjeta Oro", CodigoVisa = "VISA-992211", NumAuto = "102938", NumMov = "5002010", Edit = "N" }
            };
            return Task.FromResult((list.AsEnumerable(), 1, 1));
        }

        public Task<string?> NotificaSMSAsync(SmsRequest sms) => Task.FromResult<string?>("Notificación SMS enviada exitosamente (Simulado)");
        public Task<string?> NotificaMailAsync(MailRequest mail) => Task.FromResult<string?>("Notificación Mail enviada exitosamente (Simulado)");
        public Task<bool> UpdateEstadoLinkAsync(string codParametro) => Task.FromResult(true);
        public Task<IEnumerable<ShortLinkItem>> ObtieneLinksAsync(int limit) => Task.FromResult<IEnumerable<ShortLinkItem>>(new List<ShortLinkItem>());
        public Task<int> ExistenPendientesAsync() => Task.FromResult(0);
        public Task<bool> ExistePerifericoAsync(int codPeriferico) => Task.FromResult(false);
        public Task<bool> UpdateURLCortoAsync(decimal numConsecutivo, string urlCorto) => Task.FromResult(true);
        public Task<bool> RegistraBitacoraBDAsync(string urlLargo, string urlCorto, int codPeriferico) => Task.FromResult(true);
    }

    public class MockMenuRepository : IMenuRepository
    {
        public Task<IEnumerable<MenuItem>> GetMenuItemsAsync(string username, string systemCode) =>
            Task.FromResult<IEnumerable<MenuItem>>(new List<MenuItem>
            {
                new MenuItem { CodMenuItem = 1, Nombre = "Cobros Visa En Link", Path = "", Descripcion = "Gestión de cobros", CodItemPadre = 0, Visible = "S" },
                new MenuItem { CodMenuItem = 2, Nombre = "Emisión de Link", Path = "frmEmisionLink", Descripcion = "Emitir un nuevo link", CodItemPadre = 1, Visible = "S" },
                new MenuItem { CodMenuItem = 3, Nombre = "Activación de Link", Path = "frmActivacion", Descripcion = "Activación manual", CodItemPadre = 1, Visible = "S" },
                new MenuItem { CodMenuItem = 4, Nombre = "Cancelar Link", Path = "frmCancelarLink", Descripcion = "Cancelar programaciones", CodItemPadre = 1, Visible = "S" },
                new MenuItem { CodMenuItem = 5, Nombre = "Carga Masiva", Path = "frmCargaMasiva", Descripcion = "Carga de archivos masivos", CodItemPadre = 1, Visible = "S" },
                new MenuItem { CodMenuItem = 6, Nombre = "Verificación de Links", Path = "frmVerificacionLink", Descripcion = "Conciliación de pagos", CodItemPadre = 1, Visible = "S" },
                new MenuItem { CodMenuItem = 7, Nombre = "Parámetros del Sistema", Path = "frmParametros", Descripcion = "Configurar parámetros", CodItemPadre = 1, Visible = "S" }
            });

        public Task<string?> ValidateRRHHAsync(string username) => Task.FromResult<string?>("Activo");
        public Task<string?> ValidatePAAsync(string username) => Task.FromResult<string?>("Activo");

        public Task<IEnumerable<UserRoleInfo>> VerificarRolAsync(string username, string systemCode) =>
            Task.FromResult<IEnumerable<UserRoleInfo>>(new List<UserRoleInfo>
            {
                new UserRoleInfo { Usuario = username, Rol = 1, CodMenuItem = 1, Accion = "ALL", Sistema = int.TryParse(systemCode, out int sys) ? sys : 1 }
            });
    }

    public class MockProductRepository : IProductRepository
    {
        public Task<decimal?> GetMontoPRAsync(string numCuenta) => Task.FromResult<decimal?>(7500.00m);
        public Task<decimal?> GetMontoTCAsync(string numCuenta) => Task.FromResult<decimal?>(5000.00m);
        public Task<bool> ExisteCuentaAsync(string numCta) => Task.FromResult(true);
        public Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente) => Task.FromResult(false);
    }

    public class MockSiteRepository : ISiteRepository
    {
        public Task<SystemParameters?> GetParametrosAsync() => Task.FromResult<SystemParameters?>(new SystemParameters
        {
            FreRevAutorizacion = "15",
            FreRevHrsRepetir = "48",
            FreGenLink = "10",
            FreGenHora = "20:00",
            TcTipTransac = "V",
            TcSubtipTrans = "02",
            NumCtaContaQtz = "3049182736",
            NumCtaContaDol = "3049182740",
            CodAgencia = "099",
            CodTipoTc = "01",
            CodSubtipoTc = "01",
            CodTipoPr = "05",
            CodSubtipoPr = "05",
            CodDepartamento = "01",
            CodDeptoPr = "01",
            DesTransaccion = "Cobro Visa En Link Promerica",
            ApiImagenBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            MsgRemitente = "Banco Promerica Guatemala",
            MsgHeader = "Estimado Cliente, Banco Promerica le envía su link de pago seguro:",
            MsgFooter = "Si tiene alguna consulta, llámenos al PBX: 1724. Banco Promerica.",
            MsgSms = "Promerica: Adjuntamos link de pago para su cuenta: {link}"
        });

        public Task<long> ObtenerCodigoInternoAsync() => Task.FromResult(123456L);
        public Task<bool> UpdateParametrosAsync(SystemParameters parameters, string username) => Task.FromResult(true);
        public Task<bool> InsertTokenAsync(string token) => Task.FromResult(true);
        public Task<string?> GetTokenInternoAsync() => Task.FromResult<string?>("mock-internal-token-9988");
        public Task<bool> RegistraBitacoraAsync(BitacoraRequest request) => Task.FromResult(true);
        public Task<bool> RegistraBitacoraCoreAsync(BitCoreRequest request) => Task.FromResult(true);
    }

    public class MockTransactionRepository : ITransactionRepository
    {
        public Task<PagedTransactionResult> GetTransaccionesAsync(TransactionQueryRequest request) =>
            Task.FromResult(new PagedTransactionResult
            {
                TotalRecords = 1,
                FilteredRecords = 1,
                Data = new List<TransactionGridItem>
                {
                    new TransactionGridItem
                    {
                        CodTransaccion = "TR-1002",
                        CodCli = "C-99281",
                        NomCliente = "PÉREZ HERNÁNDEZ, JUAN CARLOS",
                        NumCta = "4019283746",
                        MonEfectivo = "350.00",
                        FecIngreso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                }
            });
    }

    public class MockCarrierRepository : ICarrierRepository
    {
        public Task<bool> InsertUsuarioAsync(CarrierModel carrier, string username) => Task.FromResult(true);
        public Task<bool> UpdateUsuarioAsync(CarrierModel carrier, string username) => Task.FromResult(true);
        public Task<bool> InsertTransportadoraAsync(CarrierModel carrier) => Task.FromResult(true);
        public Task<bool> UpdateTransportadoraAsync(CarrierModel carrier) => Task.FromResult(true);
        public Task<CarrierModel?> GetTransportadoraAsync(string usuario) => Task.FromResult<CarrierModel?>(new CarrierModel
        {
            CodTranspo = "100",
            NomTranspo = "Guatex Express",
            Direccion = "Ciudad de Guatemala",
            Telefono = "55667788",
            NomEncargado = "Carlos Archila",
            IndEstado = "A",
            Clave = "12345"
        });

        public Task<IEnumerable<CarrierModel>> GetTransportadorasAsync() => Task.FromResult<IEnumerable<CarrierModel>>(new List<CarrierModel>
        {
            new CarrierModel
            {
                CodTranspo = "100",
                NomTranspo = "Guatex Express",
                Direccion = "Ciudad de Guatemala",
                Telefono = "55667788",
                NomEncargado = "Carlos Archila",
                IndEstado = "A",
                Clave = "12345"
            }
        });

        public Task<IEnumerable<CarrierDropdownItem>> GetTransportadorasDLLAsync(string codCliAci = "") =>
            Task.FromResult<IEnumerable<CarrierDropdownItem>>(new List<CarrierDropdownItem>
            {
                new CarrierDropdownItem { CodTranspo = "100", NomTranspo = "Guatex Express" }
            });
    }
}
