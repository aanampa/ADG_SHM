# Notas de Sesion con Claude Code
**Fecha:** 2026-01-13

## Resumen del Proyecto

Proyecto de API REST y Portal Web para gestion de honorarios medicos del sistema SHM (San Pablo Hospital Management).

### Tecnologias Usadas
- .NET 8.0
- Dapper (ORM ligero)
- Oracle Database 21c XE (en Docker)
- Oracle.ManagedDataAccess.Core
- BCrypt.Net-Next (hash de contrasenas)
- NLog (logging)
- Swagger/OpenAPI
- Bootstrap 5.3.2 (frontend)
- Chart.js (graficos)
- Cookie Authentication con Claims

## Estructura del Proyecto

### Proyecto Original (Legacy)
```
c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\
├── apirest/
│   └── ShmUsuarioApi/           # API REST original (monolitico)
├── db/                          # Scripts SQL
│   ├── 01_create_user.sql
│   ├── 02_create_tables.sql
│   └── 03_insert_data.sql
├── docker-compose.yml           # Configuracion Docker para Oracle
└── README.md
```

### Nueva Arquitectura en Capas (Clean Architecture)
```
c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\src\
├── SHM.HonorarioMedico.sln      # Solucion principal
│
├── SHM.AppDomain/               # Capa de Dominio (Library)
│   ├── Configurations/
│   │   └── SmtpSettings.cs
│   ├── DTOs/
│   │   ├── Usuario/
│   │   ├── Tabla/
│   │   ├── TablaDetalle/
│   │   ├── Sede/
│   │   ├── Banco/
│   │   ├── Bitacora/
│   │   ├── Rol/
│   │   ├── Opcion/
│   │   ├── RolOpcion/
│   │   ├── ArchivoComprobante/
│   │   ├── Produccion/
│   │   └── Parametro/
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Tabla.cs
│   │   ├── TablaDetalle.cs
│   │   ├── Sede.cs
│   │   ├── Banco.cs
│   │   ├── Bitacora.cs
│   │   ├── Rol.cs
│   │   ├── Opcion.cs
│   │   ├── RolOpcion.cs
│   │   ├── ArchivoComprobante.cs
│   │   ├── Produccion.cs
│   │   └── Parametro.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IUsuarioRepository.cs
│   │   │   ├── ITablaRepository.cs
│   │   │   ├── ITablaDetalleRepository.cs
│   │   │   ├── ISedeRepository.cs
│   │   │   ├── IBancoRepository.cs
│   │   │   ├── IBitacoraRepository.cs
│   │   │   ├── IRolRepository.cs
│   │   │   ├── IOpcionRepository.cs
│   │   │   ├── IRolOpcionRepository.cs
│   │   │   ├── IArchivoComprobanteRepository.cs
│   │   │   ├── IProduccionRepository.cs
│   │   │   └── IParametroRepository.cs
│   │   └── Services/
│   │       ├── IUsuarioService.cs
│   │       ├── IEmailService.cs
│   │       ├── ITablaService.cs
│   │       ├── ITablaDetalleService.cs
│   │       ├── ISedeService.cs
│   │       ├── IBancoService.cs
│   │       ├── IBitacoraService.cs
│   │       ├── IRolService.cs
│   │       ├── IOpcionService.cs
│   │       ├── IRolOpcionService.cs
│   │       ├── IArchivoComprobanteService.cs
│   │       ├── IProduccionService.cs
│   │       └── IParametroService.cs
│   └── Utils/
│
├── SHM.AppApplication/          # Capa de Aplicacion (Library)
│   └── Services/
│       ├── UsuarioService.cs
│       ├── EmailService.cs
│       ├── TablaService.cs
│       ├── TablaDetalleService.cs
│       ├── SedeService.cs
│       ├── BancoService.cs
│       ├── BitacoraService.cs
│       ├── RolService.cs
│       ├── OpcionService.cs
│       ├── RolOpcionService.cs
│       ├── ArchivoComprobanteService.cs
│       ├── ProduccionService.cs
│       └── ParametroService.cs
│
├── SHM.AppInfrastructure/       # Capa de Infraestructura (Library)
│   ├── Configurations/
│   │   └── DatabaseConfig.cs
│   └── Repositories/
│       ├── UsuarioRepository.cs
│       ├── TablaRepository.cs
│       ├── TablaDetalleRepository.cs
│       ├── SedeRepository.cs
│       ├── BancoRepository.cs
│       ├── BitacoraRepository.cs
│       ├── RolRepository.cs
│       ├── OpcionRepository.cs
│       ├── RolOpcionRepository.cs
│       ├── ArchivoComprobanteRepository.cs
│       ├── ProduccionRepository.cs
│       └── ParametroRepository.cs
│
├── SHM.AppApiHonorarioMedico/   # API REST (Web API)
│   ├── Controllers/
│   │   ├── UsuariosController.cs
│   │   ├── TablasController.cs
│   │   ├── TablaDetallesController.cs
│   │   ├── SedesController.cs
│   │   ├── BancosController.cs
│   │   ├── BitacorasController.cs
│   │   ├── RolesController.cs
│   │   ├── OpcionesController.cs
│   │   ├── RolOpcionesController.cs
│   │   ├── ArchivoComprobantesController.cs
│   │   ├── ProduccionesController.cs
│   │   └── ParametrosController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── nlog.config
│
└── SHM.AppWebCompaniaMedica/    # Portal Web MVC (Proveedor/Compania Medica)
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── BaseController.cs
    │   ├── HomeController.cs
    │   ├── FacturasController.cs
    │   └── UsuarioController.cs
    ├── Views/
    │   ├── Auth/
    │   ├── Home/
    │   ├── Facturas/
    │   ├── Usuario/
    │   └── Shared/
    ├── wwwroot/
    ├── Program.cs
    ├── nlog.config
    └── appsettings.json
```

