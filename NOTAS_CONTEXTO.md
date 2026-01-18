# Notas de Contexto - Proyecto SHM (Sistema de Honorarios Medicos)

## Informacion General del Proyecto

**Nombre:** SHM - Sistema de Honorarios Medicos
**Cliente:** San Pablo
**Base de Datos:** Oracle
**Framework:** ASP.NET Core 8 MVC
**ORM:** Dapper
**Arquitectura:** Clean Architecture (Domain, Application, Infrastructure, Web)

---

## Estructura de Proyectos

```
src/
├── SHM.AppDomain/                    # Entidades, DTOs, Interfaces
├── SHM.AppApplication/               # Servicios de aplicacion
├── SHM.AppInfrastructure/            # Repositorios (Dapper + Oracle)
├── SHM.AppWebCompaniaMedica/         # Web MVC para Companias Medicas (proveedores)
├── SHM.AppWebHonorarioMedico/        # Web MVC para gestion interna
└── SHM.AppApiHonorarioMedico/        # API REST
```

---

## Aplicacion Actual: SHM.AppWebCompaniaMedica

Portal web para que las **Companias Medicas (proveedores)** puedan:
- Iniciar sesion
- Subir facturas con archivos adjuntos
- Ver facturas enviadas
- Ver detalle de facturas
- Gestionar su perfil

### Autenticacion
- Cookie Authentication con claims
- Login: `/Auth/Login`
- Claim principal: `ClaimTypes.NameIdentifier` contiene el `IdUsuario`

---

## Paginas Implementadas Recientemente

### 1. Subir Factura (`/Facturas/Subir`)
- Formulario para subir facturas con archivos XML y PDF
- Validaciones con SweetAlert2 (estilo con lista de errores)
- Color del boton de warning: `#f26522` (naranja)
- Color del boton de error: `#dc3545` (rojo)

### 2. Facturas Enviadas (`/Facturas/Enviadas`)
- Lista paginada de facturas enviadas
- Carga AJAX con partial views

### 3. Detalle de Factura (`/Facturas/Detalle/{guid}`)
- Muestra detalle de factura usando GUID (no ID)
- Descarga de archivos adjuntos por GUID

### 4. Perfil de Usuario (`/Usuario/Perfil`) - IMPLEMENTADO HOY
- Muestra informacion del usuario autenticado
- Muestra informacion de la entidad medica asociada
- Muestra cuentas bancarias (solo lectura)
- Permite editar informacion personal
- Permite cambiar contrasena

---

## Detalles de Implementacion - Pagina Perfil

### Archivos Modificados/Creados

1. **UsuarioViewModels.cs** (`Models/UsuarioViewModels.cs`)
   - `PerfilViewModel`: Datos del usuario, entidad medica y cuentas bancarias
   - `CuentaBancariaViewModel`: NombreBanco, CuentaCorriente, CuentaCci, Moneda
   - `ActualizarPerfilViewModel`: Datos para actualizar perfil
   - `CambiarPasswordViewModel`: Datos para cambio de contrasena

2. **UsuarioController.cs** (`Controllers/UsuarioController.cs`)
   - Inyecta: `IUsuarioService`, `IEntidadMedicaService`, `IEntidadCuentaBancariaService`, `IBancoService`
   - Actions: `Perfil()`, `ActualizarPerfil()`, `CambiarPassword()`

3. **Perfil.cshtml** (`Views/Usuario/Perfil.cshtml`)
   - Columna izquierda: Avatar con iniciales, datos resumidos
   - Columna derecha: Formularios editables

4. **Program.cs** - Servicios registrados:
   - `IEntidadMedicaRepository` / `EntidadMedicaRepository`
   - `IEntidadCuentaBancariaRepository` / `EntidadCuentaBancariaRepository`
   - `IBancoRepository` / `BancoRepository`
   - `IEntidadMedicaService` / `EntidadMedicaService`
   - `IEntidadCuentaBancariaService` / `EntidadCuentaBancariaService`
   - `IBancoService` / `BancoService`

### Secciones de la Pagina Perfil

