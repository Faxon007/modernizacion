using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Backend.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static IHostBuilder UseStructuredLogging(this IHostBuilder host, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.File(
                    path:                   "ApplicationData/Logs/api-.txt",
                    rollingInterval:        RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            host.UseSerilog();

            return host;
        }
    }
}
