using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

using Backend.Infrastructure.Database;
using Backend.Infrastructure.Security;
using Backend.Repositories;
using Backend.Services;
using Backend.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Log Environment Name immediately
Console.WriteLine($"[STARTUP] Current ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");

// Configurar Serilog para registro estructurado
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(
        path: "ApplicationData/Logs/api-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// CORS para permitir la comunicación con el Frontend de Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Registrar base de datos o mocks según configuración
bool useMockData = builder.Configuration.GetValue<bool>("Database:UseMockData");

if (useMockData)
{
    Console.WriteLine("MODO SIMULADO/DUMMY ACTIVO EN EL BACKEND (No requiere archivo de conexiones encriptadas).");
    Log.Information("MODO SIMULADO/DUMMY ACTIVO EN EL BACKEND.");

    builder.Services.AddSingleton<IClientRepository, MockClientRepository>();
    builder.Services.AddSingleton<ILinkRepository, MockLinkRepository>();
    builder.Services.AddSingleton<IMenuRepository, MockMenuRepository>();
    builder.Services.AddSingleton<IProductRepository, MockProductRepository>();
    builder.Services.AddSingleton<ISiteRepository, MockSiteRepository>();
    builder.Services.AddSingleton<ITransactionRepository, MockTransactionRepository>();
    builder.Services.AddSingleton<ICarrierRepository, MockCarrierRepository>();
}
else
{
    // Registrar base de datos (.cef2)
    builder.Services.AddEncryptedDatabaseConnections();
    
    // Registrar utilidades para inyectar credenciales del usuario
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDataProtection();
    builder.Services.AddScoped<IUserConnectionProvider, UserConnectionProvider>();

    // Registrar repositorios manualmente usando IUserConnectionProvider
    builder.Services.AddScoped<IClientRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        return new ClientRepository(cs);
    });

    builder.Services.AddScoped<ILinkRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        var logger = sp.GetService<ILogger<LinkRepository>>();
        return new LinkRepository(cs, logger);
    });

    builder.Services.AddScoped<IMenuRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        return new MenuRepository(cs);
    });

    builder.Services.AddScoped<IProductRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        return new ProductRepository(cs);
    });

    builder.Services.AddScoped<ISiteRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        var logger = sp.GetRequiredService<ILogger<SiteRepository>>();
        return new SiteRepository(cs,logger);
    });

    builder.Services.AddScoped<ITransactionRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        return new TransactionRepository(cs);
    });

    builder.Services.AddScoped<ICarrierRepository>(sp =>
    {
        var db = sp.GetRequiredService<IUserConnectionProvider>();
        var cs = db.GetUserConnectionString(db.DefaultKey)
            ?? throw new InvalidOperationException($"Conexión {db.DefaultKey} no encontrada.");
        return new CarrierRepository(cs);
    });
}

// Configurar Opciones para servicios
builder.Services.Configure<VisaEnLinkOptions>(builder.Configuration.GetSection("VisaEnLink"));
builder.Services.Configure<UrlShortenerOptions>(builder.Configuration.GetSection("UrlShortener"));

// Registrar Clientes HTTP para consumo de APIs externas
builder.Services.AddHttpClient<IVisaEnLinkIntegrationService, VisaEnLinkIntegrationService>()
    .ConfigureHttpClient((sp, client) => {
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<VisaEnLinkOptions>>().Value;
        if (!string.IsNullOrEmpty(options.UrlVisa)) {
            client.BaseAddress = new Uri(options.UrlVisa);
        }
    })
    .ConfigurePrimaryHttpMessageHandler((sp) => {
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<VisaEnLinkOptions>>().Value;
        if (options.UseProxy && !string.IsNullOrEmpty(options.ProxyUrl)) {
            var proxy = new System.Net.WebProxy(options.ProxyUrl);
            if (!string.IsNullOrEmpty(options.ProxyUser)) {
                proxy.Credentials = new System.Net.NetworkCredential(options.ProxyUser, options.ProxyPassword);
            }
            return new System.Net.Http.HttpClientHandler { Proxy = proxy, UseProxy = true };
        }
        return new System.Net.Http.HttpClientHandler();
    });
builder.Services.AddHttpClient<IUrlShortenerService, UrlShortenerService>();

// Registrar Servicios de Negocio
builder.Services.AddScoped<ILinkBusinessService, LinkBusinessService>();

// Registrar Seguridad y JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Registrar Controladores
builder.Services.AddControllers();

// Configurar Swagger con soporte JWT y TransactionId
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cobro Visa En Link API",
        Version = "v1",
        Description = "API REST de Cobro Visa En Link modernizada"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT (sin el prefijo Bearer)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.OperationFilter<TransactionIdHeaderFilter>();
});

var app = builder.Build();

// Configurar CORS
app.UseCors("AllowAngularDev");

// Habilitar Swagger en desarrollo y producción para facilitar pruebas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cobro Visa En Link API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// Usar middlewares en orden correcto para peticiones de API
app.UseMiddleware<TransactionIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Registrar en consola que las conexiones están listas
try
{
    var db = app.Services.GetService<IDatabaseConnectionProvider>();
    if (db != null)
    {
        foreach (var (key, alias) in db.GetAvailableConnections())
        {
            Log.Information("Base de datos disponible: {Key} -> {Alias}", key, alias);
            Console.WriteLine($"Base de datos disponible: {key} -> {alias}");
        }
    }
    else
    {
        Log.Information("Base de datos omitida (Modo Mock Activo).");
        Console.WriteLine("Base de datos omitida (Modo Mock Activo).");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Error al verificar las conexiones iniciales.");
    Console.WriteLine($"Error al verificar las conexiones iniciales: {ex.Message}");
}

app.Run();
