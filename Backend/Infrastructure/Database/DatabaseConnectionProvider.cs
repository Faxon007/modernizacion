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
        DatabaseKey DefaultKey { get; }
        string? GetConnectionString(DatabaseKey key);
        bool IsAvailable(DatabaseKey key);
        IReadOnlyList<(DatabaseKey Key, string Alias)> GetAvailableConnections();
    }

    public class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly Dictionary<DatabaseKey, (string ConnectionString, string Alias)> _connections = new();
        private readonly ILogger<DatabaseConnectionProvider> _logger;
        public DatabaseKey DefaultKey { get; private set; }

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
            
            var defaultProviderStr = config["Database:DefaultProvider"] ?? "Oracle";
            DefaultKey = defaultProviderStr.Equals("SQL", StringComparison.OrdinalIgnoreCase) ? DatabaseKey.SQL : DatabaseKey.Oracle;

            var oracleAlias = config["Database:Oracle:Alias"] ?? "desarrollo";
            var sqlAlias = config["Database:SQL:Alias"] ?? "sql_desarrollo";

            // Oracle
            var oracleList = dbConfig.Oracle ?? [];
            foreach (var opt in oracleList)
            {
                if (string.IsNullOrWhiteSpace(opt.TnsName))
                    continue;

                if (string.Equals(opt.Alias, oracleAlias, StringComparison.OrdinalIgnoreCase))
                {
                    _connections[DatabaseKey.Oracle] = (opt.BuildConnectionString(), opt.Alias);
                    _logger.LogInformation("Conexión Oracle ('{Alias}') cargada.", opt.Alias);
                }
            }

            // SQL Server
            var sqlList = dbConfig.SqlServer ?? [];
            foreach (var opt in sqlList)
            {
                if (string.IsNullOrWhiteSpace(opt.Host))
                    continue;

                if (string.Equals(opt.Alias, sqlAlias, StringComparison.OrdinalIgnoreCase))
                {
                    _connections[DatabaseKey.SQL] = (opt.BuildConnectionString(), opt.Alias);
                    _logger.LogInformation("Conexión SqlServer ('{Alias}') cargada.", opt.Alias);
                }
            }

            if (_connections.Count == 0)
                _logger.LogWarning("El archivo .cef2 no contiene ninguna conexión configurada que coincida con los alias.");
            else
                _logger.LogInformation("DatabaseConnectionProvider listo: {Count} conexión(es) disponible(s). Default: {DefaultKey}", _connections.Count, DefaultKey);
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
