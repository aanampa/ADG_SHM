# SHM.AppWebCompaniaMedica

Portal Web MVC para Proveedores y Compañias Medicas del Sistema de Honorarios Medicos (SHM) - Grupo San Pablo.

## Descripcion General

Este proyecto es una aplicacion web ASP.NET Core MVC que permite a los proveedores y compañias medicas gestionar sus facturas electronicas, visualizar estadisticas y administrar su perfil dentro del sistema de honorarios medicos.

## Stack Tecnologico

| Componente | Tecnologia |
|------------|------------|
| Framework | ASP.NET Core 8.0 MVC |
| Lenguaje | C# 12 |
| Base de Datos | Oracle Database 21c |
| ORM | Dapper |
| Autenticacion | Cookie Authentication |
| Frontend | Bootstrap 5.3.2, jQuery, Chart.js |
| Logging | NLog |
| Encriptacion | BCrypt.Net-Next |

## Estructura del Proyecto

```
SHM.AppWebCompaniaMedica/
├── Controllers/
│   ├── AuthController.cs         # Autenticacion (Login, Logout, Recuperar Clave)
│   ├── BaseController.cs         # Controlador base con [Authorize]
│   ├── HomeController.cs         # Dashboard y pagina principal
│   ├── FacturasController.cs     # Gestion de facturas
│   └── UsuarioController.cs      # Perfil y configuracion de usuario
├── Views/
│   ├── Auth/                     # Vistas de autenticacion
│   │   ├── Login.cshtml
│   │   ├── RecuperarClave.cshtml
│   │   ├── RestablecerClave.cshtml
│   │   ├── RestablecerClaveExitoso.cshtml
│   │   └── AccesoDenegado.cshtml
│   ├── Facturas/                 # Vistas de facturas
│   │   ├── Pendientes.cshtml
│   │   ├── Enviadas.cshtml
│   │   ├── Subir.cshtml
│   │   ├── Detalle.cshtml
│   │   └── Historial.cshtml
│   ├── Home/                     # Vistas principales
│   │   ├── Index.cshtml
│   │   ├── Dashboard.cshtml
│   │   └── Privacy.cshtml
│   ├── Usuario/                  # Vistas de usuario
│   │   ├── Perfil.cshtml
│   │   └── Configuracion.cshtml
│   └── Shared/                   # Layouts y componentes compartidos
│       ├── _Layout.cshtml
│       ├── _LayoutLogin.cshtml
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
├── Models/
│   └── ErrorViewModel.cs
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   ├── images/                   # Logos e imagenes
│   ├── lib/                      # Librerias JS/CSS (Bootstrap, jQuery)
│   └── archivos/                 # Archivos de ejemplo
├── Logs/                         # Archivos de log (generados en runtime)
├── Program.cs                    # Configuracion de la aplicacion
├── appsettings.json              # Configuracion general
├── appsettings.Development.json  # Configuracion de desarrollo
└── nlog.config                   # Configuracion de NLog
```

## Funcionalidades

### Autenticacion y Seguridad
- Login con usuario y contraseña
- CAPTCHA generado en cliente (Canvas)
- Cookie Authentication con expiracion de 30 minutos
- Sliding Expiration activada
- Recuperacion de contraseña por email con token temporal
- Logout con modal de confirmacion
- Proteccion CSRF integrada
- Cookies HttpOnly

### Dashboard
- Cards de estadisticas en tiempo real
- Grafico de barras (Facturas por mes)
- Grafico de dona (Estados de facturas)
- Resumen del mes
- Notificaciones recientes

### Gestion de Facturas
- Listar facturas pendientes con busqueda
- Listar facturas enviadas con paginacion
- Subir facturas (PDF, XML, CDR)
- Ver detalle de factura
- Historial de facturas

### Perfil de Usuario
- Informacion personal
- Informacion de empresa
- Informacion bancaria
- Cambio de contraseña
- Estadisticas del usuario

### Configuracion
- Configuracion de cuenta
- Gestion de notificaciones
- Informacion del sistema

## Configuracion

### Cadena de Conexion (appsettings.json)

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1"
  }
}
```

### Configuracion SMTP (appsettings.json)

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "tu-correo@gmail.com",
    "Password": "tu-app-password",
    "FromEmail": "tu-correo@gmail.com",
    "FromName": "Portal de Honorarios - San Pablo"
  }
}
```

