# SHM - Sistema de Honorarios Medicos

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Oracle](https://img.shields.io/badge/Oracle-21c%20XE-red)](https://www.oracle.com/database/)
[![License](https://img.shields.io/badge/License-Proprietary-blue)]()

Sistema integral para la gestion de honorarios medicos del **Grupo San Pablo**. Permite administrar la produccion medica, facturacion, y gestion de entidades medicas asociadas.

## Tabla de Contenidos

- [Tecnologias](#tecnologias)
- [Arquitectura](#arquitectura)
- [Aplicaciones](#aplicaciones)
- [Requisitos Previos](#requisitos-previos)
- [Instalacion](#instalacion)
- [Configuracion de Base de Datos](#configuracion-de-base-de-datos)
- [Ejecucion](#ejecucion)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Documentacion](#documentacion)
- [Contribucion](#contribucion)
- [Autor](#autor)

## Tecnologias

| Categoria | Tecnologia |
|-----------|------------|
| **Backend** | .NET 8.0, ASP.NET Core MVC |
| **ORM** | Dapper |
| **Base de Datos** | Oracle Database 21c XE |
| **Driver BD** | Oracle.ManagedDataAccess.Core |
| **Seguridad** | BCrypt.Net-Next (Hashing), Cookie Authentication |
| **Logging** | NLog |
| **Documentacion API** | Swagger/OpenAPI |
| **Frontend** | Bootstrap 5, SweetAlert2, DataTables |
| **Contenedores** | Docker, Docker Compose |

## Arquitectura

El proyecto implementa **Clean Architecture** (Arquitectura Limpia) con separacion clara de responsabilidades:

```
                    +-----------------------+
                    |   Presentation Layer  |
                    |  (API, Web Portals)   |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |  Application Layer    |
                    |     (Services)        |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |    Domain Layer       |
                    | (Entities, DTOs,      |
                    |  Interfaces)          |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    | Infrastructure Layer  |
                    |   (Repositories,      |
                    |    DB Config)         |
                    +-----------------------+
```

### Principios Aplicados

- **Dependency Inversion**: Las capas superiores dependen de abstracciones
- **Separation of Concerns**: Cada capa tiene una responsabilidad especifica
- **Repository Pattern**: Abstraccion del acceso a datos
- **DTO Pattern**: Objetos de transferencia para comunicacion entre capas

## Aplicaciones

El sistema consta de tres aplicaciones:

### 1. API REST (`SHM.AppApiHonorarioMedico`)
API RESTful para consumo de servicios externos e integraciones.

- **Puerto:** 5001 (HTTPS)
- **Documentacion:** Swagger UI disponible en `/swagger`

### 2. Portal Administrativo (`SHM.AppWebHonorarioMedico`)
Portal web para administradores del sistema.

- **Puerto:** 5002 (HTTPS)
- **Funcionalidades:** Gestion de usuarios, entidades, produccion, etc.

### 3. Portal Compania Medica (`SHM.AppWebCompaniaMedica`)
Portal web para entidades medicas externas.

- **Puerto:** 5003 (HTTPS)
- **Funcionalidades:**
  - Login con autenticacion por cookies
  - Subir Factura (carga de XML y PDF)
  - Facturas Enviadas (listado con DataTables)
  - Detalle de Factura
  - Perfil de Usuario
  - Informacion de Entidad Medica
  - Cuentas Bancarias

## Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para Oracle)
- [Git](https://git-scm.com/)

## Instalacion

### 1. Clonar el repositorio

```bash
git clone -b <rama>  https://github.com/aanampa/ADG_SHM.git

Ejemplo:
git clone -b antonio_v1 https://github.com/aanampa/ADG_SHM.git

cd ADG_SHM
```

### 2. Restaurar dependencias

```bash
cd src
dotnet restore SHM.HonorarioMedico.sln
```

### 3. Compilar la solucion

```bash
dotnet build SHM.HonorarioMedico.sln
```

## Configuracion de Base de Datos

### Iniciar Oracle con Docker

```bash
# Desde la raiz del proyecto
docker-compose up -d
```

Esperar ~2 minutos la primera vez. Verificar con:

```bash
docker-compose logs -f
```

Cuando aparezca `DATABASE IS READY TO USE!` está lista.

## Detener

```bash
docker-compose down
```

Los datos se mantienen.

## Reiniciar con datos limpios

```bash
docker-compose down -v
docker-compose up -d
```

Esto iniciara un contenedor de Oracle 21c XE con la configuracion predefinida.

### Parametros de Conexion

| Parametro | Valor |
|-----------|-------|
| **Host** | localhost |
| **Puerto** | 11521 |
| **Service Name** | XEPDB1 |
| **Usuario** | shm_dev |
| **Password** | DevPass123 |

### Ejecutar Scripts SQL

Los scripts se ejecutan en orden:

```bash
# Conectar a Oracle
docker exec -it oracle-practice-db sqlplus shm_dev/DevPass123@//localhost:1521/XEPDB1

# Ejecutar scripts en orden:
# 1. db/01_create_user.sql
# 2. db/02_create_tables.sql
# 3. db/03_insert_data.sql
```

Para mas detalles, consultar [README_ORACLE.md](README_ORACLE.md).

## Ejecucion

### Ejecutar API

```bash
cd src/SHM.AppApiHonorarioMedico
dotnet run
```
Acceder a: `https://localhost:5001/swagger`

### Ejecutar Portal Administrativo

```bash
cd src/SHM.AppWebHonorarioMedico
dotnet run
```
Acceder a: `https://localhost:5002`

### Ejecutar Portal Compania Medica

```bash
cd src/SHM.AppWebCompaniaMedica
dotnet run
```
Acceder a: `https://localhost:5003`

## Estructura del Proyecto

```
ORACLE/
├── db/                                 # Scripts SQL
│   ├── 01_create_user.sql
│   ├── 02_create_tables.sql
│   └── 03_insert_data.sql
│
├── prompts/                            # Prompts para desarrollo con IA
│
├── src/                                # Codigo fuente
│   ├── SHM.AppDomain/                  # Capa de Dominio
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   ├── Entities/                   # Entidades del dominio
│   │   └── Interfaces/                 # Contratos
│   │
│   ├── SHM.AppApplication/             # Capa de Aplicacion
│   │   └── Services/                   # Logica de negocio
│   │
│   ├── SHM.AppInfrastructure/          # Capa de Infraestructura
│   │   ├── Configurations/             # Configuracion de BD
│   │   └── Repositories/               # Repositorios
│   │
│   ├── SHM.AppApiHonorarioMedico/      # API REST
│   ├── SHM.AppWebHonorarioMedico/      # Portal Administrativo
│   └── SHM.AppWebCompaniaMedica/       # Portal Compania Medica
│
├── docker-compose.yml                  # Configuracion Docker
├── README.md                           # Este archivo
└── README_ORACLE.md                    # Guia de Oracle
```

## Entidades Principales

| Entidad | Tabla | Descripcion |
|---------|-------|-------------|
| Usuario | SHM_SEG_USUARIO | Usuarios del sistema |
| Rol | SHM_SEG_ROL | Roles de seguridad |
| Opcion | SHM_SEG_OPCION | Opciones del menu |
| Sede | SHM_SEDE | Sedes/Locaciones |
| EntidadMedica | SHM_ENTIDAD_MEDICA | Companias medicas |
| EntidadCuentaBancaria | SHM_ENTIDAD_CUENTA_BANCARIA | Cuentas bancarias |
| Banco | SHM_BANCO | Catalogo de bancos |
| Produccion | SHM_PRODUCCION | Produccion medica |
| Archivo | SHM_ARCHIVO | Archivos del sistema |
| ArchivoComprobante | SHM_ARCHIVO_COMPROBANTE | Relacion archivo-comprobante |
| Tabla | SHM_TABLA | Tablas maestras |
| TablaDetalle | SHM_TABLA_DETALLE | Detalles de tablas maestras |
| Parametro | SHM_PARAMETRO | Parametros del sistema |
| Bitacora | SHM_SEG_BITACORA | Auditoria de acciones |

## Caracteristicas de Seguridad

- **GUID en URLs**: Los IDs de base de datos nunca se exponen al cliente
- **Hash de Passwords**: BCrypt con salt automatico
- **Autenticacion por Cookies**: Claims para datos del usuario
- **Soft Delete**: Los registros se desactivan, no se eliminan
- **Auditoria**: Campos de creacion y modificacion en todas las tablas

## Documentacion

| Documento | Descripcion |
|-----------|-------------|
| [src/README.md](src/README.md) | Documentacion tecnica detallada |
| [README_ORACLE.md](README_ORACLE.md) | Guia de configuracion de Oracle |
| [prompts/](prompts/) | Prompts utilizados para desarrollo |

## Contribucion

1. Crear una rama desde `develop`:
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```

2. Realizar los cambios siguiendo las convenciones del proyecto

3. Documentar el codigo con XML comments:
   ```csharp
   /// <summary>
   /// Descripcion de la clase o metodo.
   ///
   /// <author>Nombre Autor</author>
   /// <created>YYYY-MM-DD</created>
   /// </summary>
   ```

4. Crear un Pull Request hacia `develop`

## Estilos del Proyecto

### Colores CSS

```css
:root {
    --primary-color: #3498db;
    --secondary-color: #2c3e50;
    --accent-color: #f26522;      /* Color San Pablo */
    --success-color: #28a745;
    --danger-color: #dc3545;
}
```

### Validaciones SweetAlert2

- **Warning (campos requeridos)**: `#f26522`
- **Error**: `#dc3545`
- **Success**: `#28a745`

## Autor

**ADG Antonio** - Desarrollo inicial (2026)

---

*Sistema desarrollado para Grupo San Pablo*
