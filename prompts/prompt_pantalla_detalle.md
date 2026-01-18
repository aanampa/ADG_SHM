# Prompt para Crear Pantalla Tipo Detalle/Formulario

## Contexto Previo
Lee el archivo `NOTAS_CONTEXTO.md` y `src/README.md` para entender el contexto del proyecto.

## Solicitud

Crea una pantalla tipo detalle/formulario para la entidad **[NombreEntidad]** en el proyecto **[NombreProyectoWeb]**.

---

## Especificaciones Tecnicas

### 1. Arquitectura MVC

```
Controllers/
└── [NombreEntidad]Controller.cs    # Controlador con acciones

Views/[NombreEntidad]/
├── Crear.cshtml                     # Formulario de creacion
├── Editar.cshtml                    # Formulario de edicion
├── Detalle.cshtml                   # Vista solo lectura
└── _FormularioPartial.cshtml        # Partial compartido (opcional)

ViewModels/
└── [NombreEntidad]ViewModels.cs     # ViewModels especificos
```

### 2. ViewModel con Validaciones

```csharp
public class [NombreEntidad]FormViewModel
{
    public string? GuidRegistro { get; set; }  // Para edicion, nunca ID

    [Required(ErrorMessage = "El campo es requerido")]
    [StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo es requerido")]
    [EmailAddress(ErrorMessage = "Email no valido")]
    [Display(Name = "Correo Electronico")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefono")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Debe tener 9 digitos")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Seleccione una opcion")]
    [Display(Name = "Tipo")]
    public int? IdTipo { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    [Display(Name = "Monto")]
    public decimal? Monto { get; set; }

    // Para combos
    public List<SelectListItem> TiposDisponibles { get; set; } = new();
}
```

### 3. Controlador

```csharp
[Authorize]
public class [NombreEntidad]Controller : BaseController
{
    private readonly I[NombreEntidad]Service _service;
    private readonly ITipoService _tipoService;

    public [NombreEntidad]Controller(
        I[NombreEntidad]Service service,
        ITipoService tipoService)
    {
        _service = service;
        _tipoService = tipoService;
    }

    // GET: /[NombreEntidad]/Crear
    public async Task<IActionResult> Crear()
    {
        var model = new [NombreEntidad]FormViewModel();
        await CargarCombos(model);
        return View(model);
    }

    // POST: /[NombreEntidad]/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([NombreEntidad]FormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombos(model);
            return View(model);
        }

        try
        {
            await _service.CreateAsync(model, GetUsuarioId());
            TempData["Mensaje"] = "Registro creado correctamente";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar: " + ex.Message);
            await CargarCombos(model);
            return View(model);
        }
    }

    // GET: /[NombreEntidad]/Editar/{guid}
    public async Task<IActionResult> Editar(string guid)
    {
        var entity = await _service.GetByGuidAsync(guid);
        if (entity == null)
            return NotFound();

        var model = MapToViewModel(entity);
        await CargarCombos(model);
        return View(model);
    }

    // POST: /[NombreEntidad]/Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar([NombreEntidad]FormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombos(model);
            return View(model);
        }

        try
        {
            await _service.UpdateByGuidAsync(model.GuidRegistro!, model, GetUsuarioId());
            TempData["Mensaje"] = "Registro actualizado correctamente";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
            await CargarCombos(model);
            return View(model);
        }
    }

    // GET: /[NombreEntidad]/Detalle/{guid}
    public async Task<IActionResult> Detalle(string guid)
    {
        var entity = await _service.GetByGuidAsync(guid);
        if (entity == null)
            return NotFound();

        return View(entity);
    }

    // POST: /[NombreEntidad]/Guardar (AJAX)
    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] [NombreEntidad]FormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errores });
        }

        try
        {
            if (string.IsNullOrEmpty(model.GuidRegistro))
            {
                await _service.CreateAsync(model, GetUsuarioId());
            }
            else
            {
                await _service.UpdateByGuidAsync(model.GuidRegistro, model, GetUsuarioId());
            }

            return Json(new { success = true, mensaje = "Guardado correctamente" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errores = new[] { ex.Message } });
        }
    }

    private async Task CargarCombos([NombreEntidad]FormViewModel model)
    {
        var tipos = await _tipoService.GetAllActivosAsync();
        model.TiposDisponibles = tipos.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Descripcion
        }).ToList();
    }
}
```

