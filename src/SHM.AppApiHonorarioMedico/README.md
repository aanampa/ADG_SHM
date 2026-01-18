# SHM.AppApiHonorarioMedico

API REST para el Sistema de Honorarios Medicos (SHM) - Grupo San Pablo.

## Descripcion General

API REST desarrollada en ASP.NET Core 8.0 que expone los servicios del sistema de honorarios medicos. Provee endpoints para la gestion de usuarios, entidades medicas, producciones, facturas y tablas maestras.

## Stack Tecnologico

| Componente | Tecnologia |
|------------|------------|
| Framework | ASP.NET Core 8.0 Web API |
| Lenguaje | C# 12 |
| Base de Datos | Oracle Database 21c |
| ORM | Dapper |
| Documentacion | Swagger/OpenAPI |
| Logging | NLog |
| Encriptacion | BCrypt.Net-Next |

## Estructura del Proyecto

```
SHM.AppApiHonorarioMedico/
├── Controllers/
│   ├── UsuariosController.cs
│   ├── SedesController.cs
│   ├── TablasController.cs
│   ├── TablaDetallesController.cs
│   ├── EntidadesMedicasController.cs
│   ├── BancosController.cs
│   ├── ProduccionesController.cs
│   └── FacturasController.cs
├── Logs/
├── Program.cs
├── appsettings.json
└── nlog.config
```

## Configuracion

### Cadena de Conexion (appsettings.json)

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1"
  }
}
```

### CORS

Configurado para permitir todos los origenes en desarrollo. Ajustar en produccion:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## Dependencias

### Paquetes NuGet
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="NLog.Web.AspNetCore" Version="5.3.5" />
```

### Referencias a Proyectos
```xml
<ProjectReference Include="..\SHM.AppApplication\SHM.AppApplication.csproj" />
<ProjectReference Include="..\SHM.AppDomain\SHM.AppDomain.csproj" />
<ProjectReference Include="..\SHM.AppInfrastructure\SHM.AppInfrastructure.csproj" />
```

## Ejecucion

### Requisitos Previos
- .NET 8.0 SDK
- Oracle Database (local o Docker)

### Iniciar la API

```bash
cd src/SHM.AppApiHonorarioMedico
dotnet run
```

### URLs de Acceso
| Recurso | URL |
|---------|-----|
| API | https://localhost:5001 |
| Swagger | https://localhost:5001/swagger |

## Endpoints

### Usuarios (`/api/usuarios`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/usuarios | Obtener todos los usuarios |
| GET | /api/usuarios/{id} | Obtener usuario por ID |
| GET | /api/usuarios/guid/{guid} | Obtener usuario por GUID |
| GET | /api/usuarios/login/{login} | Obtener usuario por login |
| POST | /api/usuarios | Crear nuevo usuario |
| PUT | /api/usuarios/{id} | Actualizar usuario |
| DELETE | /api/usuarios/{id} | Desactivar usuario |

### Sedes (`/api/sedes`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/sedes | Obtener todas las sedes |
| GET | /api/sedes/{id} | Obtener sede por ID |
| GET | /api/sedes/codigo/{codigo} | Obtener sede por codigo |
| POST | /api/sedes | Crear nueva sede |
| PUT | /api/sedes/{id} | Actualizar sede |
| DELETE | /api/sedes/{id} | Desactivar sede |

### Tablas Maestras (`/api/tablas`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/tablas | Obtener todas las tablas |
| GET | /api/tablas/{id} | Obtener tabla por ID |
| GET | /api/tablas/codigo/{codigo} | Obtener tabla por codigo |
| POST | /api/tablas | Crear nueva tabla |
| PUT | /api/tablas/{id} | Actualizar tabla |
| DELETE | /api/tablas/{id} | Desactivar tabla |

### Tabla Detalles (`/api/tabladetalles`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/tabladetalles | Obtener todos los detalles |
| GET | /api/tabladetalles/{id} | Obtener detalle por ID |
| GET | /api/tabladetalles/tabla/{idTabla} | Obtener detalles por tabla |
| GET | /api/tabladetalles/tabla/{idTabla}/codigo/{codigo} | Obtener detalle por codigo |
| POST | /api/tabladetalles | Crear nuevo detalle |
| PUT | /api/tabladetalles/{id} | Actualizar detalle |
| DELETE | /api/tabladetalles/{id} | Desactivar detalle |

