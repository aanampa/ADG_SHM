# Recomendaciones Generales para el Desarrollo

## Contexto del Proyecto
Este documento contiene las recomendaciones y estandares para el desarrollo del Sistema de Honorarios Medicos (SHM) del Grupo San Pablo.

Antes de comenzar, lee los archivos:
- `NOTAS_CONTEXTO.md` - Contexto del proyecto
- `src/README.md` - Documentacion tecnica general

---

## 1. Arquitectura y Estructura

### Clean Architecture
El proyecto sigue Clean Architecture con 4 capas:

```
SHM.AppDomain          → Entidades, DTOs, Interfaces (sin dependencias)
SHM.AppApplication     → Servicios de negocio (depende de Domain)
SHM.AppInfrastructure  → Repositorios, BD (depende de Domain)
SHM.AppWeb*/Api*       → Presentacion (depende de todas)
```

**Regla de oro**: Las dependencias siempre apuntan hacia adentro (hacia Domain).

### Nombrado de Archivos y Clases

| Tipo | Convencion | Ejemplo |
|------|------------|---------|
| Entidad | Singular, PascalCase | `Usuario.cs`, `Factura.cs` |
| DTO | `[Entidad][Accion]Dto` | `CreateUsuarioDto.cs`, `UsuarioResponseDto.cs` |
| Repository | `I[Entidad]Repository` | `IUsuarioRepository.cs` |
| Service | `I[Entidad]Service` | `IUsuarioService.cs` |
| Controller | `[Entidad]Controller` | `UsuarioController.cs` |
| ViewModel | `[Entidad]ViewModel` | `PerfilViewModel.cs` |

### Documentacion de Codigo (XML Comments)

**IMPORTANTE**: Solo documentar a nivel de **clase** y **metodo**. No es necesario documentar propiedades o atributos individuales.

El objetivo principal es identificar al autor y fecha para facilitar el seguimiento en los merges.

#### Formato para Clases

```csharp
/// <summary>
/// Servicio para la gestion de usuarios del sistema.
/// </summary>
/// <author>Juan Perez</author>
/// <created>2025-01-18</created>
public class UsuarioService : IUsuarioService
{
    // Propiedades y campos NO necesitan documentacion
    private readonly IUsuarioRepository _usuarioRepository;

    public int IdUsuario { get; set; }
    public string Login { get; set; }
}
```

#### Formato para Metodos

```csharp
/// <summary>
/// Crea un nuevo usuario en el sistema.
/// </summary>
/// <author>Juan Perez</author>
/// <created>2025-01-18</created>
public async Task<UsuarioResponseDto> CreateUsuarioAsync(CreateUsuarioDto createDto, int idCreador)
{
    // Implementacion...
}
```

#### Formato Minimo Requerido

Para cada clase o metodo nuevo, incluir al menos:

```csharp
/// <summary>
/// [Descripcion breve de lo que hace]
/// </summary>
/// <author>[Nombre del desarrollador]</author>
/// <created>[Fecha YYYY-MM-DD]</created>
```

#### Ejemplos por Tipo de Archivo

**Entidad:**
```csharp
/// <summary>
/// Entidad que representa un usuario del sistema.
/// Mapeada a la tabla SHM_SEG_USUARIO.
/// </summary>
/// <author>Juan Perez</author>
/// <created>2025-01-18</created>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Login { get; set; }
    public int Activo { get; set; }
    // ... demas propiedades sin documentar
}
```

**DTO:**
```csharp
/// <summary>
/// DTO para la creacion de un nuevo usuario.
/// </summary>
/// <author>Maria Garcia</author>
/// <created>2025-01-18</created>
public class CreateUsuarioDto
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string Password { get; set; }
}
```

**Repositorio:**
```csharp
/// <summary>
/// Repositorio para operaciones de base de datos de usuarios.
/// Utiliza Dapper con Oracle Database.
/// </summary>
/// <author>Carlos Lopez</author>
/// <created>2025-01-18</created>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Obtiene todos los usuarios activos.
    /// </summary>
    /// <author>Carlos Lopez</author>
    /// <created>2025-01-18</created>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        // ...
    }
}
```

**Servicio:**
```csharp
/// <summary>
/// Servicio de negocio para gestion de usuarios.
/// </summary>
/// <author>Ana Torres</author>
/// <created>2025-01-18</created>
public class UsuarioService : IUsuarioService
{
    /// <summary>
    /// Valida credenciales y retorna el usuario si son correctas.
    /// </summary>
    /// <author>Ana Torres</author>
    /// <created>2025-01-18</created>
    public async Task<UsuarioResponseDto?> ValidarCredencialesAsync(string login, string password)
    {
        // ...
    }
}
```