### 4. Vista de Formulario (Crear.cshtml / Editar.cshtml)

```html
@model [NombreEntidad]FormViewModel
@{
    ViewData["Title"] = Model.GuidRegistro == null ? "Nuevo Registro" : "Editar Registro";
    var esEdicion = !string.IsNullOrEmpty(Model.GuidRegistro);
}

<div class="container-fluid py-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h4 class="mb-0">
            <i class="bi bi-@(esEdicion ? "pencil" : "plus-circle") me-2"></i>
            @ViewData["Title"]
        </h4>
        <a href="@Url.Action("Index")" class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Volver
        </a>
    </div>

    <!-- Formulario -->
    <div class="card shadow-sm">
        <div class="card-body">
            <form id="formPrincipal" asp-action="@(esEdicion ? "Editar" : "Crear")" method="post">
                @Html.AntiForgeryToken()
                <input type="hidden" asp-for="GuidRegistro" />

                <!-- Mensaje de campos obligatorios -->
                <p class="text-muted small mb-4">
                    Los campos marcados con <span class="text-danger">*</span> son obligatorios.
                </p>

                <div class="row">
                    <!-- Campo texto requerido -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="Nombre" class="form-label">
                            Nombre <span class="text-danger">*</span>
                        </label>
                        <input asp-for="Nombre" class="form-control" maxlength="100" />
                        <span asp-validation-for="Nombre" class="text-danger small"></span>
                    </div>

                    <!-- Campo email requerido -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="Email" class="form-label">
                            Correo Electronico <span class="text-danger">*</span>
                        </label>
                        <input asp-for="Email" type="email" class="form-control" />
                        <span asp-validation-for="Email" class="text-danger small"></span>
                    </div>

                    <!-- Campo telefono opcional -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="Telefono" class="form-label">Telefono</label>
                        <input asp-for="Telefono" class="form-control" maxlength="9"
                               placeholder="Ej: 987654321" />
                        <span asp-validation-for="Telefono" class="text-danger small"></span>
                    </div>

                    <!-- Combo simple (pocos valores) -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="IdTipo" class="form-label">
                            Tipo <span class="text-danger">*</span>
                        </label>
                        <select asp-for="IdTipo" asp-items="Model.TiposDisponibles"
                                class="form-select">
                            <option value="">-- Seleccione --</option>
                        </select>
                        <span asp-validation-for="IdTipo" class="text-danger small"></span>
                    </div>

                    <!-- Combo con Select2 (muchos valores) -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="IdEntidad" class="form-label">
                            Entidad <span class="text-danger">*</span>
                        </label>
                        <select asp-for="IdEntidad" asp-items="Model.EntidadesDisponibles"
                                class="form-select select2-entidad">
                            <option value="">-- Buscar entidad --</option>
                        </select>
                        <span asp-validation-for="IdEntidad" class="text-danger small"></span>
                    </div>

                    <!-- Campo numerico -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="Monto" class="form-label">Monto</label>
                        <div class="input-group">
                            <span class="input-group-text">S/</span>
                            <input asp-for="Monto" type="number" step="0.01" min="0"
                                   class="form-control" />
                        </div>
                        <span asp-validation-for="Monto" class="text-danger small"></span>
                    </div>

                    <!-- Campo fecha -->
                    <div class="col-md-6 mb-3">
                        <label asp-for="FechaEmision" class="form-label">
                            Fecha de Emision <span class="text-danger">*</span>
                        </label>
                        <input asp-for="FechaEmision" type="date" class="form-control" />
                        <span asp-validation-for="FechaEmision" class="text-danger small"></span>
                    </div>

                    <!-- Textarea -->
                    <div class="col-12 mb-3">
                        <label asp-for="Observaciones" class="form-label">Observaciones</label>
                        <textarea asp-for="Observaciones" class="form-control" rows="3"
                                  maxlength="500"></textarea>
                        <span asp-validation-for="Observaciones" class="text-danger small"></span>
                    </div>

                    <!-- Campo solo lectura -->
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Codigo</label>
                        <input value="@Model.Codigo" class="form-control" readonly
                               style="background-color: var(--bg-main);" />
                    </div>
                </div>

                <!-- Botones de accion -->
                <hr class="my-4" />
                <div class="d-flex justify-content-end gap-2">
                    <a href="@Url.Action("Index")" class="btn btn-secondary">
                        <i class="bi bi-x-lg me-1"></i>Cancelar
                    </a>
                    <button type="submit" class="btn btn-primary" id="btnGuardar">
                        <i class="bi bi-check-lg me-1"></i>Guardar
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Styles {
    <!-- Select2 (solo si se usa) -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" rel="stylesheet" />

    <style>
        /* Campos readonly */
        .form-control[readonly] {
            background-color: var(--bg-main) !important;
        }

        /* Select2 altura consistente */
        .select2-container .select2-selection--single {
            height: 38px;
            padding: 5px 10px;
        }

        /* Asterisco de campo requerido */
        .text-danger {
            color: #dc3545 !important;
        }
    </style>
}

@section Scripts {
    <!-- jQuery Validation -->
    <partial name="_ValidationScriptsPartial" />

    <!-- Select2 (solo si se usa) -->
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Configuracion
        const config = {
            colores: {
                primary: '#3498db',
                warning: '#f26522',
                success: '#28a745',
                danger: '#dc3545'
            }
        };

        // Inicializacion
        document.addEventListener('DOMContentLoaded', () => {
            inicializarSelect2();
            configurarValidacion();
            configurarFormulario();
            mostrarMensajeTempData();
        });

        // Inicializar Select2 para combos con muchos valores
        function inicializarSelect2() {
            $('.select2-entidad').select2({
                theme: 'bootstrap-5',
                placeholder: 'Buscar entidad...',
                allowClear: true,
                language: {
                    noResults: () => 'No se encontraron resultados',
                    searching: () => 'Buscando...',
                    inputTooShort: () => 'Escriba al menos 2 caracteres'
                },
                minimumInputLength: 2
            });

            // Select2 con busqueda AJAX (para listas muy grandes)
            $('.select2-ajax').select2({
                theme: 'bootstrap-5',
                placeholder: 'Buscar...',
                allowClear: true,
                ajax: {
                    url: '@Url.Action("BuscarEntidades")',
                    dataType: 'json',
                    delay: 300,
                    data: (params) => ({
                        term: params.term,
                        page: params.page || 1
                    }),
                    processResults: (data, params) => ({
                        results: data.items,
                        pagination: { more: data.hasMore }
                    }),
                    cache: true
                },
                minimumInputLength: 2
            });
        }

        // Configurar validacion personalizada
        function configurarValidacion() {
            // Deshabilitar validacion HTML5 nativa
            document.getElementById('formPrincipal').setAttribute('novalidate', 'true');
        }

        // Configurar envio del formulario
        function configurarFormulario() {
            const form = document.getElementById('formPrincipal');

            form.addEventListener('submit', async (e) => {
                e.preventDefault();

                // Validar campos
                const errores = validarFormulario();

                if (errores.length > 0) {
                    mostrarErroresValidacion(errores);
                    return;
                }

                // Confirmar guardado
                const confirmado = await confirmarGuardado();
                if (!confirmado) return;

                // Mostrar loading
                mostrarLoading();

                // Enviar formulario
                form.submit();
            });
        }

        // Validar formulario y retornar lista de errores
        function validarFormulario() {
            const errores = [];
            const form = document.getElementById('formPrincipal');

            // Obtener campos requeridos
            const camposRequeridos = [
                { id: 'Nombre', label: 'Nombre' },
                { id: 'Email', label: 'Correo Electronico' },
                { id: 'IdTipo', label: 'Tipo' },
                { id: 'FechaEmision', label: 'Fecha de Emision' }
            ];

            // Validar campos requeridos
            camposRequeridos.forEach(campo => {
                const elemento = document.getElementById(campo.id);
                if (elemento && !elemento.value.trim()) {
                    errores.push(`El campo "${campo.label}" es requerido`);
                    marcarCampoError(elemento);
                } else if (elemento) {
                    quitarCampoError(elemento);
                }
            });

            // Validar formato email
            const emailInput = document.getElementById('Email');
            if (emailInput && emailInput.value) {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(emailInput.value)) {
                    errores.push('El correo electronico no tiene un formato valido');
                    marcarCampoError(emailInput);
                }
            }

            // Validar telefono (si tiene valor)
            const telefonoInput = document.getElementById('Telefono');
            if (telefonoInput && telefonoInput.value) {
                const telefonoRegex = /^\d{9}$/;
                if (!telefonoRegex.test(telefonoInput.value)) {
                    errores.push('El telefono debe tener 9 digitos');
                    marcarCampoError(telefonoInput);
                }
            }

            // Validar monto (si tiene valor)
            const montoInput = document.getElementById('Monto');
            if (montoInput && montoInput.value) {
                const monto = parseFloat(montoInput.value);
                if (isNaN(monto) || monto < 0) {
                    errores.push('El monto debe ser un numero mayor o igual a 0');
                    marcarCampoError(montoInput);
                }
            }

            return errores;
        }

        // Marcar campo con error
        function marcarCampoError(elemento) {
            elemento.classList.add('is-invalid');
            elemento.classList.remove('is-valid');
        }

        // Quitar marca de error
        function quitarCampoError(elemento) {
            elemento.classList.remove('is-invalid');
            elemento.classList.add('is-valid');
        }

        // Mostrar errores de validacion con SweetAlert2
        function mostrarErroresValidacion(errores) {
            const listaHtml = errores.map(e => `<li>${e}</li>`).join('');

            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: `
                    <p class="mb-2">Por favor complete los siguientes campos:</p>
                    <ul style="text-align: left; padding-left: 20px;">
                        ${listaHtml}
                    </ul>
                `,
                confirmButtonColor: config.colores.warning,
                confirmButtonText: 'Entendido'
            });
        }

        // Confirmar guardado
        async function confirmarGuardado() {
            const result = await Swal.fire({
                title: '¿Guardar cambios?',
                text: 'Se guardaran los datos ingresados',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: config.colores.success,
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Si, guardar',
                cancelButtonText: 'Cancelar'
            });

            return result.isConfirmed;
        }

        // Mostrar loading mientras se procesa
        function mostrarLoading() {
            Swal.fire({
                title: 'Guardando...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                allowEscapeKey: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
        }

        // Mostrar mensaje de TempData (si existe)
        function mostrarMensajeTempData() {
            @if (TempData["Mensaje"] != null)
            {
                <text>
                const mensaje = '@TempData["Mensaje"]';
                const tipo = '@TempData["TipoMensaje"]' || 'success';

                Swal.fire({
                    icon: tipo,
                    title: tipo === 'success' ? 'Exito' : 'Atencion',
                    text: mensaje,
                    confirmButtonColor: tipo === 'success' ? config.colores.success : config.colores.warning,
                    timer: 3000,
                    timerProgressBar: true
                });
                </text>
            }
        }

        // Guardar via AJAX (alternativa al submit tradicional)
        async function guardarAjax() {
            const errores = validarFormulario();

            if (errores.length > 0) {
                mostrarErroresValidacion(errores);
                return;
            }

            const confirmado = await confirmarGuardado();
            if (!confirmado) return;

            mostrarLoading();

            const form = document.getElementById('formPrincipal');
            const formData = new FormData(form);
            const datos = Object.fromEntries(formData.entries());

            try {
                const response = await fetch('@Url.Action("Guardar")', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(datos)
                });

                const result = await response.json();

                if (result.success) {
                    await Swal.fire({
                        icon: 'success',
                        title: 'Guardado',
                        text: result.mensaje || 'Registro guardado correctamente',
                        confirmButtonColor: config.colores.success,
                        timer: 2000,
                        timerProgressBar: true
                    });

                    window.location.href = '@Url.Action("Index")';
                } else {
                    mostrarErroresValidacion(result.errores || ['Error al guardar']);
                }
            } catch (error) {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Ocurrio un error al procesar la solicitud',
                    confirmButtonColor: config.colores.danger
                });
            }
        }

        // Limpiar formulario
        function limpiarFormulario() {
            const form = document.getElementById('formPrincipal');
            form.reset();

            // Limpiar Select2
            $('.select2-entidad').val(null).trigger('change');

            // Quitar marcas de validacion
            form.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
                el.classList.remove('is-invalid', 'is-valid');
            });
        }

        // Confirmar cancelacion si hay cambios
        async function confirmarCancelacion() {
            const result = await Swal.fire({
                title: '¿Cancelar?',
                text: 'Los cambios no guardados se perderan',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: config.colores.danger,
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Si, cancelar',
                cancelButtonText: 'Continuar editando'
            });

            if (result.isConfirmed) {
                window.location.href = '@Url.Action("Index")';
            }
        }
    </script>
}
```

