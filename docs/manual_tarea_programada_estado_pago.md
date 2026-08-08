# Manual de Instalacion - Tarea Programada: Actualizacion de Estado de Pago

| Atributo | Valor |
|----------|-------|
| **Proyecto** | SHM - Sistema de Honorarios Medicos |
| **Cliente** | Complejo Hospitalario San Pablo |
| **Documento** | Instalacion de tarea programada - Actualizar Estado de Pago |
| **Version** | 1.0 |
| **Fecha** | Julio 2026 |
| **Dirigido a** | Administrador de Red / Infraestructura |

---

## 1. Objetivo

Configurar una tarea programada de Windows que ejecute diariamente, a una hora fija, el proceso que actualiza el estado de pago (consultado en SAP) de las facturas y recibos por honorarios (RHE) que pertenecen a ordenes de pago aprobadas.

El proceso invoca un endpoint del API REST del sistema SHM. No requiere instalar software adicional: solo copiar un script de PowerShell y registrar una tarea en el Task Scheduler de Windows.

---

## 2. Componentes involucrados

| Componente | Detalle |
|-----------|---------|
| **Endpoint invocado** | `POST /api/sapinterface/actualizar-estado-pago-masivo` |
| **API REST (SHM)** | `SHM.AppApiHonorarioMedico`, desplegado en IIS — Servidor `192.1.0.173`, puerto `92` (ver `docs/despliegue_solucion.md`) |
| **Script** | `Actualizar-EstadoPagoMasivo.ps1` (PowerShell) |
| **Mecanismo de ejecucion** | Task Scheduler de Windows (tarea diaria) |
| **Log generado** | Archivo de texto diario, uno por fecha de ejecucion |

> **Requisito previo:** el API REST (`SHM.AppApiHonorarioMedico`) debe estar desplegado y accesible desde el servidor donde se registrara la tarea programada. Si la tarea corre en un servidor distinto al del API, verificar conectividad de red (puerto 92, HTTPS) antes de continuar.

---

## 3. Requisitos previos

- Acceso de administrador al servidor donde se registrara la tarea (puede ser el mismo servidor de aplicaciones, `192.1.0.173`, u otro servidor con acceso de red al API).
- PowerShell 5.1 o superior (viene incluido en Windows Server 2016+).
- Acceso HTTPS (puerto 92) desde el servidor de la tarea hacia `192.1.0.173`.
- Carpeta de destino para el script, por ejemplo: `D:\appweb\shmappapi\scripts\`.
- Carpeta de destino para los logs, por ejemplo: `D:\appweb\shmappapi\logs\`.

---

## 4. Paso 1 - Copiar el script al servidor

1. Copiar el archivo [`scripts/Actualizar-EstadoPagoMasivo.ps1`](../scripts/Actualizar-EstadoPagoMasivo.ps1) del repositorio hacia el servidor, en la ruta:

   ```
   D:\appweb\shmappapi\scripts\Actualizar-EstadoPagoMasivo.ps1
   ```

2. Crear la carpeta de logs si no existe:

   ```powershell
   New-Item -ItemType Directory -Path "D:\appweb\shmappapi\logs" -Force
   ```

---

## 5. Paso 2 - Probar el script manualmente

Antes de programar la tarea, ejecutar el script una vez de forma manual para validar que responde correctamente:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\appweb\shmappapi\scripts\Actualizar-EstadoPagoMasivo.ps1"
```

**Validar:**

- El comando termina sin mostrar errores en pantalla.
- Se genera un archivo de log en `D:\appweb\shmappapi\logs\actualizar-estado-pago-masivo_AAAAMMDD.log` (fecha del dia actual).
- El contenido del log muestra una linea `OK - Ordenes: N | Comprobantes: N | ...` (proceso exitoso) o el detalle del error si algo fallo (por ejemplo, problema de conectividad con el API).

Si el log muestra `EXCEPCION` relacionado a certificado SSL o conexion rechazada, verificar:
- Que el API este activo (`https://192.1.0.173:92/swagger` debe responder desde el navegador del servidor).
- Que no haya un firewall bloqueando el puerto 92 entre el servidor de la tarea y `192.1.0.173`.

