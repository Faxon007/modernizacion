using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        private const string HeaderName = "x-transaction-id";

        public async Task InvokeAsync(HttpContext context)
        {
            var txId = context.Items[HeaderName]?.ToString() ?? "N/A";
            var sw = Stopwatch.StartNew();

            logger.LogInformation(
                "[START] [{TxId}] {Method} {Path}{Query}",
                txId,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            try
            {
                await next(context);
            }
            finally
            {
                sw.Stop();

                var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                          : context.Response.StatusCode >= 400 ? LogLevel.Warning
                          : LogLevel.Information;

                logger.Log(level,
                    "[END] [{TxId}] {Method} {Path} -> {StatusCode} | {ElapsedMs}ms",
                    txId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
    }
}
