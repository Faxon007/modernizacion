using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Repositories
{
    public class ProductRepository(string connectionString) : IProductRepository
    {
        public async Task<decimal?> GetMontoPRAsync(string numCuenta)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = $"SELECT ROUND(MON_CANCELACION * MON_TASA, 2) AS VALOR FROM TABLE(BO.PKG_SCL.PkgScl_GetInfoPR(:numCuenta))";
            try
            {
                return await conn.QueryFirstOrDefaultAsync<decimal?>(sql, new { numCuenta = decimal.Parse(numCuenta) });
            }
            catch
            {
                // Si falla por sintaxis de table-function de Oracle, intentamos la llamada original
                string sqlFallback = $"SELECT ROUND(MON_CANCELACION * MON_TASA, 2) AS VALOR FROM BO.PKG_SCL.PkgScl_GetInfoPR(:numCuenta)";
                return await conn.QueryFirstOrDefaultAsync<decimal?>(sqlFallback, new { numCuenta = decimal.Parse(numCuenta) });
            }
        }

        public async Task<decimal?> GetMontoTCAsync(string numCuenta)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT 
                             (NVL(MON_DEUDA_QTZ,0) + NVL(MON_EXTRAF_QTZ,0) + NVL(MON_RESERPRIN_QTZ,0) + NVL(MON_RESERINT_QTZ,0) + NVL(MON_OTROS_QTZ,0)) 
                             + ROUND((NVL(MON_DEUDA_USD,0) + NVL(MON_EXTRAF_USD,0) + NVL(MON_RESERPRIN_USD,0) + NVL(MON_RESERINT_USD,0) + NVL(MON_OTROS_USD,0)) * MON_TASA, 2) 
                             AS VALOR 
                           FROM TABLE(BO.PKG_SCL.PkgScl_GetInfoTC(:numCuenta))";
            try
            {
                return await conn.QueryFirstOrDefaultAsync<decimal?>(sql, new { numCuenta = decimal.Parse(numCuenta) });
            }
            catch
            {
                string sqlFallback = @"SELECT 
                                         (NVL(MON_DEUDA_QTZ,0) + NVL(MON_EXTRAF_QTZ,0) + NVL(MON_RESERPRIN_QTZ,0) + NVL(MON_RESERINT_QTZ,0) + NVL(MON_OTROS_QTZ,0)) 
                                         + ROUND((NVL(MON_DEUDA_USD,0) + NVL(MON_EXTRAF_USD,0) + NVL(MON_RESERPRIN_USD,0) + NVL(MON_RESERINT_USD,0) + NVL(MON_OTROS_USD,0)) * MON_TASA, 2) 
                                         AS VALOR 
                                       FROM BO.PKG_SCL.PkgScl_GetInfoTC(:numCuenta)";
                return await conn.QueryFirstOrDefaultAsync<decimal?>(sqlFallback, new { numCuenta = decimal.Parse(numCuenta) });
            }
        }

        public async Task<bool> ExisteCuentaAsync(string numCta)
        {
            using var conn = new OracleConnection(connectionString);
            string sql1 = @"SELECT COUNT(*) FROM pr_creditos@""tc_cbs"" c 
                            WHERE c.estado IN ('D','I','V') AND c.no_credito = :numCta AND ROWNUM = 1";

            var count = await conn.ExecuteScalarAsync<int>(sql1, new { numCta });
            if (count > 0) return true;

            string sql2 = @"SELECT COUNT(*) FROM tc_cuenta_credito c 
                            JOIN tc_prod_emis_tjt p ON p.cod_emisor = c.cod_emisor
                                                  AND p.cod_prod_emisor = c.cod_prod_emisor
                                                  AND p.cod_empresa = c.cod_empresa
                                                  AND p.cod_marca NOT LIKE '%DEB%'
                            WHERE c.ind_estado_cta IN ('A', 'P', 'V') AND c.num_cta_credito = :numCta AND ROWNUM = 1";

            count = await conn.ExecuteScalarAsync<int>(sql2, new { numCta });
            return count > 0;
        }

        public async Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COUNT(*) FROM BO.BO_LISTA_EXCLUSION_PAGOSTD blep 
                           WHERE blep.COD_PERSONA = :codCliente AND blep.IND_ESTADO = 'A'";

            var count = await conn.ExecuteScalarAsync<int>(sql, new { codCliente });
            return count > 0;
        }
    }
}
