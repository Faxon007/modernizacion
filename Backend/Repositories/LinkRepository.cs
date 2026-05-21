using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;

namespace Backend.Repositories
{
    public class LinkRepository(string connectionString) : ILinkRepository
    {
        public async Task<string?> InsertLinkAsync(LinkEntity link)
        {
            using var conn = new OracleConnection(connectionString);
            var p = new DynamicParameters();
            
            p.Add("p_NumCuenta", string.IsNullOrEmpty(link.NumCuenta) ? (object)DBNull.Value : decimal.Parse(link.NumCuenta), DbType.Decimal, ParameterDirection.Input);
            p.Add("p_TipCuenta", link.TipCuenta, DbType.String, ParameterDirection.Input);
            p.Add("p_MonCobro", string.IsNullOrEmpty(link.MonCobro) ? (object)DBNull.Value : decimal.Parse(link.MonCobro), DbType.Decimal, ParameterDirection.Input);
            p.Add("p_TipPago", link.TipPago, DbType.String, ParameterDirection.Input);
            p.Add("p_EsDefault", string.IsNullOrEmpty(link.EsDefault) ? "NULL" : link.EsDefault, DbType.String, ParameterDirection.Input);
            p.Add("p_TipEnvio", link.TipEnvio, DbType.String, ParameterDirection.Input);
            p.Add("p_NumTelefono", string.IsNullOrEmpty(link.NumTelefono) ? "NULL" : link.NumTelefono, DbType.String, ParameterDirection.Input);
            p.Add("p_NomCorreo", string.IsNullOrEmpty(link.NomCorreo) ? "NULL" : link.NomCorreo, DbType.String, ParameterDirection.Input);
            p.Add("p_TipLink", link.TipLink, DbType.String, ParameterDirection.Input);
            p.Add("p_DiaMes", string.IsNullOrEmpty(link.DiaMes) ? "NULL" : link.DiaMes, DbType.String, ParameterDirection.Input);
            p.Add("p_IndEstado", link.IndEstado, DbType.String, ParameterDirection.Input);
            p.Add("p_CodSku", string.IsNullOrEmpty(link.CodSku) ? "NULL" : link.CodSku, DbType.String, ParameterDirection.Input);
            p.Add("p_Url", string.IsNullOrEmpty(link.UrlLink) ? "NULL" : link.UrlLink, DbType.String, ParameterDirection.Input);
            p.Add("p_URLCorto", string.IsNullOrEmpty(link.UrlCorto) ? "NULL" : link.UrlCorto, DbType.String, ParameterDirection.Input);
            p.Add("P_MsgError", null, DbType.String, ParameterDirection.Output, 4000);

            await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_InsParamLink", p, commandType: CommandType.StoredProcedure);
            
            var error = p.Get<string>("P_MsgError");
            return string.IsNullOrEmpty(error) || error.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : error;
        }

