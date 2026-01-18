# SHM - Sistema de Honorarios Medicos

Sistema para la gestion de honorarios medicos del Grupo San Pablo.

## Tecnologias

- .NET 8.0
- ASP.NET Core MVC
- Dapper (ORM)
- Oracle Database 21c XE
- Oracle.ManagedDataAccess.Core
- BCrypt.Net-Next (Hashing de passwords)
- NLog (Logging)
- Swagger/OpenAPI
- Bootstrap 5
- SweetAlert2

## Arquitectura

El proyecto sigue una arquitectura Clean Architecture con las siguientes capas:

```
src/
├── SHM.AppDomain/                  # Capa de Dominio
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Entities/                   # Entidades del dominio
│   └── Interfaces/                 # Contratos (Repositories, Services)
│
├── SHM.AppApplication/             # Capa de Aplicacion
│   └── Services/                   # Logica de negocio
│
├── SHM.AppInfrastructure/          # Capa de Infraestructura
│   ├── Configurations/             # Configuracion de BD
│   └── Repositories/               # Implementacion de repositorios
│
├── SHM.AppApiHonorarioMedico/      # API REST (Backend)
│   └── Controllers/                # Controladores REST
│
├── SHM.AppWebHonorarioMedico/      # Portal Web Administrativo
│   ├── Controllers/                # Controladores MVC
│   ├── Views/                      # Vistas Razor
│   └── wwwroot/                    # Assets estaticos
│
└── SHM.AppWebCompaniaMedica/       # Portal Web para Compania Medica
    ├── Controllers/                # Controladores MVC
    ├── Views/                      # Vistas Razor
    └── wwwroot/                    # Assets estaticos
```

## Aplicaciones

### 1. SHM.AppApiHonorarioMedico (API REST)
API REST para consumo de servicios. Incluye Swagger para documentacion.

**Puerto:** 5001 (HTTPS)

### 2. SHM.AppWebHonorarioMedico (Portal Administrativo)
Portal web para la administracion del sistema de honorarios medicos.

**Puerto:** 5002 (HTTPS)

### 3. SHM.AppWebCompaniaMedica (Portal Compania Medica)
Portal web para que las companias medicas gestionen sus facturas y perfil.

**Puerto:** 5003 (HTTPS)

**Funcionalidades implementadas:**
- Login con autenticacion por cookies
- Subir Factura (carga de XML y PDF)
- Facturas Enviadas (listado con DataTables)
- Detalle de Factura
- Perfil de Usuario (edicion de datos personales, cambio de password)
- Informacion de Entidad Medica (solo lectura)
- Cuentas Bancarias (solo lectura)

## Requisitos Previos

- .NET 8.0 SDK
- Oracle Database 21c XE (Docker recomendado)
- Docker y Docker Compose (opcional)

## Configuracion de Base de Datos

### Usando Docker

```bash
cd c:\PROYECTOS\SAN_PABLO\02_HHMM\ORACLE
docker-compose up -d
```

Ver `README_ORACLE.md` para mas detalles sobre la configuracion de Oracle.

### Parametros de Conexion

| Parametro | Valor |
|-----------|-------|
| Host | localhost |
| Puerto | 11521 |
| Service Name | XEPDB1 |
| Usuario | shm_dev |
| Password | DevPass123 |

## Ejecucion

### Compilar la solucion

```bash
cd src
dotnet build SHM.HonorarioMedico.sln
```

### Ejecutar API

```bash
cd src/SHM.AppApiHonorarioMedico
dotnet run
```

### Ejecutar Portal Administrativo

```bash
cd src/SHM.AppWebHonorarioMedico
dotnet run
```

### Ejecutar Portal Compania Medica

```bash
cd src/SHM.AppWebCompaniaMedica
dotnet run
```

## Entidades Principales

| Entidad | Tabla | Descripcion |
|---------|-------|-------------|
| Usuario | SHM_SEG_USUARIO | Usuarios del sistema |
| Perfil | SHM_SEG_PERFIL | Perfiles de usuario |
| Sede | SHM_SEDE | Sedes/Locaciones |
| EntidadMedica | SHM_ENTIDAD_MEDICA | Companias medicas |
| EntidadCuentaBancaria | SHM_ENTIDAD_CUENTA_BANCARIA | Cuentas bancarias |
| Banco | SHM_BANCO | Catalogo de bancos |
| Produccion | SHM_PRODUCCION | Produccion medica |
| Factura | SHM_FACTURA | Facturas |
| FacturaArchivo | SHM_FACTURA_ARCHIVO | Archivos de factura (XML, PDF) |
| Tabla | SHM_TABLA | Tablas maestras |
| TablaDetalle | SHM_TABLA_DETALLE | Detalles de tablas maestras |

## Caracteristicas

- **Soft Delete**: Los registros no se eliminan fisicamente, se desactivan (ACTIVO = 0)
- **Auditoria**: Campos de creacion y modificacion (IdCreador, FechaCreacion, IdModificador, FechaModificacion)
- **GUID**: Cada registro tiene un identificador unico GUID para seguridad en URLs
- **Logging**: Logs diarios generados en carpeta Logs/
- **CORS**: Configurado para permitir todos los origenes (ajustar en produccion)
- **Autenticacion**: Basada en Cookies con Claims

## Patrones de Seguridad

- **GUID en URLs**: Nunca se exponen IDs de base de datos al cliente
- **Hash de Passwords**: BCrypt con salt automatico
- **Claims**: Usuario autenticado almacena IdUsuario, Login, Nombres, IdEntidadMedica

## Validaciones Frontend

Se utiliza SweetAlert2 para validaciones con el siguiente estilo:
- **Warning (campos requeridos)**: Color #f26522
- **Error**: Color #dc3545
- **Success**: Color #28a745

## Scripts SQL

Los scripts de base de datos se encuentran en:
```
db/
├── 01_create_user.sql    # Creacion de usuario Oracle
├── 02_create_tables.sql  # Creacion de tablas y secuencias
└── 03_insert_data.sql    # Datos iniciales
```

## Conexion a Oracle via SQL*Plus

```bash
docker exec -it oracle-practice-db sqlplus shm_dev/DevPass123@//localhost:1521/XEPDB1
```

## Variables CSS

El proyecto usa variables CSS para consistencia:

```css
:root {
    --primary-color: #3498db;
    --secondary-color: #2c3e50;
    --accent-color: #f26522;      /* Color San Pablo */
    --success-color: #28a745;
    --danger-color: #dc3545;
    --bg-main: #f5f7fa;
}
```

## Notas de Desarrollo

Ver `NOTAS_CONTEXTO.md` para notas detalladas del contexto de desarrollo.
