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
            
            var encPath = config["Database:EncryptedConfigPath"]
                ?? throw new InvalidOperationException("Falta 'Database:EncryptedConfigPath' en la configuración.");

            if (!File.Exists(encPath))
                throw new FileNotFoundException($"Archivo .cef2 no encontrado: '{encPath}'. ");
            
            var privateKeyPem = ResolvePrivateKey(config);
            
            var encryptedBase64 = File.ReadAllText(encPath).Trim();
            DatabaseConfig dbConfig;

            try
            {
                dbConfig = DatabaseConfigCrypto.Decrypt(encryptedBase64, privateKeyPem);
            }
            catch (Exception ex)
            {
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
        
        private static string ResolvePrivateKey(IConfiguration config)
        {
            var keyPath = config["Database:PrivateKeyEnvVar"] ?? throw new InvalidOperationException("Se configuró Database:PrivateKeyEnvVar");

            if (!File.Exists(keyPath))
                throw new FileNotFoundException($"Clave privada RSA no encontrada en: '{keyPath}'.", keyPath);

            return File.ReadAllText(keyPath).Trim();
        }
    }
}
