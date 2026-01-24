# CLAUDE.md - Contexto del Proyecto SHM

## Informacion General

| Campo | Valor |
|-------|-------|
| **Nombre** | SHM - Sistema de Honorarios Medicos |
| **Cliente** | Grupo San Pablo |
| **Framework** | ASP.NET Core 8 MVC |
| **Base de Datos** | Oracle 21c XE (Docker) |
| **ORM** | Dapper |
| **Arquitectura** | Clean Architecture |

---

## Estructura del Proyecto

```
src/
├── SHM.AppDomain/                    # Capa de Dominio
│   ├── Entities/                     # Entidades del negocio
│   ├── DTOs/                         # Data Transfer Objects
│   └── Interfaces/                   # Contratos (Repositories, Services)
│
├── SHM.AppApplication/               # Capa de Aplicacion
│   └── Services/                     # Logica de negocio
│
├── SHM.AppInfrastructure/            # Capa de Infraestructura
│   ├── Configurations/               # Configuracion de BD
│   └── Repositories/                 # Implementacion con Dapper
│
├── SHM.AppApiHonorarioMedico/        # API REST (puerto 5001)
├── SHM.AppWebHonorarioMedico/        # Portal Administrativo (puerto 5002)
└── SHM.AppWebCompaniaMedica/         # Portal Companias Medicas (puerto 5003)
```

---

## Comandos Frecuentes

```bash
# Compilar solucion
cd src && dotnet build SHM.HonorarioMedico.sln

# Ejecutar Portal Administrativo
cd src/SHM.AppWebHonorarioMedico && dotnet run

# Ejecutar Portal Companias Medicas
cd src/SHM.AppWebCompaniaMedica && dotnet run

# Ejecutar API REST (Swagger en /swagger)
cd src/SHM.AppApiHonorarioMedico && dotnet run

# Iniciar Oracle (Docker)
docker-compose up -d

# Conectar a Oracle via SQL*Plus
docker exec -it oracle-practice-db sqlplus shm_dev/DevPass123@//localhost:1521/XEPDB1
```

---

## Conexion a Base de Datos

```
Host: localhost
Puerto: 11521
Service Name: XEPDB1
Usuario: shm_dev
Password: DevPass123

Connection String:
User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1
```

---

## Entidades Principales

| Entidad | Tabla Oracle | Descripcion |
|---------|--------------|-------------|
| Usuario | SHM_SEG_USUARIO | Usuarios (internos/externos) |
| Rol | SHM_SEG_ROL | Roles de seguridad |
| Opcion | SHM_SEG_OPCION | Opciones del menu |
| RolOpcion | SHM_SEG_ROL_OPCION | Relacion rol-opcion |
| Sede | SHM_SEDE | Sedes/Clinicas |
| EntidadMedica | SHM_ENTIDAD_MEDICA | Companias medicas |
| EntidadCuentaBancaria | SHM_ENTIDAD_CUENTA_BANCO | Cuentas bancarias |
| Banco | SHM_BANCO | Catalogo de bancos |
| Produccion | SHM_PRODUCCION | Produccion medica |
| Archivo | SHM_ARCHIVO | Archivos del sistema |
| ArchivoComprobante | SHM_ARCHIVO_COMPROBANTE | Relacion archivo-comprobante |
| Tabla | SHM_TABLA | Tablas maestras |
| TablaDetalle | SHM_TABLA_DETALLE | Detalles de tablas |
| Parametro | SHM_PARAMETRO | Parametros del sistema |
| Bitacora | SHM_BITACORA | Auditoria |

---

## Patron de Implementacion CRUD

Para cada entidad seguir este patron:

1. **SHM.AppDomain**
   - `Entities/[Entidad].cs`
   - `DTOs/[Entidad]/Create[Entidad]Dto.cs`
   - `DTOs/[Entidad]/Update[Entidad]Dto.cs`
   - `DTOs/[Entidad]/[Entidad]ResponseDto.cs`
   - `Interfaces/Repositories/I[Entidad]Repository.cs`
   - `Interfaces/Services/I[Entidad]Service.cs`

2. **SHM.AppApplication**
   - `Services/[Entidad]Service.cs`

3. **SHM.AppInfrastructure**
   - `Repositories/[Entidad]Repository.cs`

4. **Registrar en Program.cs**
   ```csharp
   builder.Services.AddScoped<I[Entidad]Repository, [Entidad]Repository>();
   builder.Services.AddScoped<I[Entidad]Service, [Entidad]Service>();
   ```

---

## Reglas de Seguridad

### GUID en URLs (OBLIGATORIO)
```csharp
// CORRECTO - Usar GUID
public async Task<IActionResult> Detalle(string guid)
public async Task<IActionResult> Editar(string guid)

// INCORRECTO - Nunca exponer IDs
public async Task<IActionResult> Detalle(int id)  // NO USAR
```

### Soft Delete
```csharp
// Nunca eliminar fisicamente, solo desactivar
UPDATE tabla SET ACTIVO = 0, ID_MODIFICADOR = :id, FECHA_MODIFICACION = SYSDATE
```

### Campos de Auditoria (en todas las tablas)
```sql
GUID_REGISTRO Varchar2(100)      -- Identificador publico
ACTIVO Number(1)                 -- 1=Activo, 0=Inactivo
ID_CREADOR Number
FECHA_CREACION Date
ID_MODIFICADOR Number
FECHA_MODIFICACION Date
```

---

## Autenticacion

- **Tipo**: Cookie Authentication con Claims
- **Expiracion**: 30 minutos (sliding)
- **Claims principales**:
  - `ClaimTypes.NameIdentifier` = IdUsuario
  - `ClaimTypes.Name` = Login
  - `ClaimTypes.GivenName` = Nombres
  - `IdEntidadMedica` (custom)

