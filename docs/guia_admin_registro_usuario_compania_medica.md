# Guía del Administrador
## Registro y Gestión de Usuarios de Compañías Médicas

**Versión:** 1.0
**Fecha:** Mayo 2026
**Cliente:** Grupo San Pablo
**Dirigido a:** Administradores del Portal Administrativo (SHM.AppWebHonorarioMedico)

---

## Introducción

Esta guía describe las tareas que realiza el administrador del sistema para crear y gestionar los usuarios de las compañías médicas en el Portal Administrativo de Honorarios Médicos.

Una vez que el administrador registra al usuario, el sistema genera automáticamente una contraseña temporal y envía un correo de bienvenida al usuario con sus credenciales de acceso.

| Tarea | Descripción |
|-------|-------------|
| Registrar usuario | Crear una nueva cuenta para un responsable de compañía médica |
| Editar usuario | Modificar datos, entidad médica, rol o estado |
| Reset de clave | Generar nueva contraseña temporal y reenviar credenciales |
| Desactivar usuario | Bloquear el acceso sin eliminar el registro |

---

## 1. Acceder al módulo de Usuarios Externos

En el menú lateral del Portal Administrativo, navegar a:

**Seguridad → Usuarios Externos**

<img src="images/adm_01_listado_usuarios.png" width="750"/>

*Figura 1: Listado de usuarios externos*

Se muestra la tabla con todos los usuarios externos registrados. Puede usar el campo de búsqueda para filtrar por nombre, login, email o entidad médica.

---

## 2. Registrar un nuevo usuario

Hacer clic en el botón **"+ Nuevo Usuario"** en la esquina superior derecha del panel.

<img src="images/adm_02_modal_nuevo_usuario.png" width="650"/>

*Figura 2: Formulario de nuevo usuario externo*

### Campos del formulario

| Campo | Obligatorio | Descripción |
|-------|:-----------:|-------------|
| **Login** | Sí | Nombre de usuario para acceder al portal. Debe ser único en el sistema. Se recomienda usar el formato `nombre.apellido` o el RUC de la compañía |
| **Email** | Sí | Correo electrónico al que se enviarán las credenciales. Debe ser válido y accesible por el usuario |
| **Nombres** | Sí | Nombre(s) del responsable de la compañía médica |
| **Apellido Paterno** | Sí | Primer apellido |
| **Apellido Materno** | No | Segundo apellido |
| **N° Documento** | No | DNI o CE del responsable |
| **Celular** | No | Número de celular de contacto |
| **Entidad Médica** | No | Compañía médica a la que pertenece el usuario. Buscar por nombre en el combo |
| **Rol** | Sí | Perfil de acceso. Para usuarios de compañías médicas seleccionar **"Compañía Médica"** (se preselecciona automáticamente) |

> **Login duplicado:** Si el login ya existe en el sistema, al guardar aparecerá un mensaje de error. Usar un login diferente e intentarlo nuevamente.

### Opción de envío de credenciales

<img src="images/adm_03_checkbox_enviar_correo.png" width="500"/>

*Figura 3: Opción de envío de credenciales por correo*

Al pie del formulario se encuentra:

> ☑ **Enviar credenciales por correo electrónico**

Esta casilla viene **marcada por defecto**.

| Opción | Resultado |
|--------|-----------|
| **Marcada** | El sistema envía automáticamente el correo de bienvenida con el login y la contraseña temporal |
| **Desmarcada** | La contraseña generada se muestra directamente en pantalla con opción de copiar. El administrador debe comunicarla al usuario por otro medio |

### Guardar el registro

Hacer clic en **"Guardar"**.

El sistema:
1. Valida que el login no esté en uso
2. Genera una contraseña temporal aleatoria de 8 caracteres
3. Crea el usuario en estado **Activo**
4. Envía el correo de bienvenida (si la opción está marcada)
5. Muestra una notificación de éxito

<img src="images/adm_04_notificacion_exito.png" width="400"/>

