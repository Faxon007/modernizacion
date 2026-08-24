using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;
using Backend.Infrastructure.Database;

namespace Backend.Repositories
{
    public class SiteRepository(string connectionString, ILogger<SiteRepository> logger) : ISiteRepository
    {
        private readonly ILogger<SiteRepository> _logger = logger;

        public async Task<SystemParameters?> GetParametrosAsync()
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT FRE_REV_AUTO AS FreRevAutorizacion, 
                                  FRE_REV_HRS_REP AS FreRevHrsRepetir, 
                                  FRE_GEN_LINK AS FreGenLink,     
                                  FRE_GEN_HORA AS FreGenHora,     
                                  TC_TIP_TRANSAC AS TcTipTransac,   
                                  TC_SUBTIP_TRANS AS TcSubtipTrans,    
                                  NUM_CTA_CONTA_QTZ AS NumCtaContaQtz,   
                                  NUM_CTA_CONTA_DOL AS NumCtaContaDol,     
                                  COD_AGENCIA AS CodAgencia,        
                                  COD_TIPO_TC AS CodTipoTc,
                                  COD_SUBTIPO_TC AS CodSubtipoTc,
                                  COD_TIPO_PR AS CodTipoPr,
                                  COD_SUBTIPO_PR AS CodSubtipoPr,
                                  COD_DEPARTAMENTO AS CodDepartamento,
                                  COD_DEPTO_PR AS CodDeptoPr,
                                  DES_TRANSACCION AS DesTransaccion, 
                                  API_IMAGEN,      
                                  MSG_REMITENTE AS MsgRemitente, 
                                  MSG_HEADER,    
                                  MSG_FOOTER,    
                                  MSG_SMS AS MsgSms
                           FROM BO.SCL_PARAM_SISTEMA";

            // Since Dapper cannot directly map BLOB columns to string properties, we read as dynamic and convert.
            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(sql);
            if (row == null) return null;

            var sysParams = new SystemParameters
            {
                FreRevAutorizacion = Convert.ToString(row.FREREVAUTORIZACION) ?? string.Empty,
                FreRevHrsRepetir = Convert.ToString(row.FREREVHRSREPETIR) ?? string.Empty,
                FreGenLink = Convert.ToString(row.FREGENLINK) ?? string.Empty,
                FreGenHora = Convert.ToString(row.FREGENHORA) ?? string.Empty,
                TcTipTransac = Convert.ToString(row.TCTIPTRANSAC) ?? string.Empty,
                TcSubtipTrans = Convert.ToString(row.TCSUBTIPTRANS) ?? string.Empty,
                NumCtaContaQtz = Convert.ToString(row.NUMCTACONTAQTZ) ?? string.Empty,
                NumCtaContaDol = Convert.ToString(row.NUMCTACONTADOL) ?? string.Empty,
                CodAgencia = Convert.ToString(row.CODAGENCIA) ?? string.Empty,
                CodTipoTc = Convert.ToString(row.CODTIPOTC) ?? string.Empty,
                CodSubtipoTc = Convert.ToString(row.CODSUBTIPOTC) ?? string.Empty,
                CodTipoPr = Convert.ToString(row.CODTIPOPR) ?? string.Empty,
                CodSubtipoPr = Convert.ToString(row.CODSUBTIPOPR) ?? string.Empty,
                CodDepartamento = Convert.ToString(row.CODDEPARTAMENTO) ?? string.Empty,
                CodDeptoPr = Convert.ToString(row.CODDEPTOPR) ?? string.Empty,
                DesTransaccion = Convert.ToString(row.DESTRANSACCION) ?? string.Empty,
                MsgRemitente = Convert.ToString(row.MSGREMITENTE) ?? string.Empty,
                MsgSms = Convert.ToString(row.MSGSMS) ?? string.Empty
            };

            if (row.API_IMAGEN is byte[] imgBytes)
            {
                sysParams.ApiImagenBase64 = "data:image/png;base64," + Convert.ToBase64String(imgBytes);
            }

            if (row.MSG_HEADER is byte[] headerBytes)
            {
                sysParams.MsgHeader = Encoding.UTF8.GetString(headerBytes);
            }

            if (row.MSG_FOOTER is byte[] footerBytes)
            {
                sysParams.MsgFooter = Encoding.UTF8.GetString(footerBytes);
            }

            return sysParams;
        }

        public async Task<long> ObtenerCodigoInternoAsync()
        {
            using var conn = new OracleConnection(connectionString);
            string sql = "SELECT S_SCL_CORRELATIVO.NEXTVAL FROM DUAL";
            return await conn.ExecuteScalarAsync<long>(sql);
        }