### 5. Vista de Solo Lectura (Detalle.cshtml)

```html
@model [NombreEntidad]ResponseDto
@{
    ViewData["Title"] = "Detalle del Registro";
}

<div class="container-fluid py-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h4 class="mb-0">
            <i class="bi bi-eye me-2"></i>@ViewData["Title"]
        </h4>
        <div>
            <a href="@Url.Action("Editar", new { guid = Model.GuidRegistro })"
               class="btn btn-warning me-2">
                <i class="bi bi-pencil me-1"></i>Editar
            </a>
            <a href="@Url.Action("Index")" class="btn btn-outline-secondary">
                <i class="bi bi-arrow-left me-1"></i>Volver
            </a>
        </div>
    </div>

    <!-- Contenido -->
    <div class="card shadow-sm">
        <div class="card-body">
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label class="form-label text-muted small">Nombre</label>
                    <p class="form-control-plaintext fw-medium">@Model.Nombre</p>
                </div>

                <div class="col-md-6 mb-3">
                    <label class="form-label text-muted small">Correo Electronico</label>
                    <p class="form-control-plaintext">@Model.Email</p>
                </div>

                <div class="col-md-6 mb-3">
                    <label class="form-label text-muted small">Estado</label>
                    <p class="form-control-plaintext">
                        @if (Model.Activo == 1)
                        {
                            <span class="badge bg-success">Activo</span>
                        }
                        else
                        {
                            <span class="badge bg-secondary">Inactivo</span>
                        }
                    </p>
                </div>

                <div class="col-md-6 mb-3">
                    <label class="form-label text-muted small">Fecha de Creacion</label>
                    <p class="form-control-plaintext">
                        @Model.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                    </p>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## Reglas y Buenas Practicas

### Campos Obligatorios

1. **Marca visual con asterisco**
   ```html
   <label class="form-label">
       Nombre <span class="text-danger">*</span>
   </label>
   ```

2. **Mensaje informativo al inicio del formulario**
   ```html
   <p class="text-muted small mb-4">
       Los campos marcados con <span class="text-danger">*</span> son obligatorios.
   </p>
   ```

3. **Color del asterisco**: Usar `#dc3545` (danger de Bootstrap)