## Entidades CRUD Implementadas (Sesion 2026-01-13)

### Resumen de Entidades Creadas Hoy

| Entidad | Tabla | Secuencia | Endpoints Base |
|---------|-------|-----------|----------------|
| Banco | SHM_BANCO | SHM_BANCO_SEQ | /api/bancos |
| Bitacora | SHM_BITACORA | SHM_BITACORA_SEQ | /api/bitacoras |
| Rol | SHM_SEG_ROL | SHM_ROL_SEQ | /api/roles |
| Opcion | SHM_SEG_OPCION | SHM_SEG_OPCION_SEQ | /api/opciones |
| RolOpcion | SHM_SEG_ROL_OPCION | N/A (PK compuesta) | /api/rolopciones |
| ArchivoComprobante | SHM_ARCHIVO_COMPROBANTE | SHM_ARCHIVO_COMPROBANTE_SEQ | /api/archivocomprobantes |
| Produccion | SHM_PRODUCCION | SHM_PRODUCCION_SEQ | /api/producciones |
| Parametro | SHM_PARAMETRO | SHM_PARAMETRO_SEQ | /api/parametros |

### Endpoints por Entidad

#### Banco (/api/bancos)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/bancos | Obtiene todos los bancos |
| GET | /api/bancos/{id} | Obtiene por ID |
| GET | /api/bancos/codigo/{codigo} | Obtiene por codigo |
| POST | /api/bancos | Crea nuevo banco |
| PUT | /api/bancos/{id} | Actualiza banco |
| DELETE | /api/bancos/{id} | Elimina (desactiva) banco |

#### Bitacora (/api/bitacoras)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/bitacoras | Obtiene todas las bitacoras |
| GET | /api/bitacoras/{id} | Obtiene por ID |
| GET | /api/bitacoras/entidad/{entidad} | Obtiene por entidad |
| POST | /api/bitacoras | Crea nueva bitacora |
| PUT | /api/bitacoras/{id} | Actualiza bitacora |
| DELETE | /api/bitacoras/{id} | Elimina (desactiva) bitacora |