        public async Task<bool> AplicaPagoPRAsync(PagoRequest pago, string moneda)
        {
            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();
            try
            {
                // Obtener codigo_cliente
                string prSql = "SELECT CODIGO_CLIENTE FROM PR_CREDITOS@\"tc_cbs\" WHERE NO_CREDITO = :numCta";
                var cliente = await conn.ExecuteScalarAsync<string>(prSql, new { numCta = decimal.Parse(pago.NumCta) }, tx);
                if (string.IsNullOrEmpty(cliente))
                    throw new Exception("Error al consultar dato del prestamo: no se encontró código de cliente.");

                // Obtener agencia
                string agSql = "SELECT COD_AGENCIA FROM BO.SCL_PARAM_SISTEMA WHERE ROWNUM = 1";
                var agencia = await conn.ExecuteScalarAsync<string>(agSql, null, tx);

                // Revisar si ya fue pagado
                string checkSql = @"SELECT COUNT(*) FROM BO.SCL_LISTADO_LINKS 
                                    WHERE COD_SKU = :codSku AND NUM_MOVIMIENTO IS NOT NULL";
                var count = await conn.ExecuteScalarAsync<int>(checkSql, new { codSku = pago.CodSku }, tx);
                if (count > 0)
                    throw new Exception("Ya se encuentra pagado el link!!!");

                // Ejecutar pago
                var p = new DynamicParameters();
                p.Add("pCodEmpresa", "1", DbType.String, ParameterDirection.Input);
                p.Add("pCodAgencia", agencia, DbType.String, ParameterDirection.Input);
                p.Add("pCodCliente", decimal.Parse(cliente), DbType.Decimal, ParameterDirection.Input);
                p.Add("pCodMonedaPago", moneda, DbType.String, ParameterDirection.Input);
                p.Add("pQtzMonPago", decimal.Parse(pago.MonPago), DbType.Decimal, ParameterDirection.Input);
                p.Add("p_NumCredito", decimal.Parse(pago.NumCta), DbType.Decimal, ParameterDirection.Input);
                p.Add("p_NumMov", null, DbType.Decimal, ParameterDirection.Output);
                p.Add("pTasaAplicada", null, DbType.Decimal, ParameterDirection.Output);
                p.Add("pMensajeUsuario", "NULL", DbType.String, ParameterDirection.InputOutput, 4000);
                p.Add("pMensajeTecnico", "NULL", DbType.String, ParameterDirection.InputOutput, 4000);

                await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_PagPres", p, transaction: tx, commandType: CommandType.StoredProcedure);

                var msgUsr = p.Get<string>("pMensajeUsuario");
                if (!string.IsNullOrEmpty(msgUsr) && !msgUsr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(msgUsr);

                var numMov = p.Get<decimal?>("p_NumMov");
                var tasa = p.Get<decimal?>("pTasaAplicada") ?? 1;

                // Actualizar link
                string updSql = @"UPDATE BO.SCL_LISTADO_LINKS 
                                  SET NUM_AUTORIZACION = :autVisa, 
                                      NUM_MOVIMIENTO = :numMov, 
                                      VAL_TASA_CAMBIO = :tasa, 
                                      FEC_PAGO_CORE = SYSDATE 
                                  WHERE COD_LINK = :codLink";
                
                await conn.ExecuteAsync(updSql, new { autVisa = pago.AutVisa, numMov, tasa, codLink = decimal.Parse(pago.CodLink) }, tx);
                
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AplicaPagoTCAsync(PagoRequest pago, string moneda)
        {
            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();
            try
            {
                // Revisar si ya fue pagado
                string checkSql = @"SELECT COUNT(*) FROM BO.SCL_LISTADO_LINKS 
                                    WHERE COD_SKU = :codSku AND NUM_MOVIMIENTO IS NOT NULL";
                var count = await conn.ExecuteScalarAsync<int>(checkSql, new { codSku = pago.CodSku }, tx);
                if (count > 0)
                    throw new Exception("Ya se encuentra pagado el link!!!");

                // Ejecutar pago
                var p = new DynamicParameters();
                p.Add("pCodEmpresa", "1", DbType.String, ParameterDirection.Input);
                p.Add("pNumCtaCredito", pago.NumCta, DbType.String, ParameterDirection.Input);
                p.Add("pTarjeta", DBNull.Value, DbType.Decimal, ParameterDirection.Input);
                p.Add("pMoneda", moneda, DbType.String, ParameterDirection.Input);
                p.Add("pQtzMonPago", decimal.Parse(pago.MonPago), DbType.Decimal, ParameterDirection.Input);
                p.Add("pMonAplicado", DBNull.Value, DbType.Decimal, ParameterDirection.InputOutput);
                p.Add("pNumMovPagoEfe", DBNull.Value, DbType.Decimal, ParameterDirection.InputOutput);
                p.Add("pTasaAplicada", null, DbType.Decimal, ParameterDirection.Output);
                p.Add("pMensajeUsuario", null, DbType.String, ParameterDirection.Output, 4000);
                p.Add("pMensajeTecnico", null, DbType.String, ParameterDirection.Output, 4000);

                await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_PagTc", p, transaction: tx, commandType: CommandType.StoredProcedure);

                var msgUsr = p.Get<string>("pMensajeUsuario");
                if (!string.IsNullOrEmpty(msgUsr) && !msgUsr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(msgUsr);

                var numMov = p.Get<decimal?>("pNumMovPagoEfe");
                var tasa = p.Get<decimal?>("pTasaAplicada") ?? 1;

                // Actualizar link
                string updSql = @"UPDATE BO.SCL_LISTADO_LINKS 
                                  SET NUM_AUTORIZACION = :autVisa, 
                                      NUM_MOVIMIENTO = :numMov, 
                                      VAL_TASA_CAMBIO = :tasa, 
                                      FEC_PAGO_CORE = SYSDATE 
                                  WHERE COD_LINK = :codLink";
                
                await conn.ExecuteAsync(updSql, new { autVisa = pago.AutVisa, numMov, tasa, codLink = decimal.Parse(pago.CodLink) }, tx);
                
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<LinkParametroInfo?> GetParametroAsync(string codLink)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT P.NUM_CUENTA AS NumCuenta, P.TIP_CUENTA AS TipCuenta, P.TIP_PAGO AS TipPago, P.MON_COBRO AS MonCobro 
                           FROM BO.SCL_LISTADO_LINKS L 
                           JOIN BO.SCL_PARAMETROS_LINK P ON P.COD_PARAMETRO = L.COD_PARAMETRO 
                           WHERE L.COD_LINK = :codLink";
            return await conn.QueryFirstOrDefaultAsync<LinkParametroInfo>(sql, new { codLink = decimal.Parse(codLink) });
        }

        public async Task<LinkCtaInfo?> GetLinkCtaAsync(string numCta)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COD_PARAMETRO AS CodParametro, 
                                  DIA_MES AS DiaMes, 
                                  CASE 
                                     WHEN DIA_MES < EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'mm/yyyy'), 'dd/mm/yyyy') 
                                     WHEN DIA_MES = EXTRACT(DAY FROM SYSDATE) THEN TRUNC(SYSDATE) 
                                     WHEN DIA_MES > EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(SYSDATE, 'mm/yyyy'), 'dd/mm/yyyy') 
                                  END AS ProximaFecha 
                           FROM BO.SCL_PARAMETROS_LINK 
                           WHERE TIP_LINK = '1' 
                             AND IND_ESTADO = 'A' 
                             AND NUM_CUENTA = :numCta 
                             AND ROWNUM = 1";
            return await conn.QueryFirstOrDefaultAsync<LinkCtaInfo>(sql, new { numCta = decimal.Parse(numCta) });
        }

        public async Task<LinkCtaInfo?> GetLinkParametroAsync(string codParametro)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COD_PARAMETRO AS CodParametro, 
                                  DIA_MES AS DiaMes, 
                                  CASE 
                                     WHEN DIA_MES < EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'mm/yyyy'), 'dd/mm/yyyy') 
                                     WHEN DIA_MES = EXTRACT(DAY FROM SYSDATE) THEN TRUNC(SYSDATE) 
                                     WHEN DIA_MES > EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(SYSDATE, 'mm/yyyy'), 'dd/mm/yyyy') 
                                  END AS ProximaFecha 
                           FROM BO.SCL_PARAMETROS_LINK 
                           WHERE TIP_LINK = '1' 
                             AND IND_ESTADO = 'A' 
                             AND COD_PARAMETRO = :codParametro 
                             AND ROWNUM = 1";
            return await conn.QueryFirstOrDefaultAsync<LinkCtaInfo>(sql, new { codParametro = decimal.Parse(codParametro) });
        }

        public async Task<(IEnumerable<LinkListItem> Items, int TotalCount, int FilteredCount)> GetLinksPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username)
        {
            using var conn = new OracleConnection(connectionString);
            
            // Whitelist ordering column
            var orderColClean = orderCol.ToUpper() switch
            {
                "CORRELATIVO" => "LNK.COD_PARAMETRO",
                "PRODUCTO" => "LNK.NUM_CUENTA",
                "MONTO" => "LNK.MON_COBRO",
                "PAGO" => "LNK.TIP_PAGO",
                "EMISION_LINK" => "LNK.FEC_EMISION",
                "USUARIO" => "LNK.COD_USUARIO",
                "ENVIO" => "LNK.TIP_ENVIO",
                "TIPO_LINK" => "LNK.TIP_LINK",
                _ => "LNK.COD_PARAMETRO"
            };

            var orderDirClean = orderDir.ToUpper() == "DESC" ? "DESC" : "ASC";

            // 1. Total Count
            string countTotalSql = @"SELECT COUNT(*) FROM BO.SCL_PARAMETROS_LINK LNK 
                                     WHERE TRUNC(LNK.FEC_EMISION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3)";
            int totalCount = await conn.ExecuteScalarAsync<int>(countTotalSql);

            // 2. Filtered Count
            string filterSql = "";
            var p = new DynamicParameters();
            p.Add("username", username, DbType.String, ParameterDirection.Input);

            if (!string.IsNullOrWhiteSpace(search))
            {
                filterSql = @" AND (
                    TO_CHAR(LNK.COD_PARAMETRO) LIKE :search OR 
                    TO_CHAR(LNK.NUM_CUENTA) LIKE :search OR 
                    TO_CHAR(LNK.MON_COBRO) LIKE :search OR 
                    TO_CHAR(LNK.FEC_EMISION, 'dd/mm/yyyy') LIKE :search OR 
                    LNK.COD_USUARIO LIKE :search
                )";
                p.Add("search", $"%{search.ToUpper()}%", DbType.String, ParameterDirection.Input);
            }

            string countFilterSql = $@"SELECT COUNT(*) FROM BO.SCL_PARAMETROS_LINK LNK 
                                      WHERE 1 = 1 
                                        AND TRUNC(FEC_EMISION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) 
                                        AND NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), COD_USUARIO) = 
                                            NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), :username)
                                        {filterSql}";
            int filteredCount = await conn.ExecuteScalarAsync<int>(countFilterSql, p);

            // 3. Paged Data
            p.Add("offset", start, DbType.Int32, ParameterDirection.Input);
            p.Add("limit", length == -1 ? filteredCount : length, DbType.Int32, ParameterDirection.Input);

            string dataSql = $@"SELECT 
                                   LNK.COD_PARAMETRO AS Correlativo, 
                                   TO_CHAR(LNK.NUM_CUENTA) AS Producto, 
                                   LNK.MON_COBRO AS Monto, 
                                   DECODE(LNK.TIP_PAGO,'1','Pagar en Dolares','Quetzales') AS Pago, 
                                   TO_CHAR(LNK.FEC_EMISION,'dd/mm/yyyy') AS EmisionLink, 
                                   LNK.COD_USUARIO AS Usuario, 
                                   DECODE(LNK.TIP_ENVIO,'1','SMS','Correo') AS Envio, 
                                   DECODE(LNK.TIP_LINK,'1','Automatico','Manual') AS TipoLink 
                                 FROM BO.SCL_PARAMETROS_LINK LNK 
                                 WHERE 1 = 1  
                                   AND TRUNC(FEC_EMISION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) 
                                   AND NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), COD_USUARIO) = 
                                       NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), :username)
                                   {filterSql}
                                 ORDER BY {orderColClean} {orderDirClean}
                                 OFFSET :offset ROWS FETCH NEXT :limit ROWS ONLY";

            var items = await conn.QueryAsync<LinkListItem>(dataSql, p);
            return (items, totalCount, filteredCount);
        }

        public async Task<(IEnumerable<LinkVerificaItem> Items, int TotalCount, int FilteredCount)> GetLinksVerificaPagedAsync(
            int start, int length, string orderCol, string orderDir, string search, string username)
        {
            using var conn = new OracleConnection(connectionString);

            // Whitelist ordering column
            var orderColClean = orderCol.ToUpper() switch
            {
                "CORRELATIVO" => "LNK.COD_LINK",
                "PRODUCTO" => "PAR.NUM_CUENTA",
                "CODIGO_VISA" => "LNK.COD_SKU",
                "NUM_AUTO" => "LNK.NUM_AUTORIZACION",
                "NUM_MOV" => "LNK.NUM_MOVIMIENTO",
                _ => "LNK.COD_LINK"
            };

            var orderDirClean = orderDir.ToUpper() == "DESC" ? "DESC" : "ASC";

            // 1. Total Count
            string countTotalSql = @"SELECT COUNT(*) FROM BO.SCL_LISTADO_LINKS LNK 
                                     INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO
                                     WHERE TRUNC(LNK.FEC_ADICION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3)";
            int totalCount = await conn.ExecuteScalarAsync<int>(countTotalSql);

            // 2. Filtered Count
            string filterSql = "";
            var p = new DynamicParameters();
            p.Add("username", username, DbType.String, ParameterDirection.Input);

            if (!string.IsNullOrWhiteSpace(search))
            {
                filterSql = @" AND (
                    TO_CHAR(LNK.COD_PARAMETRO) LIKE :search OR 
                    TO_CHAR(PAR.NUM_CUENTA) LIKE :search OR 
                    LNK.COD_SKU LIKE :search
                )";
                p.Add("search", $"%{search.ToUpper()}%", DbType.String, ParameterDirection.Input);
            }

            string countFilterSql = $@"SELECT COUNT(*) FROM BO.SCL_LISTADO_LINKS LNK 
                                      INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO
                                      WHERE 1 = 1 
                                        AND TRUNC(LNK.FEC_ADICION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) 
                                        AND NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), PAR.COD_USUARIO) = 
                                            NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), :username)
                                        {filterSql}";
            int filteredCount = await conn.ExecuteScalarAsync<int>(countFilterSql, p);

            // 3. Paged Data
            p.Add("offset", start, DbType.Int32, ParameterDirection.Input);
            p.Add("limit", length == -1 ? filteredCount : length, DbType.Int32, ParameterDirection.Input);

            string dataSql = $@"SELECT 
                                   TO_CHAR(LNK.COD_LINK) AS Correlativo, 
                                   TO_CHAR(PAR.NUM_CUENTA) AS Producto, 
                                   LNK.COD_SKU AS CodigoVisa, 
                                   COALESCE(LNK.NUM_AUTORIZACION,'Pendiente') AS NumAuto, 
                                   COALESCE(LNK.NUM_MOVIMIENTO,'Pendiente') AS NumMov,
                                   DECODE(LNK.NUM_AUTORIZACION, NULL, 'Consulta/Pago', 'Pagado') AS Edit
                                 FROM BO.SCL_LISTADO_LINKS LNK 
                                 INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO
                                 WHERE 1 = 1  
                                   AND TRUNC(LNK.FEC_ADICION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) 
                                   AND NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), PAR.COD_USUARIO) = 
                                       NVL((SELECT USUARIO FROM GT_RRHH.RRHH_USUARIO_ROL WHERE ROL = 1330 AND USUARIO = :username), :username)
                                   {filterSql}
                                 ORDER BY {orderColClean} {orderDirClean}
                                 OFFSET :offset ROWS FETCH NEXT :limit ROWS ONLY";

            var items = await conn.QueryAsync<LinkVerificaItem>(dataSql, p);
            return (items, totalCount, filteredCount);
        }

        public async Task<string?> NotificaSMSAsync(SmsRequest sms)
        {
            using var conn = new OracleConnection(connectionString);
            var p = new DynamicParameters();
            p.Add("p_NumCtaCredito", decimal.Parse(sms.NumCta), DbType.Decimal, ParameterDirection.Input);
            p.Add("p_NumTelefono", decimal.Parse(sms.Telefono), DbType.Decimal, ParameterDirection.Input);
            p.Add("P_SmsMensaje", sms.Mensaje, DbType.String, ParameterDirection.Input);
            p.Add("P_MsgError", null, DbType.String, ParameterDirection.Output, 4000);

            await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_SmsEnviar", p, commandType: CommandType.StoredProcedure);
            var error = p.Get<string>("P_MsgError");
            return string.IsNullOrEmpty(error) || error.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : error;
        }

        public async Task<string?> NotificaMailAsync(MailRequest mail)
        {
            using var conn = new OracleConnection(connectionString);
            var p = new DynamicParameters();
            p.Add("p_EMail", mail.Mail, DbType.String, ParameterDirection.Input);
            p.Add("p_Asunto", mail.Asunto, DbType.String, ParameterDirection.Input);
            p.Add("p_DesBody", mail.Link, DbType.String, ParameterDirection.Input);
            p.Add("P_MsgError", null, DbType.String, ParameterDirection.Output, 4000);

            await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_SndMail", p, commandType: CommandType.StoredProcedure);
            var error = p.Get<string>("P_MsgError");
            return string.IsNullOrEmpty(error) || error.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : error;
        }

        public async Task<bool> UpdateEstadoLinkAsync(string codParametro)
        {
            using var conn = new OracleConnection(connectionString);
            var p = new DynamicParameters();
            p.Add("p_CodParametro", decimal.Parse(codParametro), DbType.Decimal, ParameterDirection.Input);
            p.Add("p_IndEstado", "I", DbType.String, ParameterDirection.Input);
            p.Add("P_MsgError", null, DbType.String, ParameterDirection.InputOutput, 4000);

            await conn.ExecuteAsync("BO.PKG_SCL.PkgScl_UpdParamLinkEst", p, commandType: CommandType.StoredProcedure);
            var error = p.Get<string>("P_MsgError");
            if (!string.IsNullOrEmpty(error) && !error.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                throw new Exception(error);

            return true;
        }

        public async Task<IEnumerable<ShortLinkItem>> ObtieneLinksAsync(int limit)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COD_CONSECUTIVO AS CodConsecutivo, LONG_LINK AS LongLink 
                           FROM BO.CLI_SHORT_LINKS 
                           WHERE IND_INI_PROCESO = 0 
                             AND SHORT_LINK IS NULL 
                             AND ROWNUM <= :limit";
            return await conn.QueryAsync<ShortLinkItem>(sql, new { limit });
        }

        public async Task<int> ExistenPendientesAsync()
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COUNT(1) FROM BO.CLI_SHORT_LINKS 
                           WHERE IND_INI_PROCESO = 0 AND SHORT_LINK IS NULL";
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task<bool> ExistePerifericoAsync(int codPeriferico)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = "SELECT COUNT(1) FROM BO.PPL_LISTADO_PERIFERICO WHERE COD_PERIFERICO = :codPeriferico";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { codPeriferico });
            return count > 0;
        }

        public async Task<bool> UpdateURLCortoAsync(decimal numConsecutivo, string urlCorto)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"UPDATE BO.CLI_SHORT_LINKS 
                           SET SHORT_LINK = :urlCorto,
                               IND_INI_PROCESO = 1,
                               IND_FIN_PROCESO = 1,
                               FEC_RESPUESTA = SYSDATE 
                           WHERE COD_CONSECUTIVO = :numConsecutivo";
            var result = await conn.ExecuteAsync(sql, new { urlCorto, numConsecutivo });
            return result > 0;
        }

        public async Task<bool> RegistraBitacoraBDAsync(string urlLargo, string urlCorto, int codPeriferico)
        {
            using var conn = new OracleConnection(connectionString);
            string detail = $"Se creo link corto ({urlCorto}) asociado al URL largo ({urlLargo})";
            string sql = @"INSERT INTO BO.PPL_BITACORA_WEBSERVICE (COD_PERIFERICO, DET_BITACORA) 
                           VALUES (:codPeriferico, :detail)";
            var result = await conn.ExecuteAsync(sql, new { codPeriferico, detail });
            return result > 0;
        }
    }
}