## Dependencias

### Paquetes NuGet
```xml
<PackageReference Include="NLog.Web.AspNetCore" Version="5.3.5" />
```

### Referencias a Proyectos
```xml
<ProjectReference Include="..\SHM.AppApplication\SHM.AppApplication.csproj" />
<ProjectReference Include="..\SHM.AppDomain\SHM.AppDomain.csproj" />
<ProjectReference Include="..\SHM.AppInfrastructure\SHM.AppInfrastructure.csproj" />
```

### Librerias Frontend
- Bootstrap 5.3.2
- Bootstrap Icons 1.11.1
- jQuery 3.x
- jQuery Validation
- Chart.js
- DataTables (jQuery plugin)
- SweetAlert2
- Google Fonts (Outfit, JetBrains Mono)

## Servicios Registrados

```csharp
// Configuracion
builder.Services.AddSingleton<DatabaseConfig>();
builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Servicios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Autenticacion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".SHM.Auth";
        options.Cookie.HttpOnly = true;
    });
```

## Ejecucion

### Requisitos Previos
- .NET 8.0 SDK
- Oracle Database (local o Docker)
- Navegador web moderno

### Iniciar la Aplicacion

```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\src\SHM.AppWebCompaniaMedica
dotnet run
```

### URLs de Acceso
| Perfil | URL |
|--------|-----|
| HTTP | http://localhost:5251 |
| HTTPS | https://localhost:7229 |

## Controladores y Rutas

### AuthController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Auth/Login | Formulario de login |
| POST | /Auth/Login | Validar credenciales |
| GET | /Auth/Logout | Cerrar sesion |
| GET | /Auth/AccesoDenegado | Pagina de acceso denegado |
| GET | /Auth/RecuperarClave | Formulario de recuperacion |
| POST | /Auth/RecuperarClave | Generar token y enviar email |
| GET | /Auth/RestablecerClave | Formulario de restablecimiento |
| POST | /Auth/RestablecerClave | Actualizar contraseña |

### HomeController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Home/Index | Redirecciona a Dashboard |
| GET | /Home/Dashboard | Panel principal |
| GET | /Home/Privacy | Pagina de privacidad |

### FacturasController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Facturas/Pendientes | Listar facturas pendientes |
| GET | /Facturas/Enviadas | Listar facturas enviadas |
| GET | /Facturas/Subir | Formulario de carga |
| POST | /Facturas/SubirFactura | Procesar carga |
| GET | /Facturas/Detalle | Detalle de factura |
| GET | /Facturas/Historial | Historial de facturas |

### UsuarioController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Usuario/Perfil | Ver perfil |
| GET | /Usuario/Configuracion | Ver configuracion |
| POST | /Usuario/ActualizarPerfil | Guardar perfil |
| POST | /Usuario/ActualizarConfiguracion | Guardar configuracion |

## Logging

Los logs se generan diariamente en la carpeta `Logs/` con el formato:
```
Logs/Log_yyyyMMdd.txt
```

Configuracion en `nlog.config`:
- Archivo de log rotativo diario
- Niveles: Debug, Info, Warning, Error
- Console output para desarrollo

## Diseño y UX

### Colores Corporativos
- Purpura principal: `#5a1160`
- Naranja secundario: `#f26522`
- Fondo claro: `#f8f9fa`

### Caracteristicas
- Diseño responsive (mobile-first)
- Sidebar collapsible en dispositivos moviles
- Animaciones CSS (fade-in, stagger)
- Cards con sombras y bordes redondeados
- Tipografia: Outfit (texto), JetBrains Mono (codigo)

## Notas Importantes

1. **Validacion de Contraseñas**: Actualmente usa validacion temporal sin cifrado BCrypt para desarrollo. En produccion, habilitar la validacion completa.

2. **SMTP**: Configurar credenciales reales de SMTP en `appsettings.json` para el envio de correos.

3. **Datos Estaticos**: Algunas vistas usan datos estaticos de ejemplo. Reemplazar por consultas reales a la base de datos.

4. **Token de Recuperacion**: El token de recuperacion de contraseña expira en 1 hora.

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