        public async Task<bool> UpdateParametrosAsync(SystemParameters parameters, string username)
        {
            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                string checkSql = "SELECT COUNT(*) FROM BO.SCL_PARAM_SISTEMA";
                int exists = await conn.ExecuteScalarAsync<int>(checkSql, transaction: tx);

                byte[]? imgData = null;
                if (!string.IsNullOrEmpty(parameters.ApiImagenBase64))
                {
                    string base64 = parameters.ApiImagenBase64;
                    if (base64.Contains(","))
                    {
                        base64 = base64.Split(',')[1];
                    }
                    imgData = Convert.FromBase64String(base64);
                }

                byte[] headerBytes = Encoding.UTF8.GetBytes(parameters.MsgHeader);
                byte[]? footerBytes = !string.IsNullOrEmpty(parameters.MsgFooter) 
                    ? Encoding.UTF8.GetBytes(parameters.MsgFooter) 
                    : null;

                // Safely parse numeric properties
                int? freRevHrsRepetir = int.TryParse(parameters.FreRevHrsRepetir, out int f1) ? f1 : null;
                int? tcTipTransac = int.TryParse(parameters.TcTipTransac, out int f2) ? f2 : null;
                int? codTipoTc = int.TryParse(parameters.CodTipoTc, out int f3) ? f3 : null;
                int? codSubtipoTc = int.TryParse(parameters.CodSubtipoTc, out int f4) ? f4 : null;
                int? codTipoPr = int.TryParse(parameters.CodTipoPr, out int f5) ? f5 : null;
                int? codSubtipoPr = int.TryParse(parameters.CodSubtipoPr, out int f6) ? f6 : null;

                var queryParams = new
                {
                    FreRevAuto = parameters.FreRevAutorizacion,
                    FreRevHrsRep = freRevHrsRepetir,
                    FreGenLink = parameters.FreGenLink,
                    FreGenHora = parameters.FreGenHora,
                    TcTipTransac = tcTipTransac,
                    TcSubtipTrans = parameters.TcSubtipTrans,
                    NumCtaContaQtz = parameters.NumCtaContaQtz,
                    NumCtaContaDol = parameters.NumCtaContaDol,
                    CodAgencia = parameters.CodAgencia,
                    CodTipoTc = codTipoTc,
                    CodSubtipoTc = codSubtipoTc,
                    CodTipoPr = codTipoPr,
                    CodSubtipoPr = codSubtipoPr,
                    CodDepartamento = parameters.CodDepartamento,
                    DesTransaccion = parameters.DesTransaccion,
                    Imagen = imgData,
                    MsgRemitente = parameters.MsgRemitente,
                    Header = headerBytes,
                    Footer = footerBytes,
                    MsgSms = parameters.MsgSms,
                    CodDeptoPr = parameters.CodDeptoPr,
                    Usuario = username
                };

                if (exists == 0)
                {
                    string insertSql = @"INSERT INTO BO.SCL_PARAM_SISTEMA (
                                            FRE_REV_AUTO, FRE_REV_HRS_REP, FRE_GEN_LINK, FRE_GEN_HORA,
                                            TC_TIP_TRANSAC, TC_SUBTIP_TRANS, NUM_CTA_CONTA_QTZ, NUM_CTA_CONTA_DOL,
                                            COD_AGENCIA, COD_TIPO_TC, COD_SUBTIPO_TC, COD_TIPO_PR, 
                                            COD_SUBTIPO_PR, COD_DEPARTAMENTO, DES_TRANSACCION, API_IMAGEN, 
                                            MSG_REMITENTE, MSG_HEADER, MSG_FOOTER, MSG_SMS, COD_DEPTO_PR, USU_INGRESO, FEC_INGRESO
                                         ) VALUES (
                                            :FreRevAuto, :FreRevHrsRep, :FreGenLink, :FreGenHora,
                                            :TcTipTransac, :TcSubtipTrans, :NumCtaContaQtz, :NumCtaContaDol,
                                            :CodAgencia, :CodTipoTc, :CodSubtipoTc, :CodTipoPr,
                                            :CodSubtipoPr, :CodDepartamento, :DesTransaccion, :Imagen,
                                            :MsgRemitente, :Header, :Footer, :MsgSms, :CodDeptoPr, :Usuario, SYSDATE
                                         )";

                    await conn.ExecuteAsync(insertSql, queryParams, transaction: tx);
                }
                else
                {
                    string updateSql = @"UPDATE BO.SCL_PARAM_SISTEMA SET
                                            FRE_REV_AUTO = :FreRevAuto,
                                            FRE_REV_HRS_REP = :FreRevHrsRep,
                                            FRE_GEN_LINK = :FreGenLink,
                                            FRE_GEN_HORA = :FreGenHora,
                                            TC_TIP_TRANSAC = :TcTipTransac,
                                            TC_SUBTIP_TRANS = :TcSubtipTrans,
                                            NUM_CTA_CONTA_QTZ = :NumCtaContaQtz,
                                            NUM_CTA_CONTA_DOL = :NumCtaContaDol,
                                            COD_AGENCIA = :CodAgencia,
                                            COD_TIPO_TC = :CodTipoTc,
                                            COD_SUBTIPO_TC = :CodSubtipoTc,
                                            COD_TIPO_PR = :CodTipoPr,
                                            COD_SUBTIPO_PR = :CodSubtipoPr,
                                            COD_DEPARTAMENTO = :CodDepartamento,
                                            DES_TRANSACCION = :DesTransaccion,
                                            API_IMAGEN = :Imagen,
                                            MSG_REMITENTE = :MsgRemitente,
                                            MSG_HEADER = :Header,
                                            MSG_FOOTER = :Footer,
                                            MSG_SMS = :MsgSms,
                                            COD_DEPTO_PR = :CodDeptoPr,
                                            USU_MODIFICA = :Usuario,
                                            FEC_MODIFICA = SYSDATE";

                    await conn.ExecuteAsync(updateSql, queryParams, transaction: tx);
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> InsertTokenAsync(string token)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = "INSERT INTO BO.SCL_TOKEN_LINK (VAL_TOKEN) VALUES (:token)";
            var affected = await conn.ExecuteAsync(sql, new { token });
            return affected > 0;
        }