**Controlador:**
```csharp
/// <summary>
/// Controlador para la gestion de usuarios.
/// </summary>
/// <author>Pedro Sanchez</author>
/// <created>2025-01-18</created>
[Authorize]
public class UsuarioController : BaseController
{
    /// <summary>
    /// Muestra el listado de usuarios.
    /// </summary>
    /// <author>Pedro Sanchez</author>
    /// <created>2025-01-18</created>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // ...
    }

    /// <summary>
    /// Obtiene la lista de usuarios en formato JSON para DataTables.
    /// </summary>
    /// <author>Pedro Sanchez</author>
    /// <created>2025-01-20</created>
    [HttpGet]
    public async Task<IActionResult> ObtenerLista()
    {
        // ...
    }
}
```

#### Cuando Modificas Codigo Existente

Si modificas significativamente un metodo existente, agregar:

```csharp
/// <summary>
/// Crea un nuevo usuario en el sistema.
/// </summary>
/// <author>Juan Perez</author>
/// <created>2025-01-18</created>
/// <modified>Maria Garcia - 2025-01-25 - Agregada validacion de email</modified>
public async Task<UsuarioResponseDto> CreateUsuarioAsync(CreateUsuarioDto createDto, int idCreador)
{
    // ...
}
```

#### Resumen

| Elemento | Documentar? | Tags requeridos |
|----------|-------------|-----------------|
| Clase | SI | `<summary>`, `<author>`, `<created>` |
| Interface | SI | `<summary>`, `<author>`, `<created>` |
| Metodo publico | SI | `<summary>`, `<author>`, `<created>` |
| Metodo privado | OPCIONAL | Solo si es complejo |
| Propiedad | NO | - |
| Campo | NO | - |
| Constructor | OPCIONAL | Solo si tiene logica especial |

---

## 2. Seguridad

### NUNCA Exponer IDs de Base de Datos

```csharp
// MAL - Expone el ID interno
<a href="/Usuario/Editar/123">Editar</a>
data-id="123"

// BIEN - Usa GUID
<a href="/Usuario/Editar/ABC123-DEF456">Editar</a>
data-guid="ABC123-DEF456"
```

Todas las tablas tienen el campo `GUID_REGISTRO` para este proposito.

### Validar Siempre en el Servidor

Aunque se valide en JavaScript, **siempre** validar en el servidor:

```csharp
[HttpPost]
public async Task<IActionResult> Guardar(MiViewModel model)
{
    if (!ModelState.IsValid)
    {
        // Retornar errores
    }
    // Continuar...
}
```

### Token CSRF

Incluir en todos los formularios:
```html
@Html.AntiForgeryToken()
```

En peticiones AJAX:
```javascript
headers: {
    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
}
```

### Contraseñas

- Usar BCrypt para hash de contraseñas
- Nunca almacenar contraseñas en texto plano
- Nunca loguear contraseñas

```csharp
// Hashear
var hash = BCrypt.Net.BCrypt.HashPassword(password);

// Verificar
var esValido = BCrypt.Net.BCrypt.Verify(password, hashAlmacenado);
```

---

## 3. Base de Datos (Oracle)

### Convenciones de Nombrado

| Elemento | Convencion | Ejemplo |
|----------|------------|---------|
| Tabla | `SHM_[NOMBRE]` | `SHM_USUARIO`, `SHM_FACTURA` |
| Columna PK | `ID_[TABLA]` | `ID_USUARIO`, `ID_FACTURA` |
| Columna FK | `ID_[TABLA_REFERENCIA]` | `ID_ENTIDAD_MEDICA` |
| Secuencia | `SHM_[TABLA]_SEQ` | `SHM_USUARIO_SEQ` |
| Indice | `IDX_[TABLA]_[COLUMNA]` | `IDX_USUARIO_LOGIN` |

### Campos de Auditoria (obligatorios en todas las tablas)

```sql
ACTIVO NUMBER(1) DEFAULT 1,           -- 1=Activo, 0=Inactivo
GUID_REGISTRO VARCHAR2(100),          -- Identificador publico
ID_CREADOR NUMBER,                    -- Usuario que creo
FECHA_CREACION DATE DEFAULT SYSDATE,  -- Fecha de creacion
ID_MODIFICADOR NUMBER,                -- Usuario que modifico
FECHA_MODIFICACION DATE               -- Fecha de modificacion
```

### Soft Delete

**Nunca eliminar registros fisicamente**. Usar soft delete:

```csharp
// En el repositorio
public async Task<bool> DeleteAsync(int id, int idModificador)
{
    var sql = @"UPDATE SHM_TABLA
                SET ACTIVO = 0,
                    ID_MODIFICADOR = :IdModificador,
                    FECHA_MODIFICACION = SYSDATE
                WHERE ID_TABLA = :Id";
    // ...
}
```

