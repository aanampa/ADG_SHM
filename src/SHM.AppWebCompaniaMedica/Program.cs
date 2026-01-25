using Microsoft.AspNetCore.Authentication.Cookies;
using NLog;
using NLog.Web;
using SHM.AppApplication.Services;
using SHM.AppDomain.Configurations;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppInfrastructure.Configurations;
using SHM.AppInfrastructure.Repositories;
using SHM.AppWebCompaniaMedica.Services;

// Configurar NLog
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Iniciando aplicacion SHM.AppWebCompaniaMedica");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar NLog como proveedor de logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    var mvcBuilder = builder.Services.AddControllersWithViews();

    // Habilitar Razor Runtime Compilation solo en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        mvcBuilder.AddRazorRuntimeCompilation();
    }

    // Configurar Cookie Authentication
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/AccesoDenegado";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".SHM.Auth";
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

    // Registrar servicios de infraestructura
    builder.Services.AddSingleton<DatabaseConfig>();

    // Registrar repositorios
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
    builder.Services.AddScoped<ISedeRepository, SedeRepository>();
    builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();
    builder.Services.AddScoped<IArchivoComprobanteRepository, ArchivoComprobanteRepository>();
    builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
    builder.Services.AddScoped<IEntidadMedicaRepository, EntidadMedicaRepository>();
    builder.Services.AddScoped<IEntidadCuentaBancariaRepository, EntidadCuentaBancariaRepository>();
    builder.Services.AddScoped<IBancoRepository, BancoRepository>();
    builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
    builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();

    // Registrar servicios de aplicacion
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<IProduccionService, ProduccionService>();
    builder.Services.AddScoped<ISedeService, SedeService>();
    builder.Services.AddScoped<IArchivoService, ArchivoService>();
    builder.Services.AddScoped<IArchivoComprobanteService, ArchivoComprobanteService>();
    builder.Services.AddScoped<IBitacoraService, BitacoraService>();
    builder.Services.AddScoped<IEntidadMedicaService, EntidadMedicaService>();
    builder.Services.AddScoped<IEntidadCuentaBancariaService, EntidadCuentaBancariaService>();
    builder.Services.AddScoped<IBancoService, BancoService>();
    builder.Services.AddScoped<IParametroService, ParametroService>();

    // Configurar SmtpSettings
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Registrar servicios de la aplicacion web
    builder.Services.AddScoped<FacturaXmlParserService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "La aplicacion se detuvo por una excepcion");
    throw;
}
finally
{
    LogManager.Shutdown();
}
