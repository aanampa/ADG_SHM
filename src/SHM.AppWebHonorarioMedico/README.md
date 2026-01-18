# SHM.AppWebHonorarioMedico

Portal Web Administrativo para el Sistema de Honorarios Medicos (SHM) - Grupo San Pablo.

## Descripcion General

Aplicacion web ASP.NET Core MVC para la administracion del sistema de honorarios medicos. Este portal es utilizado por el personal administrativo del hospital para gestionar usuarios, companias medicas, producciones, facturacion y reportes.

## Stack Tecnologico

| Componente | Tecnologia |
|------------|------------|
| Framework | ASP.NET Core 8.0 MVC |
| Lenguaje | C# 12 |
| Base de Datos | Oracle Database 21c |
| ORM | Dapper |
| Autenticacion | Cookie Authentication |
| Frontend | Bootstrap 5, jQuery |
| Logging | NLog |
| Encriptacion | BCrypt.Net-Next |

## Estructura del Proyecto

```
SHM.AppWebHonorarioMedico/
├── Controllers/
│   ├── AuthController.cs           # Autenticacion
│   ├── BaseController.cs           # Controlador base
│   ├── HomeController.cs           # Dashboard
│   ├── UsuariosController.cs       # Gestion de usuarios
│   ├── EntidadesMedicasController.cs # Gestion de companias medicas
│   ├── ProduccionesController.cs   # Gestion de producciones
│   ├── FacturasController.cs       # Gestion de facturas
│   └── ReportesController.cs       # Reportes y estadisticas
├── Views/
│   ├── Auth/                       # Vistas de autenticacion
│   ├── Home/                       # Dashboard
│   ├── Usuarios/                   # CRUD de usuarios
│   ├── EntidadesMedicas/           # CRUD de entidades
│   ├── Producciones/               # Gestion de producciones
│   ├── Facturas/                   # Gestion de facturas
│   ├── Reportes/                   # Reportes
│   └── Shared/                     # Layouts compartidos
├── Models/
│   └── ViewModels/                 # ViewModels especificos
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│   └── lib/
├── Logs/
├── Program.cs
├── appsettings.json
└── nlog.config
```

## Funcionalidades

### Autenticacion y Seguridad
- Login con credenciales
- Cookie Authentication
- Control de acceso por roles
- Logout seguro

### Dashboard Administrativo
- Resumen de estadisticas generales
- Facturas pendientes de revision
- Alertas y notificaciones
- Graficos de produccion

### Gestion de Usuarios
- Listar usuarios del sistema
- Crear nuevos usuarios
- Editar usuarios existentes
- Desactivar usuarios (soft delete)
- Asignar perfiles y permisos

### Gestion de Entidades Medicas
- Listar companias medicas
- Crear/editar entidades
- Gestionar cuentas bancarias
- Ver estadisticas por entidad

### Gestion de Producciones
- Listar producciones por periodo
- Filtrar por sede, entidad, estado
- Ver detalle de produccion
- Aprobar/rechazar producciones

### Gestion de Facturas
- Revisar facturas enviadas
- Aprobar/rechazar facturas
- Ver archivos adjuntos (XML, PDF)
- Historial de cambios

### Reportes
- Reporte de producciones por periodo
- Reporte de facturacion
- Exportar a Excel/PDF
- Graficos estadisticos

## Configuracion

### Cadena de Conexion (appsettings.json)

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1"
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
- Bootstrap Icons
- jQuery 3.x
- jQuery Validation
- DataTables
- Chart.js
- SweetAlert2

## Ejecucion

### Requisitos Previos
- .NET 8.0 SDK
- Oracle Database (local o Docker)
- Navegador web moderno

### Iniciar la Aplicacion

```bash
cd src/SHM.AppWebHonorarioMedico
dotnet run
```

### URLs de Acceso
| Perfil | URL |
|--------|-----|
| HTTPS | https://localhost:5002 |

## Controladores y Rutas

### AuthController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Auth/Login | Formulario de login |
| POST | /Auth/Login | Validar credenciales |
| GET | /Auth/Logout | Cerrar sesion |

### HomeController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | / | Dashboard principal |

### UsuariosController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Usuarios | Listar usuarios |
| GET | /Usuarios/Crear | Formulario de creacion |
| POST | /Usuarios/Crear | Guardar nuevo usuario |
| GET | /Usuarios/Editar/{guid} | Formulario de edicion |
| POST | /Usuarios/Editar | Guardar cambios |
| POST | /Usuarios/Eliminar | Desactivar usuario |

### EntidadesMedicasController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /EntidadesMedicas | Listar entidades |
| GET | /EntidadesMedicas/Detalle/{guid} | Ver detalle |
| GET | /EntidadesMedicas/Crear | Formulario de creacion |
| POST | /EntidadesMedicas/Crear | Guardar entidad |

### ProduccionesController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Producciones | Listar producciones |
| GET | /Producciones/Detalle/{guid} | Ver detalle |
| POST | /Producciones/Aprobar | Aprobar produccion |
| POST | /Producciones/Rechazar | Rechazar produccion |

### FacturasController
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /Facturas | Listar facturas |
| GET | /Facturas/Detalle/{guid} | Ver detalle |
| POST | /Facturas/Aprobar | Aprobar factura |
| POST | /Facturas/Rechazar | Rechazar factura |

## Logging

Los logs se generan en la carpeta `Logs/` con rotacion diaria:
```
Logs/Log_yyyyMMdd.txt
```

## Diseño y UX

### Colores Corporativos
- Purpura principal: `#5a1160`
- Naranja secundario: `#f26522`
- Fondo: `#f5f7fa`

### Caracteristicas
- Diseño responsive
- Sidebar con navegacion
- DataTables con paginacion y busqueda
- Modales de confirmacion
- Notificaciones toast

## Notas de Desarrollo

1. **Roles de Usuario**: Implementar control de acceso basado en perfiles.

2. **Auditoria**: Todas las acciones quedan registradas con usuario y fecha.

3. **GUID en URLs**: Nunca exponer IDs de base de datos.

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
