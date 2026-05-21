using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;

namespace Backend.Repositories
{
    public class CarrierRepository(string connectionString) : ICarrierRepository
    {
        private static byte[] GenerateSalt()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return randomNumber;
        }

        private static byte[] PBKDF2Sha256GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            using var hmac = new HMACSHA256(password);
            int hashLength = hmac.HashSize / 8;
            if ((hmac.HashSize & 7) != 0)
                hashLength++;
            int keyLength = dklen / hashLength;
            if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                throw new ArgumentOutOfRangeException(nameof(dklen));
            if (dklen % hashLength != 0)
                keyLength++;
            byte[] extendedkey = new byte[salt.Length + 4];
            Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
            using var ms = new MemoryStream();
            
            for (int i = 0; i < keyLength; i++)
            {
                extendedkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);
                byte[] u = hmac.ComputeHash(extendedkey);
                Array.Clear(extendedkey, salt.Length, 4);
                byte[] f = u;
                for (int j = 1; j < iterationCount; j++)
                {
                    u = hmac.ComputeHash(u);
                    for (int k = 0; k < f.Length; k++)
                    {
                        f[k] ^= u[k];
                    }
                }
                ms.Write(f, 0, f.Length);
                Array.Clear(u, 0, u.Length);
                Array.Clear(f, 0, f.Length);
            }
            byte[] dk = new byte[dklen];
            ms.Position = 0;
            ms.Read(dk, 0, dklen);
            ms.Position = 0;
            for (long i = 0; i < ms.Length; i++)
            {
                ms.WriteByte(0);
            }
            Array.Clear(extendedkey, 0, extendedkey.Length);
            return dk;
        }

        public async Task<bool> InsertUsuarioAsync(CarrierModel carrier, string username)
        {
            using var conn = new OracleConnection(connectionString);

            byte[] salt = GenerateSalt();
            byte[] hashedPassword = PBKDF2Sha256GetBytes(32, Encoding.UTF8.GetBytes(carrier.Clave), salt, 5000);

            // Legacy insert uses UTF8.GetString(hashedPassword) which was an interesting design choice,
            // but we must preserve it exactly in case downstream systems read it like that.
            string hashedStr = Encoding.UTF8.GetString(hashedPassword);
            string saltStr = Encoding.UTF8.GetString(salt);

            string sql = @"INSERT INTO BO.SWS_TOKEN_CONTROL (
                               COD_SISTEMA, COD_USUARIO, CLAVE, IND_ACTIVO, USU_INGRESO, FEC_INGRESO, SALT
                           ) VALUES (
                               189, :codTranspo, :clave, 'A', :username, SYSDATE, :salt
                           )";

            var affected = await conn.ExecuteAsync(sql, new
            {
                codTranspo = carrier.CodTranspo,
                clave = hashedStr,
                username,
                salt = saltStr
            });

            return affected > 0;
        }

        public async Task<bool> UpdateUsuarioAsync(CarrierModel carrier, string username)
        {
            using var conn = new OracleConnection(connectionString);
            string sql;
            
            if (!string.IsNullOrEmpty(carrier.Clave))
            {
                byte[] salt = GenerateSalt();
                byte[] hashedPassword = PBKDF2Sha256GetBytes(32, Encoding.UTF8.GetBytes(carrier.Clave), salt, 5000);

                // Note: Legacy update used Base64String, distinct from insert which used UTF8.GetString.
                // We preserve this exact behavior as specified.
                string hashedStr = Convert.ToBase64String(hashedPassword);
                string saltStr = Convert.ToBase64String(salt);

                sql = @"UPDATE BO.SWS_TOKEN_CONTROL SET 
                            CLAVE = :clave,
                            IND_ACTIVO = :indEstado,
                            SALT = :salt,
                            USU_ACTUALIZO = :username,
                            FEC_ACTUALIZO = SYSDATE 
                        WHERE COD_USUARIO = :codTranspo";

                var affected = await conn.ExecuteAsync(sql, new
                {
                    clave = hashedStr,
                    indEstado = carrier.IndEstado,
                    salt = saltStr,
                    username,
                    codTranspo = carrier.CodTranspo
                });
                return affected > 0;
            }
            else
            {
                sql = @"UPDATE BO.SWS_TOKEN_CONTROL SET 
                            IND_ACTIVO = :indEstado,
                            USU_ACTUALIZO = :username,
                            FEC_ACTUALIZO = SYSDATE 
                        WHERE COD_USUARIO = :codTranspo";

                var affected = await conn.ExecuteAsync(sql, new
                {
                    indEstado = carrier.IndEstado,
                    username,
                    codTranspo = carrier.CodTranspo
                });
                return affected > 0;
            }
        }

        public async Task<bool> InsertTransportadoraAsync(CarrierModel carrier)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"INSERT INTO BO.ACI_TRANSPORTADORA (
                               COD_TRANSPO, NOM_TRANSPO, DIRECCION, TELEFONO, NOM_ENCARGADO
                           ) VALUES (
                               :CodTranspo, :NomTranspo, :Direccion, :Telefono, :NomEncargado
                           )";

            var affected = await conn.ExecuteAsync(sql, carrier);
            return affected > 0;
        }

        public async Task<bool> UpdateTransportadoraAsync(CarrierModel carrier)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"UPDATE BO.ACI_TRANSPORTADORA SET
                               NOM_TRANSPO = :NomTranspo, 
                               DIRECCION = :Direccion, 
                               TELEFONO = :Telefono, 
                               NOM_ENCARGADO = :NomEncargado 
                           WHERE COD_TRANSPO = :CodTranspo";

            var affected = await conn.ExecuteAsync(sql, carrier);
            return affected > 0;
        }

        public async Task<CarrierModel?> GetTransportadoraAsync(string usuario)
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT TRA.COD_TRANSPO AS CodTranspo, 
                                  TRA.NOM_TRANSPO AS NomTranspo, 
                                  TRA.DIRECCION AS Direccion, 
                                  TRA.TELEFONO AS Telefono, 
                                  TRA.NOM_ENCARGADO AS NomEncargado, 
                                  SWS.IND_ACTIVO AS IndEstado
                           FROM BO.ACI_TRANSPORTADORA TRA
                           LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO = SWS.COD_USUARIO 
                           WHERE TRA.COD_TRANSPO = :usuario";

            return await conn.QueryFirstOrDefaultAsync<CarrierModel>(sql, new { usuario });
        }

        public async Task<IEnumerable<CarrierModel>> GetTransportadorasAsync()
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT TRA.COD_TRANSPO AS CodTranspo, 
                                  TRA.NOM_TRANSPO AS NomTranspo, 
                                  TRA.DIRECCION AS Direccion, 
                                  TRA.TELEFONO AS Telefono, 
                                  TRA.NOM_ENCARGADO AS NomEncargado, 
                                  DECODE(SWS.IND_ACTIVO, 'A', 'Activo', 'Inactivo') AS IndEstado
                           FROM BO.ACI_TRANSPORTADORA TRA
                           LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO = SWS.COD_USUARIO 
                           ORDER BY TRA.NOM_TRANSPO ASC";

            return await conn.QueryAsync<CarrierModel>(sql);
        }

        public async Task<IEnumerable<CarrierDropdownItem>> GetTransportadorasDLLAsync(string codCliAci = "")
        {
            using var conn = new OracleConnection(connectionString);
            string sql = @"SELECT TRA.COD_TRANSPO AS CodTranspo, TRA.NOM_TRANSPO AS NomTranspo
                           FROM BO.ACI_TRANSPORTADORA TRA 
                           LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO = SWS.COD_USUARIO 
                           WHERE SWS.IND_ACTIVO = 'A'";

            if (!string.IsNullOrEmpty(codCliAci))
            {
                sql += @" UNION 
                          SELECT TRA.COD_TRANSPO AS CodTranspo, 
                                 TRA.NOM_TRANSPO || ' (' || DECODE(SWS.IND_ACTIVO, 'A', 'ACTIVO', 'INACTIVO') || ')' AS NomTranspo 
                          FROM BO.ACI_TRANSPORTADORA TRA 
                          LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_TRANSPO = CLI.COD_TRANSPO 
                          LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO = SWS.COD_USUARIO 
                          WHERE CLI.COD_CLI_ACI = :codCliAci 
                            AND SWS.IND_ACTIVO = 'I'";
            }

            return await conn.QueryAsync<CarrierDropdownItem>(sql, new { codCliAci });
        }
    }
}