---

## Estilos y Frontend

### Colores Corporativos
```css
--primary-color: #3498db;
--accent-color: #f26522;      /* Naranja San Pablo */
--success-color: #28a745;
--danger-color: #dc3545;
--bg-main: #f5f5f5;
```

### SweetAlert2
```javascript
// Warning (campos requeridos) - Naranja
confirmButtonColor: '#f26522'

// Error - Rojo
confirmButtonColor: '#dc3545'

// Success - Verde
confirmButtonColor: '#28a745'
```

### Campos Obligatorios
```html
<label>Nombre <span class="text-danger">*</span></label>
```

### Campos Readonly
```html
<input readonly style="background-color: var(--bg-main);" />
```

---

## Librerias Frontend

- Bootstrap 5.3.2
- Bootstrap Icons
- jQuery 3.x
- DataTables (listados con paginacion)
- SweetAlert2 (alertas y confirmaciones)
- Select2 (combos con busqueda)
- Chart.js (graficos)

---

## Documentacion de Codigo

### Formato para Clases y Metodos
```csharp
/// <summary>
/// Descripcion breve de la clase o metodo.
/// </summary>
/// <author>Nombre del Desarrollador</author>
/// <created>YYYY-MM-DD</created>
```

### Modificaciones
```csharp
/// <modified>Nombre - YYYY-MM-DD - Descripcion del cambio</modified>
```

---

## Validaciones JavaScript

```javascript
// Validar antes de enviar
const errores = [];

// Requerido
if (!valor.trim()) errores.push('Campo requerido');

// Email
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

// Telefono Peru (9 digitos)
const telRegex = /^\d{9}$/;

// RUC Peru (11 digitos)
const rucRegex = /^\d{11}$/;

// Mostrar errores
if (errores.length > 0) {
    Swal.fire({
        icon: 'warning',
        title: 'Campos requeridos',
        html: '<ul>' + errores.map(e => '<li>' + e + '</li>').join('') + '</ul>',
        confirmButtonColor: '#f26522'
    });
}
```

---

## Convenciones de Nombrado

| Tipo | Convencion | Ejemplo |
|------|------------|---------|
| Entidad | PascalCase singular | `Usuario`, `EntidadMedica` |
| DTO | [Entidad][Accion]Dto | `CreateUsuarioDto`, `UsuarioResponseDto` |
| Repository | I[Entidad]Repository | `IUsuarioRepository` |
| Service | I[Entidad]Service | `IUsuarioService` |
| Controller | [Entidad]Controller | `UsuarioController` |
| Tabla Oracle | SHM_[NOMBRE] | `SHM_SEG_USUARIO` |
| Secuencia | SHM_[TABLA]_SEQ | `SHM_SEG_USUARIO_SEQ` |

---

## Esquema de Base de Datos

Los scripts SQL del esquema se encuentran en la carpeta `db/`:

| Archivo | Proposito |
|---------|-----------|
| `01_create_user.sql` | Creacion del usuario shm_dev en Oracle |
| `02_create_tables.sql` | Creacion de todas las tablas del sistema |
| `03_create_sequence.sql` | Secuencias para auto-increment de IDs |
| `04_insert_data.sql` | Datos iniciales (tablas maestras, roles, usuario admin) |
| `05_insert_data_test.sql` | Datos de prueba adicionales |
| `06_create_tables_vd 20250120.sql` | Tablas adicionales (version 20250120) |
| `06_inserta_data_vd_20250120.sql` | Datos para tablas adicionales |

### Orden de Ejecucion
```bash
# Ejecutar en Oracle en este orden:
1. 01_create_user.sql      # Crear usuario y permisos
2. 02_create_tables.sql    # Crear estructura de tablas
3. 03_create_sequence.sql  # Crear secuencias
4. 04_insert_data.sql      # Insertar datos iniciales
5. 05_insert_data_test.sql # (Opcional) Datos de prueba
6. 06_create_tables_vd*.sql # Tablas adicionales si aplica
7. 06_inserta_data_vd*.sql  # Datos adicionales si aplica
```

### Estructura de Tablas Principales
```sql
-- Patron comun de auditoria en todas las tablas:
GUID_REGISTRO VARCHAR2(100)      -- Identificador publico (UUID)
ACTIVO NUMBER(1)                 -- 1=Activo, 0=Inactivo (soft delete)
ID_CREADOR NUMBER                -- FK a SHM_SEG_USUARIO
FECHA_CREACION DATE              -- Fecha de creacion del registro
ID_MODIFICADOR NUMBER            -- FK a SHM_SEG_USUARIO
FECHA_MODIFICACION DATE          -- Fecha ultima modificacion
```

---

## Archivos de Referencia

| Archivo | Proposito |
|---------|-----------|
| `NOTAS_CONTEXTO.md` | Contexto detallado de paginas implementadas |
| `NOTAS_SESION_CLAUDE.md` | Historial de sesiones |
| `prompts/prompt_nueva_entidad.md` | Guia para agregar entidades |
| `prompts/prompt_pantalla_lista.md` | Guia para pantallas tipo lista |
| `prompts/prompt_pantalla_detalle.md` | Guia para formularios |
| `prompts/prompt_recomendaciones_generales.md` | Estandares de desarrollo |
| `db/*.sql` | Scripts SQL del esquema de base de datos |

---

## Usuario de Prueba

```
Login: SYSDAMIN
Password: 123456
Tipo: I (Interno)
Rol: Administrador del Sistema
```

---

*Ultima actualizacion: 20 de Enero 2026 - Agregado esquema de base de datos*
