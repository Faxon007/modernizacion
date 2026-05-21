using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Middlewares
{
    /// <summary>
    /// Agrega el header x-transaction-id como parámetro requerido en Swagger.
    /// </summary>
    public class TransactionIdHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-transaction-id",
                In = ParameterLocation.Header,
                Required = true,
                Description = "GUID único de trazabilidad por transacción. Ej: 3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Schema = new OpenApiSchema 
                { 
                    Type = "string", 
                    Format = "uuid",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                },
                Example = new Microsoft.OpenApi.Any.OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
            });
        }
    }
}
