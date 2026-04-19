using Microsoft.AspNetCore.Authentication.Cookies;
using NLog;
using NLog.Web;
using SHM.AppApplication.Services;
using SHM.AppDomain.Configurations;
using SHM.AppDomain.DTOs.SanPabloApi;
using SHM.AppDomain.DTOs.SapApi;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppInfrastructure.Configurations;
using SHM.AppInfrastructure.HealthChecks;
using SHM.AppInfrastructure.Repositories;
using SHM.AppWebCompaniaMedica.Services;

// Configurar NLog
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Iniciando aplicacion SHM.AppWebCompaniaMedica");

    var builder = WebApplication.CreateBuilder(args);

    // Fijar cultura en-US para que el formato de numeros (decimales, miles)
    // sea siempre consistente independientemente del locale del servidor.
    var culturaNumerica = new System.Globalization.CultureInfo("en-US");
    System.Globalization.CultureInfo.DefaultThreadCurrentCulture   = culturaNumerica;
    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culturaNumerica;
    builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(opts =>
    {
        opts.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
        opts.SupportedCultures     = new[] { culturaNumerica };
        opts.SupportedUICultures   = new[] { culturaNumerica };
        opts.RequestCultureProviders.Clear(); // ignorar Accept-Language del browser
    });

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
    builder.Services.AddScoped<IUsuarioSedeRepository, UsuarioSedeRepository>();
    builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
    builder.Services.AddScoped<ISedeRepository, SedeRepository>();
    builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();
    builder.Services.AddScoped<IArchivoComprobanteRepository, ArchivoComprobanteRepository>();
    builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
    builder.Services.AddScoped<IEntidadMedicaRepository, EntidadMedicaRepository>();
    builder.Services.AddScoped<IEntidadCuentaBancariaRepository, EntidadCuentaBancariaRepository>();
    builder.Services.AddScoped<IBancoRepository, BancoRepository>();
    builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
    builder.Services.AddScoped<ITablaRepository, TablaRepository>();
    builder.Services.AddScoped<ITablaDetalleRepository, TablaDetalleRepository>();
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
    builder.Services.AddScoped<ITablaService, TablaService>();
    builder.Services.AddScoped<ITablaDetalleService, TablaDetalleService>();

    // Configurar SmtpSettings
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Configuracion del API externo de San Pablo
    builder.Services.Configure<SanPabloApiSettings>(
        builder.Configuration.GetSection("SanPabloApi"));

    // Registrar HttpClient y servicio para API San Pablo
    builder.Services.AddHttpClient<ISanPabloApiService, SanPabloApiService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

    // Configuracion del API de SAP
    builder.Services.Configure<SapApiSettings>(
        builder.Configuration.GetSection("SapApi"));

    // Registrar HttpClient y servicio para API SAP
    builder.Services.AddHttpClient<ISapApiService, SapApiService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

    // Registrar servicios de la aplicacion web
    builder.Services.AddScoped<FacturaXmlParserService>();
    builder.Services.AddScoped<RheXmlParserService>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<OracleHealthCheck>("oracle-database", tags: new[] { "db", "oracle" });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRequestLocalization();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Middleware: Forzar cambio de clave si el password es temporal
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var passwordTemporal = context.User.FindFirst("PasswordTemporal")?.Value;
            if (passwordTemporal == "1")
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                // Permitir solo CambiarClave, Logout y archivos estaticos
                if (!path.Contains("/auth/cambiarclave") &&
                    !path.Contains("/auth/logout") &&
                    !path.StartsWith("/vendor/") &&
                    !path.StartsWith("/css/") &&
                    !path.StartsWith("/js/") &&
                    !path.StartsWith("/images/") &&
                    !path.StartsWith("/lib/"))
                {
                    context.Response.Redirect("/Auth/CambiarClave");
                    return;
                }
            }
        }
        await next();
    });

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
