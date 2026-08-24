using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Infrastructure.Database
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddEncryptedDatabaseConnections(this IServiceCollection services)
        {
            // Habilitar el enlace por nombre globalmente para Oracle
            OracleConfiguration.BindByName = true;

            services.AddSingleton<IDatabaseConnectionProvider, DatabaseConnectionProvider>();
            return services;
        }
    }
}
