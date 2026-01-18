# Prompt para Crear Pantalla Tipo Lista

## Contexto Previo
Lee el archivo `NOTAS_CONTEXTO.md` y `src/README.md` para entender el contexto del proyecto.

## Solicitud

Crea una pantalla tipo lista para la entidad **[NombreEntidad]** en el proyecto **[NombreProyectoWeb]**.

---

## Especificaciones Tecnicas

### 1. Arquitectura MVC

```
Controllers/
└── [NombreEntidad]Controller.cs    # Controlador con acciones

Views/[NombreEntidad]/
├── Index.cshtml                     # Vista principal con lista
├── _ListaPartial.cshtml             # Partial view para la tabla (opcional)
└── Detalle.cshtml                   # Vista de detalle (si aplica)

ViewModels/
└── [NombreEntidad]ViewModels.cs     # ViewModels especificos
```

### 2. Controlador

```csharp
[Authorize]
public class [NombreEntidad]Controller : BaseController
{
    private readonly I[NombreEntidad]Service _service;

    public [NombreEntidad]Controller(I[NombreEntidad]Service service)
    {
        _service = service;
    }

    // GET: /[NombreEntidad]
    public IActionResult Index()
    {
        return View();
    }

    // GET: /[NombreEntidad]/ObtenerLista (AJAX)
    [HttpGet]
    public async Task<IActionResult> ObtenerLista()
    {
        var items = await _service.GetAllAsync();
        return Json(new { data = items });
    }

    // GET: /[NombreEntidad]/Detalle/{guid}
    public async Task<IActionResult> Detalle(string guid)
    {
        var item = await _service.GetByGuidAsync(guid);
        if (item == null)
            return NotFound();
        return View(item);
    }

    // POST: /[NombreEntidad]/Eliminar (AJAX)
    [HttpPost]
    public async Task<IActionResult> Eliminar([FromBody] EliminarRequest request)
    {
        // Usar GUID, nunca ID
        var result = await _service.DeleteByGuidAsync(request.Guid, GetUsuarioId());
        return Json(new { success = result });
    }
}
```

### 3. Vista Principal (Index.cshtml)

