using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog;
using NLog.Web;
using SHM.AppApplication.Services;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppInfrastructure.Configurations;
using SHM.AppInfrastructure.HealthChecks;
using SHM.AppInfrastructure.Repositories;

// Configurar NLog temprano para capturar todos los errores de inicio
//var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

logger.Debug("Iniciando aplicación SHM Honorario Medico API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configurar NLog como proveedor de logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container
    builder.Services.AddControllers();

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "SHM Honorario Medico API",
            Version = "v1",
            Description = "API REST para la gestión de honorarios médicos del sistema SHM"
        });
    });

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    // Dependency Injection
    builder.Services.AddSingleton<DatabaseConfig>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<ITablaRepository, TablaRepository>();
    builder.Services.AddScoped<ITablaService, TablaService>();
    builder.Services.AddScoped<ITablaDetalleRepository, TablaDetalleRepository>();
    builder.Services.AddScoped<ITablaDetalleService, TablaDetalleService>();
    builder.Services.AddScoped<ISedeRepository, SedeRepository>();
    builder.Services.AddScoped<ISedeService, SedeService>();
    builder.Services.AddScoped<IEntidadMedicaRepository, EntidadMedicaRepository>();
    builder.Services.AddScoped<IEntidadMedicaService, EntidadMedicaService>();
    builder.Services.AddScoped<IEntidadCuentaBancariaRepository, EntidadCuentaBancariaRepository>();
    builder.Services.AddScoped<IEntidadCuentaBancariaService, EntidadCuentaBancariaService>();
    builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();
    builder.Services.AddScoped<IArchivoService, ArchivoService>();
    builder.Services.AddScoped<IBancoRepository, BancoRepository>();
    builder.Services.AddScoped<IBancoService, BancoService>();
    builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
    builder.Services.AddScoped<IProduccionInterfaceService, ProduccionInterfaceService>();
    builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
    builder.Services.AddScoped<IParametroService, ParametroService>();

    builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
    builder.Services.AddScoped<IParametroService, ParametroService>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<OracleHealthCheck>("oracle-database", tags: new[] { "db", "oracle" });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment() || 1==1)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SHM Honorario Medico API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

    // Health Check endpoint con respuesta JSON detallada
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds + " ms",
                timestamp = DateTime.Now,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds + " ms"
                })
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    });

    logger.Info("Aplicación iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    // Capturar errores de inicio
    logger.Error(ex, "La aplicación se detuvo debido a un error");
    throw;
}
finally
{
    // Asegurar que NLog libere los recursos
    LogManager.Shutdown();
}