*Figura 4: Notificación de creación exitosa*

El nuevo usuario aparece en el listado de Usuarios Externos.

> Desde este momento, el usuario puede ingresar al Portal de Compañías Médicas con las credenciales recibidas por correo. En el primer ingreso, el sistema le solicitará cambiar su contraseña temporal.

---

## 3. Editar un usuario

Desde el listado, hacer clic en el ícono de **editar** de la fila correspondiente.

<img src="images/adm_05_modal_editar_usuario.png" width="650"/>

*Figura 5: Formulario de edición de usuario*

Se pueden modificar:
- Datos personales (nombre, apellidos, documento, celular)
- Correo electrónico
- Entidad médica asignada
- Rol
- Estado (Activo / Inactivo)

Hacer clic en **"Guardar"** para aplicar los cambios.

---

## 4. Resetear la contraseña (Reset de Clave)

Usar esta función cuando el usuario olvidó su contraseña o no puede acceder al sistema.

Desde el listado, hacer clic en el ícono de **Reset de Clave** de la fila correspondiente.

<img src="images/adm_06_modal_reset_clave.png" width="500"/>

*Figura 6: Modal de confirmación de reset de clave*

### Pasos

1. En el modal de confirmación, verificar que la opción **"Enviar credenciales por correo electrónico"** esté marcada
2. Hacer clic en **"Restablecer"**

El sistema:
- Genera una nueva contraseña temporal de 8 caracteres
- La envía al correo registrado del usuario (si la opción está marcada)
- En el próximo ingreso del usuario, el sistema le solicitará nuevamente el cambio de contraseña

> Si el administrador desmarca la opción de correo, la nueva contraseña se muestra en pantalla para comunicarla por otro medio.

---

## 5. Desactivar un usuario

Desactivar un usuario bloquea su acceso al portal sin eliminar el registro.

Desde el listado, hacer clic en el ícono de **eliminar** de la fila correspondiente y confirmar la acción.

El usuario quedará en estado **Inactivo** y al intentar ingresar al portal el sistema mostrará el mensaje *"Usuario inactivo"*.

> Para reactivar un usuario, editar el registro y cambiar el estado a **Activo**.

---

## 6. Flujo completo del proceso

```
Administrador (Portal Admin)           Sistema                  Usuario (Portal Compañías)
        │                                 │                               │
        │── Nuevo Usuario Externo ────────>│                               │
        │   login, email, entidad, rol     │                               │
        │                                 │── Valida login único           │
        │                                 │── Genera clave temporal        │
        │                                 │── Crea usuario activo          │
        │<── Notificación de éxito ────────│                               │
        │                                 │── Envía correo bienvenida ────>│
        │                                 │   (login + clave temporal)     │
        │                                 │                                │
        │                                 │<── Ingresa con clave temporal ─│
        │                                 │<── Completa CAPTCHA ───────────│
        │                                 │                                │
        │                                 │── Detecta clave temporal       │
        │                                 │── Redirige a Cambiar Clave ───>│
        │                                 │                                │
        │                                 │<── Nueva contraseña ───────────│
        │                                 │── Acceso al Dashboard ────────>│
```

---

## Capturas requeridas para este manual

| # | Pantalla | Descripción |
|---|----------|-------------|
| adm_01 | Listado de Usuarios Externos | Tabla con registros y botón **"+ Nuevo Usuario"** visible |
| adm_02 | Modal "Nuevo Usuario Externo" | Formulario completado con datos de ejemplo |
| adm_03 | Detalle del checkbox | Opción "Enviar credenciales por correo" marcada |
| adm_04 | Notificación de éxito | Toast verde tras crear el usuario |
| adm_05 | Modal "Editar Usuario" | Formulario de edición con campo de estado |
| adm_06 | Modal "Reset de Clave" | Confirmación con opción de envío por correo |

---

*Documento elaborado por el equipo de desarrollo ADG – Sistema SHM*
*Versión 1.0 – Mayo 2026*