### Validacion JavaScript

1. **Validar antes de enviar**
   ```javascript
   form.addEventListener('submit', (e) => {
       e.preventDefault();
       const errores = validarFormulario();
       if (errores.length > 0) {
           mostrarErroresValidacion(errores);
           return;
       }
       form.submit();
   });
   ```

2. **Tipos de validacion comunes**
   ```javascript
   // Requerido
   if (!valor.trim()) errores.push('Campo requerido');

   // Email
   const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
   if (!emailRegex.test(email)) errores.push('Email invalido');

   // Telefono Peru (9 digitos)
   const telRegex = /^\d{9}$/;
   if (!telRegex.test(telefono)) errores.push('Telefono invalido');

   // RUC Peru (11 digitos)
   const rucRegex = /^\d{11}$/;
   if (!rucRegex.test(ruc)) errores.push('RUC invalido');

   // Numero positivo
   if (isNaN(monto) || monto <= 0) errores.push('Monto invalido');

   // Fecha no futura
   if (new Date(fecha) > new Date()) errores.push('Fecha no puede ser futura');
   ```

3. **Marcar campos con error visualmente**
   ```javascript
   elemento.classList.add('is-invalid');  // Borde rojo
   elemento.classList.add('is-valid');    // Borde verde
   ```

### SweetAlert2 para Validaciones

