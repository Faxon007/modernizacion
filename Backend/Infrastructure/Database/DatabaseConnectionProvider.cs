using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Database
{
    public interface IDatabaseConnectionProvider
    {
        string? GetConnectionString(DatabaseKey key);
        bool IsAvailable(DatabaseKey key);
        IReadOnlyList<(DatabaseKey Key, string Alias)> GetAvailableConnections();
    }

    public class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly Dictionary<DatabaseKey, (string ConnectionString, string Alias)> _connections = new();
        private readonly ILogger<DatabaseConnectionProvider> _logger;

        public DatabaseConnectionProvider(
            IConfiguration config,
            ILogger<DatabaseConnectionProvider> logger)
        {
            _logger = logger;
            
            _logger.LogInformation("Iniciando DatabaseConnectionProvider. Leyendo configuración de BD...");

            var encPath = config["Database:EncryptedConfigPath"];
            _logger.LogInformation("Ruta de archivo .cef2 obtenida: {EncPath}", encPath ?? "NULO");

            if (string.IsNullOrWhiteSpace(encPath))
                throw new InvalidOperationException("Falta 'Database:EncryptedConfigPath' en la configuración.");

            if (!File.Exists(encPath))
            {
                _logger.LogError("Archivo .cef2 no encontrado en la ruta: '{EncPath}'.", encPath);
                throw new FileNotFoundException($"Archivo .cef2 no encontrado: '{encPath}'. ");
            }
            
            _logger.LogInformation("Archivo .cef2 encontrado. Resolviendo llave privada...");
            var privateKeyPem = ResolvePrivateKey(config, _logger);
            
            _logger.LogInformation("Leyendo contenido del archivo .cef2 ({EncPath})...", encPath);
            var encryptedBase64 = File.ReadAllText(encPath).Trim();
            DatabaseConfig dbConfig;

            try
            {
                _logger.LogInformation("Intentando desencriptar el archivo .cef2 con la llave privada...");
                dbConfig = DatabaseConfigCrypto.Decrypt(encryptedBase64, privateKeyPem);
                _logger.LogInformation("Desencriptación exitosa del archivo .cef2.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo desencriptar el archivo .cef2.");
                throw new InvalidOperationException("No se pudo desencriptar el archivo .cef2. ", ex);
            }
            
            // Oracle
            var oracleList = dbConfig.Oracle ?? [];
            for (int i = 0; i < oracleList.Count; i++)
            {
                var opt = oracleList[i];
                if (string.IsNullOrWhiteSpace(opt.TnsName))
                    continue;

                var key = new DatabaseKey(DatabaseEngine.Oracle, i);
                _connections[key] = (opt.BuildConnectionString(), opt.Alias);
                _logger.LogInformation("Conexión Oracle[{Index}] ('{Alias}') cargada.", i, opt.Alias);
            }

            // SQL Server
            var sqlList = dbConfig.SqlServer ?? [];
            for (int i = 0; i < sqlList.Count; i++)
            {
                var opt = sqlList[i];
                if (string.IsNullOrWhiteSpace(opt.Host))
                    continue;

                var key = new DatabaseKey(DatabaseEngine.SqlServer, i);
                _connections[key] = (opt.BuildConnectionString(), opt.Alias);
                _logger.LogInformation("Conexión SqlServer[{Index}] ('{Alias}') cargada.", i, opt.Alias);
            }

            if (_connections.Count == 0)
                _logger.LogWarning("El archivo .cef2 no contiene ninguna conexión configurada. ");
            else
                _logger.LogInformation("DatabaseConnectionProvider listo: {Count} conexión(es) disponible(s).", _connections.Count);
        }

        public string? GetConnectionString(DatabaseKey key) =>
            _connections.TryGetValue(key, out var entry) ? entry.ConnectionString : null;

        public bool IsAvailable(DatabaseKey key) =>
            _connections.ContainsKey(key);

        public IReadOnlyList<(DatabaseKey Key, string Alias)> GetAvailableConnections() =>
            _connections
                .Select(kv => (kv.Key, kv.Value.Alias))
                .ToList()
                .AsReadOnly();
        
        private static string ResolvePrivateKey(IConfiguration config, ILogger logger)
        {
            var keyPath = config["Database:PrivateKeyEnvVar"];
            logger.LogInformation("Ruta de llave privada obtenida de 'Database:PrivateKeyEnvVar': {KeyPath}", keyPath ?? "NULO");

            if (string.IsNullOrWhiteSpace(keyPath))
            {
                logger.LogError("Falta 'Database:PrivateKeyEnvVar' en la configuración.");
                throw new InvalidOperationException("Falta configurar Database:PrivateKeyEnvVar");
            }

            if (!File.Exists(keyPath))
            {
                logger.LogError("Clave privada RSA no encontrada en la ruta: '{KeyPath}'.", keyPath);
                throw new FileNotFoundException($"Clave privada RSA no encontrada en: '{keyPath}'.", keyPath);
            }

            logger.LogInformation("Clave privada RSA encontrada en '{KeyPath}'. Leyendo contenido...", keyPath);
            var content = File.ReadAllText(keyPath).Trim();
            logger.LogInformation("Contenido de clave privada leído exitosamente (longitud: {Length}).", content.Length);

            return content;
        }
    }
}