### Entidades Medicas (`/api/entidadesmedicas`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/entidadesmedicas | Obtener todas las entidades |
| GET | /api/entidadesmedicas/{id} | Obtener entidad por ID |
| GET | /api/entidadesmedicas/guid/{guid} | Obtener entidad por GUID |
| GET | /api/entidadesmedicas/ruc/{ruc} | Obtener entidad por RUC |
| POST | /api/entidadesmedicas | Crear nueva entidad |
| PUT | /api/entidadesmedicas/{id} | Actualizar entidad |
| DELETE | /api/entidadesmedicas/{id} | Desactivar entidad |

### Bancos (`/api/bancos`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/bancos | Obtener todos los bancos |
| GET | /api/bancos/{id} | Obtener banco por ID |
| POST | /api/bancos | Crear nuevo banco |
| PUT | /api/bancos/{id} | Actualizar banco |
| DELETE | /api/bancos/{id} | Desactivar banco |

### Producciones (`/api/producciones`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/producciones | Obtener todas las producciones |
| GET | /api/producciones/{id} | Obtener produccion por ID |
| GET | /api/producciones/guid/{guid} | Obtener produccion por GUID |
| GET | /api/producciones/sede/{idSede} | Obtener por sede |
| GET | /api/producciones/entidad/{idEntidad} | Obtener por entidad |
| GET | /api/producciones/periodo/{periodo} | Obtener por periodo |
| POST | /api/producciones | Crear nueva produccion |
| PUT | /api/producciones/{id} | Actualizar produccion |
| DELETE | /api/producciones/{id} | Desactivar produccion |

### Facturas (`/api/facturas`)

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/facturas | Obtener todas las facturas |
| GET | /api/facturas/{id} | Obtener factura por ID |
| GET | /api/facturas/guid/{guid} | Obtener factura por GUID |
| GET | /api/facturas/entidad/{idEntidad} | Obtener por entidad |
| POST | /api/facturas | Crear nueva factura |
| PUT | /api/facturas/{id} | Actualizar factura |
| DELETE | /api/facturas/{id} | Desactivar factura |

## Respuestas HTTP

| Codigo | Descripcion |
|--------|-------------|
| 200 | OK - Operacion exitosa |
| 201 | Created - Recurso creado |
| 204 | No Content - Eliminacion exitosa |
| 400 | Bad Request - Datos invalidos |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error del servidor |

## Formato de Respuesta

### Exito
```json
{
  "idUsuario": 1,
  "login": "admin",
  "nombres": "Administrador",
  "apellidoPaterno": "Sistema",
  "email": "admin@sanpablo.com",
  "activo": 1,
  "guidRegistro": "ABC123..."
}
```

### Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "El campo 'Login' es requerido.",
  "traceId": "00-abc123..."
}
```

## Logging

Los logs se generan en la carpeta `Logs/`:
```
Logs/Log_yyyyMMdd.txt
```

Niveles de log:
- **Debug**: Informacion detallada de desarrollo
- **Info**: Operaciones normales
- **Warning**: Situaciones inusuales
- **Error**: Errores de la aplicacion

## Swagger/OpenAPI

La documentacion interactiva esta disponible en:
```
https://localhost:5001/swagger
```

Permite:
- Explorar todos los endpoints
- Probar las APIs directamente
- Ver esquemas de request/response
- Descargar especificacion OpenAPI

## Caracteristicas

- **Soft Delete**: Los registros se desactivan (ACTIVO = 0), no se eliminan
- **Auditoria**: Campos IdCreador, FechaCreacion, IdModificador, FechaModificacion
- **GUID**: Identificador unico para seguridad en URLs publicas
- **Validacion**: Data Annotations en DTOs
- **Logging**: NLog con rotacion diaria

## Notas de Desarrollo

1. **Autenticacion**: Implementar JWT para seguridad en endpoints.

2. **Rate Limiting**: Considerar limitar requests por cliente.

3. **Versionado**: Planificar versionado de API (/api/v1/, /api/v2/).

4. **Cache**: Implementar cache para consultas frecuentes.

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