        public async Task<string?> GetTokenInternoAsync()
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT VAL_TOKEN 
                           FROM BO.SCL_TOKEN_LINK 
                           WHERE TRUNC(FEC_EMISION) = TRUNC(SYSDATE)
                           ORDER BY FEC_EMISION DESC 
                           FETCH FIRST 1 ROW ONLY";
            return await conn.QueryFirstOrDefaultAsync<string>(sql);
        }

        public async Task<bool> RegistraBitacoraAsync(BitacoraRequest request)
        {
            using var conn = new OracleConnection(connectionString);
            
            // Attempt to parse CodLink and CodParametro to decimal.
            // If parsing fails (e.g., "EMISION" for CodParametro, or empty string for CodLink),
            // pass null to the Oracle stored procedure, as it likely expects a NUMBER type.
            decimal? codLinkNumeric = decimal.TryParse(request.CodLink, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cl) ? cl : null;
            decimal? codParametroNumeric = decimal.TryParse(request.CodParametro, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cp) ? cp : null;

            var p = new OracleDynamicParameters();
            p.Add("p_CodLink", codLinkNumeric, OracleDbType.Decimal, ParameterDirection.Input);
            p.Add("p_CodParametro", codParametroNumeric, OracleDbType.Decimal, ParameterDirection.Input);
            p.Add("p_Descripcion", request.Descripcion, OracleDbType.Varchar2, ParameterDirection.Input); 
            p.Add("p_TipoProcesamiento", request.TipProcesamiento, OracleDbType.Varchar2, ParameterDirection.Input);
            p.Add("P_MsgError", dbType: OracleDbType.Varchar2, direction: ParameterDirection.InputOutput, size: 2000, value: "NULL");

            _logger.LogDebug("RegistraBitacoraAsync: Calling PkgScl_InsBitacoraLink with p_CodLink={CodLink}, p_CodParametro={CodParametro}, p_Descripcion='{Descripcion}', p_TipoProcesamiento='{TipoProcesamiento}'",
                codLinkNumeric, codParametroNumeric, request.Descripcion, request.TipProcesamiento);

            await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_InsBitacoraLink", p, commandType: CommandType.StoredProcedure);
            string? error = p.Get<string>("P_MsgError");
            if (!string.IsNullOrEmpty(error) && !error.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Procedimiento PkgScl_InsBitacoraLink retornó error: {error}");
            }
            return true;
        }

        public async Task<bool> RegistraBitacoraCoreAsync(BitCoreRequest request)
        {
            try
            {
                using var conn = new OracleConnection(connectionString);
                var p = new OracleDynamicParameters();
                p.Add("P_CodPersona", request.CodPersona, OracleDbType.Varchar2, ParameterDirection.Input);
                p.Add("P_NumCtaCredito", string.IsNullOrEmpty(request.NumCtaCredito) ? null : request.NumCtaCredito, OracleDbType.Varchar2, ParameterDirection.Input);
                p.Add("P_NumPrestamo", string.IsNullOrEmpty(request.NumCtaPrestamo) ? null : request.NumCtaPrestamo, OracleDbType.Varchar2, ParameterDirection.Input);
                p.Add("P_DesDetalle", request.Descripcion, OracleDbType.Varchar2, ParameterDirection.Input);
                p.Add("P_MsgError", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 2000);

                await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_InsBitacoraCore", p, commandType: CommandType.StoredProcedure);
                string? error = p.Get<string>("P_MsgError");
                if (!string.IsNullOrEmpty(error) && !error.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError(error,"Procedimiento PkgScl_InsBitacoraCore retornó error: {error}");
                    throw new Exception($"Procedimiento PkgScl_InsBitacoraCore retornó error: {error}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al registrar la bitácora del core. El flujo principal no se verá afectado. Descripción: {Descripcion}", request.Descripcion);
                return false; // Retorna false pero no lanza la excepción hacia arriba.
            }
        }
    }
}
