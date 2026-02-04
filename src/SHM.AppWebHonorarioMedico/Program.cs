using Microsoft.AspNetCore.Authentication.Cookies;
using NLog;
using NLog.Web;
using SHM.AppApplication.Services;
using SHM.AppDomain.Configurations;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppInfrastructure.Configurations;
using SHM.AppInfrastructure.Repositories;

// Configurar NLog
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Iniciando aplicacion SHM.AppWebHonorarioMedico");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar NLog como proveedor de logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    var mvcBuilder = builder.Services.AddControllersWithViews();

    // Habilitar Runtime Compilation en Development para refrescar vistas sin recompilar
    if (builder.Environment.IsDevelopment())
    {
        mvcBuilder.AddRazorRuntimeCompilation();
    }

    // Configurar Session
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = ".SHM.HonorarioMedico.Session";
    });

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
            options.Cookie.Name = ".SHM.HonorarioMedico.Auth";
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

    // Registrar servicios de infraestructura
    builder.Services.AddSingleton<DatabaseConfig>();

    // Registrar repositorios
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IEntidadMedicaRepository, EntidadMedicaRepository>();
    builder.Services.AddScoped<ITablaRepository, TablaRepository>();
    builder.Services.AddScoped<ITablaDetalleRepository, TablaDetalleRepository>();
    builder.Services.AddScoped<IRolRepository, RolRepository>();
    builder.Services.AddScoped<IOpcionRepository, OpcionRepository>();
    builder.Services.AddScoped<IRolOpcionRepository, RolOpcionRepository>();
    builder.Services.AddScoped<IEntidadCuentaBancariaRepository, EntidadCuentaBancariaRepository>();
    builder.Services.AddScoped<IBancoRepository, BancoRepository>();
    builder.Services.AddScoped<ISedeRepository, SedeRepository>();
    builder.Services.AddScoped<IUsuarioSedeRepository, UsuarioSedeRepository>();
    builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
    builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
    builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();
    builder.Services.AddScoped<IArchivoComprobanteRepository, ArchivoComprobanteRepository>();
    builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();
    builder.Services.AddScoped<ILiquidacionRepository, LiquidacionRepository>();

    // Registrar servicios de aplicacion
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<IEntidadMedicaService, EntidadMedicaService>();
    builder.Services.AddScoped<ITablaService, TablaService>();
    builder.Services.AddScoped<ITablaDetalleService, TablaDetalleService>();
    builder.Services.AddScoped<IRolService, RolService>();
    builder.Services.AddScoped<IOpcionService, OpcionService>();
    builder.Services.AddScoped<IRolOpcionService, RolOpcionService>();
    builder.Services.AddScoped<IEntidadCuentaBancariaService, EntidadCuentaBancariaService>();
    builder.Services.AddScoped<IBancoService, BancoService>();
    builder.Services.AddScoped<ISedeService, SedeService>();
    builder.Services.AddScoped<IParametroService, ParametroService>();
    builder.Services.AddScoped<IProduccionService, ProduccionService>();
    builder.Services.AddScoped<IArchivoService, ArchivoService>();
    builder.Services.AddScoped<IArchivoComprobanteService, ArchivoComprobanteService>();
    builder.Services.AddScoped<ILiquidacionService, LiquidacionService>();

    // Configurar SmtpSettings
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.AddScoped<IEmailService, EmailService>();

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
    app.UseSession();

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
