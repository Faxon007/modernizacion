using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Database
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddEncryptedDatabaseConnections(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseConnectionProvider, DatabaseConnectionProvider>();
            return services;
        }
    }
}