#### Rol (/api/roles)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/roles | Obtiene todos los roles |
| GET | /api/roles/{id} | Obtiene por ID |
| GET | /api/roles/codigo/{codigo} | Obtiene por codigo |
| POST | /api/roles | Crea nuevo rol |
| PUT | /api/roles/{id} | Actualiza rol |
| DELETE | /api/roles/{id} | Elimina (desactiva) rol |

#### Opcion (/api/opciones)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/opciones | Obtiene todas las opciones |
| GET | /api/opciones/{id} | Obtiene por ID |
| GET | /api/opciones/padre/{idOpcionPadre?} | Obtiene por padre (null = raiz) |
| POST | /api/opciones | Crea nueva opcion |
| PUT | /api/opciones/{id} | Actualiza opcion |
| DELETE | /api/opciones/{id} | Elimina (desactiva) opcion |

#### RolOpcion (/api/rolopciones) - Tabla de Relacion
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/rolopciones | Obtiene todas las relaciones |
| GET | /api/rolopciones/{idRol}/{idOpcion} | Obtiene por clave compuesta |
| GET | /api/rolopciones/rol/{idRol} | Obtiene opciones de un rol |
| GET | /api/rolopciones/opcion/{idOpcion} | Obtiene roles de una opcion |
| POST | /api/rolopciones | Crea nueva relacion |
| PUT | /api/rolopciones/{idRol}/{idOpcion} | Actualiza relacion |
| DELETE | /api/rolopciones/{idRol}/{idOpcion} | Elimina (desactiva) relacion |

#### ArchivoComprobante (/api/archivocomprobantes)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/archivocomprobantes | Obtiene todos |
| GET | /api/archivocomprobantes/{id} | Obtiene por ID |
| GET | /api/archivocomprobantes/produccion/{idProduccion} | Obtiene por produccion |
| GET | /api/archivocomprobantes/archivo/{idArchivo} | Obtiene por archivo |
| POST | /api/archivocomprobantes | Crea nuevo |
| PUT | /api/archivocomprobantes/{id} | Actualiza |
| DELETE | /api/archivocomprobantes/{id} | Elimina (desactiva) |

#### Produccion (/api/producciones)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/producciones | Obtiene todas las producciones |
| GET | /api/producciones/{id} | Obtiene por ID |
| GET | /api/producciones/codigo/{codigo} | Obtiene por codigo |
| GET | /api/producciones/sede/{idSede} | Obtiene por sede |
| GET | /api/producciones/entidad-medica/{idEntidadMedica} | Obtiene por entidad medica |
| GET | /api/producciones/periodo/{periodo} | Obtiene por periodo |
| POST | /api/producciones | Crea nueva produccion |
| PUT | /api/producciones/{id} | Actualiza produccion |
| DELETE | /api/producciones/{id} | Elimina (desactiva) produccion |

#### Parametro (/api/parametros)
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/parametros | Obtiene todos los parametros |
| GET | /api/parametros/{id} | Obtiene por ID |
| GET | /api/parametros/codigo/{codigo} | Obtiene por codigo |
| POST | /api/parametros | Crea nuevo parametro |
| PUT | /api/parametros/{id} | Actualiza parametro |
| DELETE | /api/parametros/{id} | Elimina (desactiva) parametro |

### Patron de Implementacion CRUD

Cada entidad sigue el patron:
1. **Entity** (`SHM.AppDomain/Entities/`) - Clase C# que mapea la tabla
2. **DTOs** (`SHM.AppDomain/DTOs/{Entidad}/`) - CreateDto, UpdateDto, ResponseDto
3. **IRepository** (`SHM.AppDomain/Interfaces/Repositories/`) - Interface del repositorio
4. **Repository** (`SHM.AppInfrastructure/Repositories/`) - Implementacion con Dapper
5. **IService** (`SHM.AppDomain/Interfaces/Services/`) - Interface del servicio
6. **Service** (`SHM.AppApplication/Services/`) - Logica de negocio
7. **Controller** (`SHM.AppApiHonorarioMedico/Controllers/`) - Endpoints REST