1. **Perfil Principal (col-lg-4)**
   - Avatar circular con iniciales del usuario
   - Nombre completo y email
   - Badge "Cuenta Verificada"
   - Tarjetas informativas: Entidad Medica, RUC, Miembro desde, Documento

2. **Entidad Medica (col-lg-4)** - Solo si tiene entidad asociada
   - Direccion, Telefono, Celular de la entidad

3. **Informacion Personal (col-lg-8)**
   - Campos editables (al hacer clic en "Editar")
   - Campos obligatorios (*): Nombres, Apellido Paterno, Apellido Materno, Email, Celular
   - Campos opcionales: Numero de Documento, Telefono, Cargo
   - Estilo readonly: `background-color: var(--bg-main)`

4. **Informacion Bancaria (col-lg-8)** - Solo lectura
   - Si tiene cuentas: Muestra Banco, Moneda, Cuenta Corriente, Cuenta CCI
   - Si no tiene cuentas: Muestra campos vacios con mensaje de advertencia amarillo
   - Mensaje informativo para contactar administrador

5. **Seguridad (col-lg-8)**
   - Formulario para cambiar contrasena
   - Campos obligatorios (*): Contrasena Actual, Nueva Contrasena, Confirmar
   - Validaciones: campos requeridos, coincidencia, minimo 8 caracteres

### Estilos de Validacion SweetAlert2

```javascript
// Campos requeridos (warning - naranja)
Swal.fire({
    icon: 'warning',
    title: 'Campos Requeridos',
    html: '<ul style="text-align: left; margin: 0; padding-left: 1.5rem;">' +
          errores.map(function(err) { return '<li>' + err + '</li>'; }).join('') +
          '</ul>',
    confirmButtonText: 'Entendido',
    confirmButtonColor: '#f26522'
});

// Error de validacion (error - rojo)
Swal.fire({
    icon: 'error',
    title: 'Error de Validacion',
    text: 'Mensaje de error',
    confirmButtonText: 'Entendido',
    confirmButtonColor: '#dc3545'
});

// Exito (success - verde)
Swal.fire({
    icon: 'success',
    title: 'Exito',
    text: 'Mensaje de exito',
    confirmButtonColor: '#28a745'
});
```

---

## Campos Agregados a la Base de Datos (Sesion Anterior)

```sql
ALTER TABLE SHM_SEG_USUARIO ADD TELEFONO NVARCHAR2(20);
ALTER TABLE SHM_SEG_USUARIO ADD CARGO NVARCHAR2(120);
ALTER TABLE SHM_ENTIDAD_MEDICA ADD DIRECCION NVARCHAR2(300);
```

Estos campos fueron agregados en todas las capas:
- Entities
- DTOs (Response, Create, Update)
- Repositories (SELECT, INSERT, UPDATE)
- Services

---

## Patron de Seguridad: Uso de GUID

**IMPORTANTE:** No exponer IDs numericos al cliente. Usar siempre `GuidRegistro` para:
- URLs de detalle
- Descargas de archivos
- Cualquier operacion desde el frontend

Ejemplo:
```csharp
// Correcto
public async Task<IActionResult> Detalle(string guid)
public async Task<IActionResult> DescargarArchivo(string guid)

// Incorrecto - NO USAR
public async Task<IActionResult> Detalle(int id)
```

---

## Variables CSS del Proyecto

```css
--accent: #2596be;           /* Color principal (azul) */
--accent-dark: #1a7a9e;      /* Color principal oscuro */
--bg-main: #f5f5f5;          /* Fondo principal / campos readonly */
--text-secondary: #6c757d;   /* Texto secundario */
--border: #dee2e6;           /* Bordes */
```

---

## Proximos Pasos Sugeridos

1. Implementar validacion de email en backend
2. Agregar validacion de formato de celular
3. Implementar notificaciones por email al cambiar contrasena
4. Agregar historial de cambios en perfil (bitacora)

---

## Notas Tecnicas

- Razor Runtime Compilation habilitado en desarrollo
- NLog configurado para logging
- Todas las operaciones async/await
- Manejo de errores con try-catch y logging

---

*Ultima actualizacion: 18 de Enero 2026*
