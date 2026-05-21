using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;

namespace Backend.Repositories
{
    public class ClientRepository(string connectionString) : IClientRepository
    {
        public async Task<ClientEntity?> GetClienteCtaAsync(string numCta)
        {
            using var conn = new OracleConnection(connectionString);

            string sql1 = @"SELECT c.codigo_cliente AS CodCliente, p.nombre AS NomCliente 
                            FROM pr_creditos@""tc_cbs"" c 
                            JOIN personas p ON p.cod_persona = c.codigo_cliente
                            WHERE c.estado IN ('D','I','V') AND c.no_credito = :numCta AND ROWNUM = 1";

            var client = await conn.QueryFirstOrDefaultAsync<ClientEntity>(sql1, new { numCta });
            if (client != null) return client;

            string sql2 = @"SELECT c.COD_CLIENTE AS CodCliente, pe.nombre AS NomCliente 
                            FROM tc_cuenta_credito c 
                            JOIN tc_prod_emis_tjt p ON p.cod_emisor = c.cod_emisor
                                                  AND p.cod_prod_emisor = c.cod_prod_emisor
                                                  AND p.cod_empresa = c.cod_empresa
                                                  AND p.cod_marca NOT LIKE '%DEB%'
                            JOIN personas pe ON pe.cod_persona = c.cod_cliente
                            WHERE c.ind_estado_cta IN ('A', 'P', 'V') AND c.num_cta_credito = :numCta AND ROWNUM = 1";

            return await conn.QueryFirstOrDefaultAsync<ClientEntity>(sql2, new { numCta });
        }

        public async Task<PrestamoInfo?> GetTipoPrestamoAsync(string numCta)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT p.no_credito AS NumCuenta, T.CODIGO_MONEDA AS Moneda 
                           FROM pr_creditos@""tc_cbs"" P 
                           JOIN PR_TIPO_CREDITO T ON T.CODIGO_EMPRESA = P.CODIGO_EMPRESA 
                                                 AND T.TIPO_CREDITO = P.TIPO_CREDITO
                           WHERE P.estado IN ('D','I','V') AND P.NO_CREDITO = :numCta";

            return await conn.QueryFirstOrDefaultAsync<PrestamoInfo>(sql, new { numCta });
        }

        public async Task<bool> IsClienteListaNegraAsync(string codEmpresa, string codCliente)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT COUNT(*) 
                           FROM BO.BO_LISTA_EXCLUSION_PAGOSTD blep 
                           WHERE blep.COD_PERSONA = :codCliente AND blep.IND_ESTADO = 'A'";

            var count = await conn.ExecuteScalarAsync<int>(sql, new { codCliente });
            return count > 0;
        }

        public async Task<string?> GetCorreoClienteAsync(string codCliente)
        {
            using var conn = new OracleConnection(connectionString);
            string sql1 = @"SELECT EMAIL_USUARIO || '@' || EMAIL_SERVIDOR AS CORREO
                            FROM EMAIL_PERSONAS 
                            WHERE ES_DEFAULT='S' AND COD_PERSONA = :codCliente";

            var email = await conn.QueryFirstOrDefaultAsync<string>(sql1, new { codCliente });
            if (!string.IsNullOrEmpty(email)) return email;

            string sql2 = @"SELECT EMAIL_USUARIO || '@' || EMAIL_SERVIDOR AS CORREO
                            FROM EMAIL_PERSONAS 
                            WHERE COD_PERSONA = :codCliente AND ROWNUM = 1";

            return await conn.QueryFirstOrDefaultAsync<string>(sql2, new { codCliente });
        }

        public async Task<string?> GetTelefonoClienteAsync(string codCliente)
        {
            using var conn = new OracleConnection(connectionString);
            string sql1 = @"SELECT TRIM(NUM_TELEFONO) 
                            FROM TEL_PERSONAS 
                            WHERE ES_DEFAULT ='S' AND COD_PERSONA = :codCliente";

            var phone = await conn.QueryFirstOrDefaultAsync<string>(sql1, new { codCliente });
            if (!string.IsNullOrEmpty(phone)) return phone;

            string sql2 = @"SELECT TRIM(NUM_TELEFONO) 
                            FROM TEL_PERSONAS 
                            WHERE COD_PERSONA = :codCliente AND ROWNUM = 1";

            return await conn.QueryFirstOrDefaultAsync<string>(sql2, new { codCliente });
        }

        public async Task<IEnumerable<CuentaInfo>> GetCuentasAsync(string codCliente)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT p.no_credito AS NumCuenta, 
                                  DECODE(p.estado, 'D', 'Vigente', 'I', 'Castigado', 'V', 'Vencido') AS Estado, 
                                  'Prestamo' AS Tipo  
                           FROM pr_creditos@""tc_cbs"" P 
                           JOIN PR_TIPO_CREDITO T ON T.CODIGO_EMPRESA = P.CODIGO_EMPRESA 
                                                 AND T.TIPO_CREDITO = P.TIPO_CREDITO
                           WHERE P.estado IN ('D','I','V') AND P.codigo_cliente = :codCliente
                           UNION 
                           SELECT c.num_cta_credito AS NumCuenta,
                                  DECODE(c.ind_estado_cta, 'A', 'Activa', 'P', 'Proceso de Cancelacion', 'V', 'Vencida') AS Estado,
                                  'Tarjeta' AS Tipo 
                           FROM tc_cuenta_credito c
                           JOIN tc_prod_emis_tjt p ON p.cod_emisor = c.cod_emisor
                                                 AND p.cod_prod_emisor = c.cod_prod_emisor
                                                 AND p.cod_empresa = c.cod_empresa
                                                 AND p.cod_marca NOT LIKE '%DEB%'
                           WHERE c.ind_estado_cta IN ('A', 'P', 'V')
                             AND c.COD_GRUPO_CTACTE NOT IN (830,837) 
                             AND c.cod_cliente = :codCliente";

            return await conn.QueryAsync<CuentaInfo>(sql, new { codCliente });
        }
    }
}
