using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;

namespace Backend.Repositories
{
    public class MenuRepository(string connectionString) : IMenuRepository
    {
        public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(string username, string systemCode)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT DISTINCT m.cod_menu_item AS CodMenuItem, 
                                           m.nombre AS Nombre, 
                                           m.path AS Path, 
                                           m.descripcion AS Descripcion, 
                                           m.cod_item_padre AS CodItemPadre, 
                                           m.visible AS Visible  
                           FROM rrhh_menu_item m
                           JOIN rrhh_permiso_item pe ON pe.COD_MENU_ITEM = m.cod_menu_item
                           JOIN rrhh_usuario_rol ur ON ur.rol = pe.rol  
                           WHERE TRIM(UPPER(ur.USUARIO)) = TRIM(UPPER(:username))
                             AND m.cod_menu_item <> 0  
                             AND m.sistema = :systemCode
                           ORDER BY m.nombre ASC";

            return await conn.QueryAsync<MenuItem>(sql, new { username = username.Trim(), systemCode = int.Parse(systemCode) });
        }

        public async Task<string?> ValidateRRHHAsync(string username)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = "SELECT activo FROM RRHH_USUARIO WHERE TRIM(UPPER(USUARIO)) = TRIM(UPPER(:username))";
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { username = username.Trim() });
        }

        public async Task<string?> ValidatePAAsync(string username)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = "SELECT est_activo FROM USUARIOS WHERE TRIM(UPPER(cod_usuario)) = TRIM(UPPER(:username))";
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { username = username.Trim() });
        }

        public async Task<IEnumerable<UserRoleInfo>> VerificarRolAsync(string username, string systemCode)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT RU.USUARIO, RUR.ROL, RPI.COD_MENU_ITEM, '' AS ACCION, RO.SISTEMA  
                           FROM RRHH_USUARIO RU 
                           LEFT JOIN RRHH_USUARIO_ROL RUR ON TRIM(UPPER(RUR.USUARIO)) = TRIM(UPPER(RU.USUARIO)) 
                           LEFT JOIN RRHH_ROL RO ON RO.ROL = RUR.ROL 
                           LEFT JOIN RRHH_PERMISO_ITEM RPI ON RPI.ROL = RUR.ROL 
                           WHERE RO.SISTEMA = :systemCode
                             AND TRIM(UPPER(RU.USUARIO)) = TRIM(UPPER(:username))";

            return await conn.QueryAsync<UserRoleInfo>(sql, new { username = username.Trim(), systemCode = int.Parse(systemCode) });
        }
    }
}