### Caracteristicas Comunes
- Soft delete: Cambia ACTIVO = 0 en lugar de eliminar
- Auditoria: ID_CREADOR, FECHA_CREACION, ID_MODIFICADOR, FECHA_MODIFICACION
- GUID_REGISTRO: Identificador unico global generado con SYS_GUID()
- Secuencias Oracle para IDs autoincrementales
- Validaciones con DataAnnotations en DTOs

## Referencias entre Proyectos
| Proyecto | Referencia a |
|----------|--------------|
| SHM.AppApplication | SHM.AppDomain |
| SHM.AppInfrastructure | SHM.AppDomain |
| SHM.AppApiHonorarioMedico | Todos los anteriores |
| SHM.AppWebCompaniaMedica | SHM.AppApplication, SHM.AppDomain, SHM.AppInfrastructure |

### Paquetes NuGet por Proyecto
| Proyecto | Paquetes |
|----------|----------|
| SHM.AppDomain | BCrypt.Net-Next |
| SHM.AppApplication | BCrypt.Net-Next, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options |
| SHM.AppInfrastructure | Dapper, Oracle.ManagedDataAccess.Core, Microsoft.Extensions.Configuration.Abstractions |
| SHM.AppApiHonorarioMedico | NLog.Web.AspNetCore, Swashbuckle.AspNetCore 6.5.0 |
| SHM.AppWebCompaniaMedica | NLog.Web.AspNetCore 5.3.5 |

## Conexion a Base de Datos

| Parametro | Valor |
|-----------|-------|
| Host | localhost |
| Puerto | 11521 |
| Service Name | XEPDB1 |
| Usuario | shm_dev |
| Password | DevPass123 |

### Cadena de Conexion .NET
```
User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1
```

## Comandos Utiles

### Iniciar Base de Datos
```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE
docker-compose up -d
```

### Compilar la Solucion
```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\src
dotnet build SHM.HonorarioMedico.sln
```

### Ejecutar la API
```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\src\SHM.AppApiHonorarioMedico
dotnet run
# URL: http://localhost:5000/swagger
```

### Ejecutar el Portal Web
```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE\src\SHM.AppWebCompaniaMedica
dotnet run
# URL: http://localhost:5251
```

### Conectar a Oracle via SQL*Plus
```bash
docker exec -it oracle-practice-db sqlplus shm_dev/DevPass123@//localhost:1521/XEPDB1
```

## Estado Actual / Historial de Sesiones

### Sesion 2026-01-10
- Se creo la nueva estructura de solucion con arquitectura en capas (Clean Architecture)
- Se migraron todos los componentes del proyecto original
- Se agregaron entidades: Tabla, TablaDetalle, Sede

### Sesion 2026-01-11
- Se creo el proyecto SHM.AppWebCompaniaMedica (ASP.NET MVC)
- Se implemento autenticacion con Cookie Authentication y Claims
- Se implemento funcionalidad de Recuperar Clave
- Se configuro NLog para logging

### Sesion 2026-01-13
- Se crearon 8 entidades CRUD completas:
  - **Banco** (SHM_BANCO) - Catalogo de bancos
  - **Bitacora** (SHM_BITACORA) - Registro de auditoría/log
  - **Rol** (SHM_SEG_ROL) - Roles del sistema
  - **Opcion** (SHM_SEG_OPCION) - Opciones de menu (jerarquicas)
  - **RolOpcion** (SHM_SEG_ROL_OPCION) - Relacion muchos a muchos rol-opcion
  - **ArchivoComprobante** (SHM_ARCHIVO_COMPROBANTE) - Archivos de comprobantes
  - **Produccion** (SHM_PRODUCCION) - Producciones medicas (entidad principal)
  - **Parametro** (SHM_PARAMETRO) - Parametros del sistema
- Cada entidad incluye: Entity, DTOs, Repository, Service, Controller
- Patron consistente con soft delete y campos de auditoria