```javascript
// Mostrar errores de validacion
function mostrarErroresValidacion(errores) {
    const listaHtml = errores.map(e => `<li>${e}</li>`).join('');

    Swal.fire({
        icon: 'warning',
        title: 'Campos requeridos',
        html: `
            <p class="mb-2">Por favor complete los siguientes campos:</p>
            <ul style="text-align: left; padding-left: 20px;">
                ${listaHtml}
            </ul>
        `,
        confirmButtonColor: '#f26522',  // Color San Pablo
        confirmButtonText: 'Entendido'
    });
}

// Confirmar accion
async function confirmarAccion(titulo, mensaje) {
    const result = await Swal.fire({
        title: titulo,
        text: mensaje,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Si, continuar',
        cancelButtonText: 'Cancelar'
    });
    return result.isConfirmed;
}

// Loading
Swal.fire({
    title: 'Procesando...',
    allowOutsideClick: false,
    showConfirmButton: false,
    didOpen: () => Swal.showLoading()
});
```

### Select2 para Combos Grandes

1. **Cuando usar Select2**
   - Mas de 20-30 opciones
   - Necesidad de busqueda/filtrado
   - Carga dinamica de datos

2. **Inicializacion basica**
   ```javascript
   $('.select2-basico').select2({
       theme: 'bootstrap-5',
       placeholder: 'Seleccione...',
       allowClear: true,
       language: 'es'
   });
   ```