---

## 6. Paso 3 - Registrar la tarea programada

### Opcion A: Linea de comandos (recomendado)

Ejecutar en una consola de **PowerShell o CMD como Administrador**:

```cmd
schtasks /create ^
  /tn "SHM_ActualizarEstadoPagoMasivo" ^
  /tr "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"D:\appweb\shmappapi\scripts\Actualizar-EstadoPagoMasivo.ps1\"" ^
  /sc daily ^
  /st 06:00 ^
  /ru "SYSTEM" ^
  /rl HIGHEST ^
  /f
```

**Descripcion de parametros:**

| Parametro | Significado |
|-----------|-------------|
| `/tn` | Nombre de la tarea en el Task Scheduler |
| `/tr` | Accion a ejecutar (PowerShell + ruta del script) |
| `/sc daily /st 06:00` | Frecuencia diaria, a las 06:00 AM |
| `/ru "SYSTEM"` | Cuenta de ejecucion (no requiere sesion de usuario iniciada) |
| `/rl HIGHEST` | Ejecuta con privilegios elevados |
| `/f` | Sobrescribe la tarea si ya existiera con el mismo nombre |

> Ajustar la hora (`/st`) segun lo que indique el equipo de sistemas si 06:00 AM no es la hora final acordada.

### Opcion B: Interfaz grafica (Task Scheduler)

1. Abrir **Task Scheduler** (`taskschd.msc`).
2. Panel derecho → **Create Task...** (no usar "Create Basic Task" para poder configurar todas las opciones).
3. Pestaña **General**:
   - Name: `SHM_ActualizarEstadoPagoMasivo`
   - Seleccionar **Run whether user is logged on or not**
   - Marcar **Run with highest privileges**
4. Pestaña **Triggers** → **New...**:
   - Begin the task: `On a schedule`
   - Settings: `Daily`
   - Start: hora `06:00:00`
   - Recur every: `1 days`
   - **OK**
5. Pestaña **Actions** → **New...**:
   - Action: `Start a program`
   - Program/script: `powershell.exe`
   - Add arguments:
     ```
     -NoProfile -ExecutionPolicy Bypass -File "D:\appweb\shmappapi\scripts\Actualizar-EstadoPagoMasivo.ps1"
     ```
   - **OK**
6. Pestaña **Conditions**: desmarcar "Start the task only if the computer is on AC power" (si aplica a un servidor).
7. Pestaña **Settings**: marcar **If the task fails, restart every** 10 minutes, hasta 3 intentos (opcional, recomendado).
8. **OK** y confirmar credenciales si se solicitan (usuario/clave de la cuenta que ejecutara la tarea, si no se uso `SYSTEM`).

---

## 7. Paso 4 - Verificar la instalacion

1. En Task Scheduler, ubicar la tarea `SHM_ActualizarEstadoPagoMasivo` en la lista.
2. Clic derecho → **Run** (para forzar una ejecucion inmediata de prueba).
3. Verificar en la columna **Last Run Result** que el codigo sea `0x0` (exito).
4. Revisar el log generado en `D:\appweb\shmappapi\logs\` para confirmar el detalle del proceso (ordenes procesadas, comprobantes actualizados, errores).

---

## 8. Mantenimiento

- **Logs:** se genera un archivo por dia (`actualizar-estado-pago-masivo_AAAAMMDD.log`). No hay rotacion/limpieza automatica configurada; se recomienda que el administrador de red establezca una politica de retencion (por ejemplo, limpiar logs con mas de 90 dias) segun las politicas de la organizacion.
- **Cambio de hora de ejecucion:** modificar el trigger de la tarea desde Task Scheduler (pestaña Triggers → Edit), o reejecutar el comando `schtasks /create ... /f` con el nuevo valor en `/st`.
- **Deshabilitar temporalmente:** clic derecho sobre la tarea → **Disable**.
- **Desinstalar:**
  ```cmd
  schtasks /delete /tn "SHM_ActualizarEstadoPagoMasivo" /f
  ```

---

## 9. Contacto

Ante dudas sobre el comportamiento del endpoint o del proceso de negocio, contactar al equipo de desarrollo (ADG Systems).
