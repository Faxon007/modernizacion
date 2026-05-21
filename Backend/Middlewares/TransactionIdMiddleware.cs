using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend.Middlewares
{
    public class TransactionIdMiddleware(RequestDelegate next)
    {
        private const string HeaderName = "x-transaction-id";

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            if (path == "/" || 
                path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) || 
                path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out var values) || string.IsNullOrWhiteSpace(values))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { success = false, code = "8", message = "El header 'x-transaction-id' es requerido." });
                return;
            }

            var txId = values.ToString();
            if (!Guid.TryParse(txId, out _))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { success = false, code = "9", message = "El header 'x-transaction-id' debe ser un GUID válido." });
                return;
            }

            context.Items[HeaderName] = txId;

            await next(context);
        }
    }
}