3. **Con busqueda AJAX**
   ```javascript
   $('.select2-ajax').select2({
       theme: 'bootstrap-5',
       placeholder: 'Buscar...',
       ajax: {
           url: '/api/buscar',
           dataType: 'json',
           delay: 300,
           data: (params) => ({ term: params.term }),
           processResults: (data) => ({ results: data }),
           cache: true
       },
       minimumInputLength: 2
   });
   ```

4. **Establecer valor programaticamente**
   ```javascript
   // Crear opcion y seleccionar
   const option = new Option('Texto', 'valor', true, true);
   $('#miSelect').append(option).trigger('change');
   ```

### Campos de Solo Lectura

```html
<!-- Input readonly -->
<input class="form-control" value="@Model.Codigo" readonly
       style="background-color: var(--bg-main);" />

<!-- O usar form-control-plaintext para solo texto -->
<p class="form-control-plaintext">@Model.Codigo</p>
```

### Seguridad

1. **Usar GUID en lugar de ID**
   - En URLs: `/Editar/{guid}` no `/Editar/{id}`
   - En hidden fields: `<input type="hidden" asp-for="GuidRegistro" />`
   - En AJAX: `data-guid` no `data-id`

2. **Token CSRF**
   ```html
   @Html.AntiForgeryToken()
   ```
   ```javascript
   headers: {
       'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
   }
   ```

3. **Validar en servidor siempre**
   - Nunca confiar solo en validacion JS
   - ModelState.IsValid en controlador

---

## Checklist de Implementacion

### Formulario
- [ ] Campos obligatorios marcados con asterisco rojo (*)
- [ ] Mensaje "campos obligatorios" al inicio
- [ ] Validacion JavaScript antes de submit
- [ ] Errores mostrados con SweetAlert2
- [ ] Campos con error tienen borde rojo (is-invalid)
- [ ] Token CSRF incluido
- [ ] GUID en hidden field (no ID)

### Combos
- [ ] Select simple para pocos valores (<20)
- [ ] Select2 para muchos valores (>20)
- [ ] Select2 con AJAX para listas muy grandes
- [ ] Opcion "Seleccione" por defecto

### UX
- [ ] Boton "Volver" visible
- [ ] Confirmar antes de guardar (opcional)
- [ ] Confirmar antes de cancelar si hay cambios
- [ ] Loading mientras se procesa
- [ ] Mensaje de exito/error despues de guardar

### Campos readonly
- [ ] Fondo gris (var(--bg-main))
- [ ] No editables

---

## Ejemplo de Uso

```
Crea una pantalla de formulario para crear/editar Facturas en el proyecto
SHM.AppWebCompaniaMedica.

Campos del formulario:
- Serie (requerido, 4 caracteres)
- Numero (requerido, numerico)
- Fecha de Emision (requerido, fecha)
- Entidad Medica (requerido, combo con Select2 - muchos valores)
- Tipo de Comprobante (requerido, combo simple - pocos valores)
- Monto Subtotal (requerido, numerico)
- Monto IGV (requerido, numerico)
- Monto Total (requerido, numerico, calculado)
- Observaciones (opcional, textarea)

El Monto Total debe calcularse automaticamente.
Usar Select2 para el combo de Entidad Medica.

Seguir las especificaciones del prompt_pantalla_detalle.md
```

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