```html
@{
    ViewData["Title"] = "Lista de [NombreEntidad]";
}

<div class="container-fluid py-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h4 class="mb-0">
            <i class="bi bi-list-ul me-2"></i>Lista de [NombreEntidad]
        </h4>
        <a href="@Url.Action("Crear")" class="btn btn-primary">
            <i class="bi bi-plus-lg me-1"></i>Nuevo
        </a>
    </div>

    <!-- Tabla -->
    <div class="card shadow-sm">
        <div class="card-body">
            <table id="tablaLista" class="table table-striped table-hover" style="width:100%">
                <thead>
                    <tr>
                        <th>Columna 1</th>
                        <th>Columna 2</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    <!-- Datos cargados via AJAX -->
                </tbody>
            </table>
        </div>
    </div>
</div>

<!-- Modal de Confirmacion -->
<div class="modal fade" id="modalConfirmar" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Confirmar Accion</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <p id="mensajeConfirmacion"></p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-danger" id="btnConfirmarAccion">Confirmar</button>
            </div>
        </div>
    </div>
</div>

@section Styles {
    <link rel="stylesheet" href="https://cdn.datatables.net/1.13.7/css/dataTables.bootstrap5.min.css">
}

@section Scripts {
    <script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.7/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Variables globales
        let tabla = null;
        let guidSeleccionado = null;
        const modalConfirmar = new bootstrap.Modal(document.getElementById('modalConfirmar'));

        // Inicializacion cuando el DOM esta listo
        document.addEventListener('DOMContentLoaded', () => {
            inicializarTabla();
            configurarEventos();
        });

        // Inicializar DataTable con AJAX
        function inicializarTabla() {
            tabla = $('#tablaLista').DataTable({
                ajax: {
                    url: '@Url.Action("ObtenerLista")',
                    dataSrc: 'data',
                    error: (xhr, error, thrown) => {
                        mostrarError('Error al cargar los datos');
                        console.error('Error DataTable:', error);
                    }
                },
                columns: [
                    { data: 'columna1' },
                    { data: 'columna2' },
                    {
                        data: 'activo',
                        render: (data) => {
                            const clase = data === 1 ? 'bg-success' : 'bg-secondary';
                            const texto = data === 1 ? 'Activo' : 'Inactivo';
                            return `<span class="badge ${clase}">${texto}</span>`;
                        }
                    },
                    {
                        data: 'guidRegistro',
                        orderable: false,
                        render: (data, type, row) => {
                            return `
                                <div class="btn-group btn-group-sm">
                                    <a href="@Url.Action("Detalle")/${data}"
                                       class="btn btn-outline-primary" title="Ver detalle">
                                        <i class="bi bi-eye"></i>
                                    </a>
                                    <a href="@Url.Action("Editar")/${data}"
                                       class="btn btn-outline-warning" title="Editar">
                                        <i class="bi bi-pencil"></i>
                                    </a>
                                    <button type="button"
                                            class="btn btn-outline-danger btn-eliminar"
                                            data-guid="${data}"
                                            title="Eliminar">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </div>
                            `;
                        }
                    }
                ],
                language: {
                    url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
                },
                order: [[0, 'asc']],
                responsive: true,
                processing: true
            });
        }

        // Configurar eventos
        function configurarEventos() {
            // Delegacion de eventos para botones de eliminar
            document.getElementById('tablaLista').addEventListener('click', (e) => {
                const btnEliminar = e.target.closest('.btn-eliminar');
                if (btnEliminar) {
                    guidSeleccionado = btnEliminar.dataset.guid;
                    document.getElementById('mensajeConfirmacion').textContent =
                        '¿Esta seguro que desea eliminar este registro?';
                    modalConfirmar.show();
                }
            });

            // Confirmar eliminacion
            document.getElementById('btnConfirmarAccion').addEventListener('click', async () => {
                if (guidSeleccionado) {
                    await eliminarRegistro(guidSeleccionado);
                    modalConfirmar.hide();
                    guidSeleccionado = null;
                }
            });
        }

        // Eliminar registro via AJAX
        async function eliminarRegistro(guid) {
            try {
                const response = await fetch('@Url.Action("Eliminar")', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify({ guid: guid })
                });

                const result = await response.json();

                if (result.success) {
                    mostrarExito('Registro eliminado correctamente');
                    tabla.ajax.reload(null, false); // Recargar sin resetear paginacion
                } else {
                    mostrarError(result.message || 'No se pudo eliminar el registro');
                }
            } catch (error) {
                console.error('Error:', error);
                mostrarError('Error al procesar la solicitud');
            }
        }

        // Recargar tabla
        function recargarTabla() {
            tabla.ajax.reload(null, false);
        }

        // Mostrar mensaje de exito con SweetAlert2
        function mostrarExito(mensaje) {
            Swal.fire({
                icon: 'success',
                title: 'Exito',
                text: mensaje,
                confirmButtonColor: '#28a745',
                timer: 2000,
                timerProgressBar: true
            });
        }

        // Mostrar mensaje de error con SweetAlert2
        function mostrarError(mensaje) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: mensaje,
                confirmButtonColor: '#dc3545'
            });
        }

        // Mostrar mensaje de advertencia con SweetAlert2
        function mostrarAdvertencia(mensaje) {
            Swal.fire({
                icon: 'warning',
                title: 'Atencion',
                text: mensaje,
                confirmButtonColor: '#f26522'
            });
        }

        // Confirmar accion con SweetAlert2
        async function confirmarAccion(mensaje, callback) {
            const result = await Swal.fire({
                title: '¿Esta seguro?',
                text: mensaje,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Si, continuar',
                cancelButtonText: 'Cancelar'
            });

            if (result.isConfirmed && callback) {
                await callback();
            }
        }
    </script>
}
```

---

## Reglas y Buenas Practicas

### Seguridad

1. **Nunca exponer IDs de base de datos**
   - Usar siempre `GuidRegistro` en URLs y data attributes
   - Los IDs internos solo se usan en backend

2. **Validar en servidor**
   - Aunque se valide en cliente, siempre validar en servidor
   - Verificar permisos del usuario antes de cada accion

3. **CSRF Protection**
   - Incluir `@Html.AntiForgeryToken()` en formularios
   - Enviar token en headers de peticiones AJAX

### JavaScript Moderno

1. **Usar ES6+**
   ```javascript
   // Preferir const y let sobre var
   const items = [];
   let contador = 0;

   // Arrow functions
   const procesar = (item) => item.valor * 2;

   // Template literals
   const html = `<div class="${clase}">${contenido}</div>`;

   // Async/await sobre callbacks
   const data = await fetchData();

   // Destructuring
   const { nombre, email } = usuario;
   ```

2. **Delegacion de eventos**
   ```javascript
   // En lugar de agregar listener a cada boton
   document.getElementById('contenedor').addEventListener('click', (e) => {
       const btn = e.target.closest('.btn-accion');
       if (btn) {
           // Manejar click
       }
   });
   ```