### Mapeo de Columnas con Dapper

Usar alias para mapear columnas Oracle a propiedades C#:

```csharp
private const string SelectColumns = @"
    ID_USUARIO as IdUsuario,
    LOGIN as Login,
    NOMBRES as Nombres,
    FECHA_CREACION as FechaCreacion";

var sql = $"SELECT {SelectColumns} FROM SHM_SEG_USUARIO WHERE ACTIVO = 1";
```

---

## 4. Frontend

### Librerias Estandar

- **Bootstrap 5.3** - Framework CSS
- **Bootstrap Icons** - Iconos
- **jQuery** - Manipulacion DOM y AJAX
- **DataTables** - Tablas con paginacion y busqueda
- **SweetAlert2** - Alertas y confirmaciones
- **Select2** - Combos con busqueda (para listas grandes)
- **Chart.js** - Graficos (dashboard)

### Colores Corporativos

```css
:root {
    --primary-color: #3498db;
    --secondary-color: #2c3e50;
    --accent-color: #f26522;      /* Naranja San Pablo */
    --success-color: #28a745;
    --danger-color: #dc3545;
    --warning-color: #f26522;     /* Usar para validaciones */
    --bg-main: #f5f7fa;           /* Fondo principal */
}
```

### SweetAlert2 - Estilos del Proyecto

```javascript
// Exito
Swal.fire({
    icon: 'success',
    title: 'Exito',
    text: 'Operacion completada',
    confirmButtonColor: '#28a745',
    timer: 2000
});

// Error
Swal.fire({
    icon: 'error',
    title: 'Error',
    text: 'Ocurrio un problema',
    confirmButtonColor: '#dc3545'
});

// Advertencia / Validacion
Swal.fire({
    icon: 'warning',
    title: 'Campos requeridos',
    html: '<ul><li>Campo 1</li><li>Campo 2</li></ul>',
    confirmButtonColor: '#f26522'  // Color San Pablo
});

// Confirmacion
const result = await Swal.fire({
    title: '¿Confirmar?',
    text: 'Esta accion no se puede deshacer',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'Si, continuar',
    cancelButtonText: 'Cancelar'
});
```

### Campos Obligatorios

Marcar con asterisco rojo:
```html
<label class="form-label">
    Nombre <span class="text-danger">*</span>
</label>
```

Incluir mensaje al inicio del formulario:
```html
<p class="text-muted small">
    Los campos marcados con <span class="text-danger">*</span> son obligatorios.
</p>
```

### Campos de Solo Lectura

```html
<input class="form-control" value="@Model.Codigo" readonly
       style="background-color: var(--bg-main);" />
```

---

## 5. JavaScript

### Usar ES6+ (JavaScript Moderno)

```javascript
// Preferir const y let (nunca var)
const items = [];
let contador = 0;

// Arrow functions
const procesar = (item) => item.valor * 2;

// Template literals
const html = `<div class="${clase}">${contenido}</div>`;

// Async/await (en lugar de callbacks)
const data = await fetch(url).then(r => r.json());

// Destructuring
const { nombre, email } = usuario;

// Spread operator
const nuevo = { ...original, campo: 'valor' };
```

### Delegacion de Eventos

Para elementos dinamicos (ej: botones en DataTables):

```javascript
// MAL - No funciona con elementos dinamicos
document.querySelectorAll('.btn-eliminar').forEach(btn => {
    btn.addEventListener('click', ...);
});

// BIEN - Delegacion de eventos
document.getElementById('contenedor').addEventListener('click', (e) => {
    const btn = e.target.closest('.btn-eliminar');
    if (btn) {
        const guid = btn.dataset.guid;
        // Manejar click
    }
});
```

### Fetch API para AJAX

```javascript
// GET
const response = await fetch('/api/datos');
const data = await response.json();

// POST con JSON
const response = await fetch('/api/guardar', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': token
    },
    body: JSON.stringify(datos)
});

// POST con FormData (para archivos)
const formData = new FormData(formulario);
const response = await fetch('/api/subir', {
    method: 'POST',
    body: formData
});
```

### Manejo de Errores

```javascript
try {
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }

    const data = await response.json();
    // Procesar data...

} catch (error) {
    console.error('Error:', error);
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: 'No se pudo completar la operacion'
    });
}
```

---

## 6. Patrones Comunes

### Inyeccion de Dependencias

Registrar en `Program.cs`:

```csharp
// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Servicios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
```

### Obtener Usuario Logueado

En controladores que heredan de `BaseController`:

```csharp
var idUsuario = GetUsuarioId();
var login = GetUsuarioLogin();
var nombres = GetUsuarioNombres();
var idEntidadMedica = GetUsuarioIdEntidadMedica();
```

### TempData para Mensajes entre Requests

```csharp
// En el controlador (guardar)
TempData["Mensaje"] = "Registro guardado correctamente";
TempData["TipoMensaje"] = "success";
return RedirectToAction("Index");

// En la vista (mostrar)
@if (TempData["Mensaje"] != null)
{
    <script>
        Swal.fire({
            icon: '@TempData["TipoMensaje"]',
            text: '@TempData["Mensaje"]'
        });
    </script>
}
```

### ViewModels vs DTOs

- **DTO**: Para transferencia de datos entre capas (Domain)
- **ViewModel**: Para la vista, puede incluir listas para combos, propiedades calculadas, etc.

```csharp
// ViewModel incluye datos adicionales para la vista
public class FacturaFormViewModel
{
    public string Serie { get; set; }
    public decimal Monto { get; set; }

    // Para el combo de entidades
    public List<SelectListItem> EntidadesDisponibles { get; set; }

    // Propiedad calculada
    public string NumeroCompleto => $"{Serie}-{Numero}";
}
```

---

## 7. Validaciones

### En el ViewModel (Data Annotations)

```csharp
[Required(ErrorMessage = "El campo es requerido")]
[StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
[EmailAddress(ErrorMessage = "Email no valido")]
[RegularExpression(@"^\d{9}$", ErrorMessage = "Debe tener 9 digitos")]
[Range(0.01, double.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
```

### En JavaScript (antes de enviar)

```javascript
function validarFormulario() {
    const errores = [];

    // Requerido
    if (!nombre.value.trim()) {
        errores.push('El nombre es requerido');
    }

    // Email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email.value)) {
        errores.push('El email no es valido');
    }

    // Telefono Peru
    if (telefono.value && !/^\d{9}$/.test(telefono.value)) {
        errores.push('El telefono debe tener 9 digitos');
    }

    // RUC Peru
    if (ruc.value && !/^\d{11}$/.test(ruc.value)) {
        errores.push('El RUC debe tener 11 digitos');
    }

    return errores;
}
```

---

## 8. Logging

Usar NLog para registrar eventos:

```csharp
private readonly ILogger<MiController> _logger;

public MiController(ILogger<MiController> logger)
{
    _logger = logger;
}

// Uso
_logger.LogInformation("Usuario {Login} inicio sesion", login);
_logger.LogWarning("Intento de acceso no autorizado");
_logger.LogError(ex, "Error al procesar factura {Id}", id);
```

Los logs se guardan en `Logs/Log_yyyyMMdd.txt`.

---

## 9. Commits y Versionado

### Mensajes de Commit

Formato: `[tipo]: descripcion breve`

Tipos:
- `feat`: Nueva funcionalidad
- `fix`: Correccion de bug
- `refactor`: Refactorizacion de codigo
- `style`: Cambios de formato (no afectan logica)
- `docs`: Documentacion
- `test`: Tests

Ejemplos:
```
feat: agregar pantalla de perfil de usuario
fix: corregir validacion de email en formulario
refactor: extraer logica de validacion a servicio
docs: actualizar README con instrucciones de instalacion
```

---

## 10. Checklist Antes de Commit

- [ ] El codigo compila sin errores
- [ ] No hay warnings criticos
- [ ] Las validaciones funcionan en cliente y servidor
- [ ] Los campos obligatorios tienen asterisco (*)
- [ ] Se usa GUID en lugar de ID en URLs
- [ ] Token CSRF incluido en formularios
- [ ] Mensajes de error son amigables para el usuario
- [ ] Los logs registran operaciones importantes
- [ ] El codigo sigue las convenciones del proyecto

---

## 11. Recursos y Referencias

### Documentacion del Proyecto
- `NOTAS_CONTEXTO.md` - Contexto y estado actual
- `src/README.md` - Documentacion tecnica
- `prompts/prompt_pantalla_lista.md` - Guia para pantallas tipo lista
- `prompts/prompt_pantalla_detalle.md` - Guia para formularios
- `prompts/prompt_nueva_entidad.md` - Guia para agregar entidades

### Documentacion Externa
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Dapper](https://github.com/DapperLib/Dapper)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3)
- [DataTables](https://datatables.net)
- [SweetAlert2](https://sweetalert2.github.io)
- [Select2](https://select2.org)

---

## 12. Contacto y Soporte

Para dudas sobre el proyecto, consultar con el lider tecnico o revisar la documentacion en la carpeta `prompts/`.

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