### Sesion 2026-01-15
- **Pantalla de Administracion de Roles** (`/Rol/Index`):
  - CRUD completo de roles con modal de asignacion de opciones de menu
  - Checkbox para seleccionar opciones que tendra acceso cada rol
  - Archivos: RolController.cs, RolViewModels.cs, Views/Rol/*

- **Pantalla de Administracion de Opciones de Menu** (`/Opcion/Index`):
  - CRUD para armar la estructura del menu (opciones padre/hijo)
  - Vista en arbol con opciones jerarquicas
  - Archivos: OpcionController.cs, OpcionViewModels.cs, Views/Opcion/*

- **Correccion del Menu Dinamico**:
  - Arreglado SQL en `UsuarioRepository.GetMenuByLoginAsync()` para incluir opciones padre con UNION
  - Agregado filtro `ssro.ACTIVO = 1` para excluir relaciones inactivas
  - Limpieza de sesion en `AuthController.Logout()` para refrescar cache del menu
  - Corregido parseo de URL en `_Menu.cshtml` con `TrimStart('/')`

- **Resaltado de Menu Activo**:
  - Funcion `f_resaltar_menu_activo()` en `_Layout.cshtml`
  - Compara controlador y accion de la URL actual con los enlaces del menu
  - Se ejecuta despues de cargar el menu via AJAX

- **Verificacion de Paginacion**:
  - Confirmado que Usuario/Externos e Usuario/Internos ya tienen paginacion implementada

## Tareas Pendientes / Proximos Pasos

### Registrar Servicios en Program.cs
- [ ] Agregar las nuevas interfaces y servicios en el contenedor DI de la API
- [ ] Ejemplo:
```csharp
// Repositories
builder.Services.AddScoped<IBancoRepository, BancoRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IOpcionRepository, OpcionRepository>();
builder.Services.AddScoped<IRolOpcionRepository, RolOpcionRepository>();
builder.Services.AddScoped<IArchivoComprobanteRepository, ArchivoComprobanteRepository>();
builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
builder.Services.AddScoped<IParametroRepository, ParametroRepository>();

// Services
builder.Services.AddScoped<IBancoService, BancoService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IOpcionService, OpcionService>();
builder.Services.AddScoped<IRolOpcionService, RolOpcionService>();
builder.Services.AddScoped<IArchivoComprobanteService, ArchivoComprobanteService>();
builder.Services.AddScoped<IProduccionService, ProduccionService>();
builder.Services.AddScoped<IParametroService, ParametroService>();
```

### Base de Datos
- [ ] Verificar que las secuencias existan en Oracle
- [ ] Ejecutar scripts de creacion de tablas si no existen

### Portal Web (SHM.AppWebCompaniaMedica)
- [ ] Configurar cuenta SMTP real para envio de correos
- [ ] Habilitar validacion BCrypt cuando las claves esten cifradas en BD
- [ ] Implementar subida de archivos (facturas PDF/XML)
- [ ] Reemplazar datos estaticos por consultas reales

### API REST
- [ ] Probar la API con la base de datos Oracle
- [ ] Implementar autenticacion JWT
- [ ] Agregar tests unitarios

### Entidades Adicionales por Crear
- [ ] EntidadMedica (SHM_ENTIDAD_MEDICA)
- [ ] EntidadCuentaBancaria (SHM_ENTIDAD_CUENTA_BANCARIA)
- [ ] Archivo (SHM_ARCHIVO)
- [ ] ProduccionDetalle (SHM_PRODUCCION_DETALLE)
- [ ] UsuarioRol (SHM_SEG_USUARIO_ROL)

## Notas Adicionales

- Las contrasenas se almacenan hasheadas con BCrypt (deshabilitado temporalmente para desarrollo)
- El sistema usa soft delete (campo ACTIVO = 0)
- Los logs se generan diariamente en la carpeta Logs/
- CORS esta configurado para permitir todos los origenes (ajustar en produccion)
- El proyecto legacy en apirest/ se mantiene como referencia
- Token de recuperacion expira en 1 hora
- Todas las entidades tienen GUID_REGISTRO generado con SYS_GUID() de Oracle

---
*Archivo actualizado por Claude Code - 2026-01-15 (Sesion 4)*