### AJAX con Fetch API

```javascript
// GET
const response = await fetch(url);
const data = await response.json();

// POST con JSON
const response = await fetch(url, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify(datos)
});

// POST con FormData
const formData = new FormData(formulario);
const response = await fetch(url, {
    method: 'POST',
    body: formData
});
```

### jQuery para Modales Bootstrap

```javascript
// Crear instancia del modal
const modal = new bootstrap.Modal(document.getElementById('miModal'));

// Mostrar modal
modal.show();

// Ocultar modal
modal.hide();

// Con jQuery (alternativa)
$('#miModal').modal('show');
$('#miModal').modal('hide');

// Eventos
$('#miModal').on('shown.bs.modal', function() {
    // Modal visible
});

$('#miModal').on('hidden.bs.modal', function() {
    // Modal oculto, limpiar formulario
    $(this).find('form')[0]?.reset();
});
```

### SweetAlert2 - Estilos del Proyecto

```javascript
// Colores corporativos
const colores = {
    primary: '#3498db',
    warning: '#f26522',    // Color San Pablo
    success: '#28a745',
    danger: '#dc3545'
};

// Alerta de exito
Swal.fire({
    icon: 'success',
    title: 'Operacion exitosa',
    text: 'El registro se guardo correctamente',
    confirmButtonColor: colores.success,
    timer: 2000,
    timerProgressBar: true
});

// Alerta de error
Swal.fire({
    icon: 'error',
    title: 'Error',
    text: 'No se pudo completar la operacion',
    confirmButtonColor: colores.danger
});

// Alerta de advertencia (campos requeridos)
Swal.fire({
    icon: 'warning',
    title: 'Campos requeridos',
    html: '<ul style="text-align:left;"><li>Campo 1</li><li>Campo 2</li></ul>',
    confirmButtonColor: colores.warning
});

// Confirmacion
const result = await Swal.fire({
    title: '¿Confirmar eliminacion?',
    text: 'Esta accion no se puede deshacer',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: colores.danger,
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'Si, eliminar',
    cancelButtonText: 'Cancelar'
});

if (result.isConfirmed) {
    // Proceder con eliminacion
}

// Toast (notificacion pequena)
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true
});

Toast.fire({
    icon: 'success',
    title: 'Guardado correctamente'
});
```

### DataTables - Configuracion Estandar

```javascript
$('#tabla').DataTable({
    ajax: {
        url: urlEndpoint,
        dataSrc: 'data'
    },
    columns: [
        { data: 'campo1' },
        { data: 'campo2' },
        {
            data: 'fecha',
            render: (data) => data ? new Date(data).toLocaleDateString('es-PE') : '-'
        },
        {
            data: 'monto',
            render: (data) => `S/ ${parseFloat(data).toFixed(2)}`
        },
        {
            data: 'guidRegistro',
            orderable: false,
            searchable: false,
            render: (data) => `<button data-guid="${data}">Accion</button>`
        }
    ],
    language: {
        url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
    },
    order: [[0, 'desc']],
    responsive: true,
    processing: true,
    pageLength: 10,
    lengthMenu: [[10, 25, 50, -1], [10, 25, 50, 'Todos']]
});

// Recargar datos
tabla.ajax.reload(null, false); // false = mantener paginacion actual
```

---

## Checklist de Implementacion

- [ ] Controlador hereda de `BaseController`
- [ ] Usa `[Authorize]` en el controlador
- [ ] Endpoints AJAX devuelven JSON
- [ ] Vista usa `@section Scripts` para JS
- [ ] DataTable carga datos via AJAX
- [ ] URLs usan GUID, no ID
- [ ] Botones de accion usan `data-guid`
- [ ] Confirmaciones con SweetAlert2
- [ ] Modales manejados con Bootstrap JS
- [ ] JavaScript usa ES6+ (const, let, arrow, async/await)
- [ ] Delegacion de eventos para elementos dinamicos
- [ ] Token CSRF en peticiones POST
- [ ] Mensajes de error amigables
- [ ] Tabla responsive

---

## Ejemplo de Uso

```
Crea una pantalla tipo lista para la entidad Factura en el proyecto
SHM.AppWebCompaniaMedica.

La lista debe mostrar:
- Numero de factura
- Fecha de emision
- Monto total
- Estado (badge con color)
- Acciones (ver detalle, descargar PDF)

Incluir filtros por:
- Rango de fechas
- Estado

Seguir las especificaciones del prompt_pantalla_lista.md
```

---

*Proyecto SHM - Sistema de Honorarios Medicos - Grupo San Pablo*
