# Flujo de Envío de Factura — Wizard 4 Pasos

**Portal:** Compañías Médicas (`SHM.AppWebCompaniaMedica`)  
**URL de entrada:** `GET /Facturas/Subir?guid={guidRegistro}`  
**Última actualización:** 2026-04-20

---

## Visión General

El envío de una factura se realiza mediante un wizard de 4 pasos implementado como una **SPA AJAX** (sin recargas de página entre pasos). La navegación entre pasos oculta/muestra paneles en el DOM. Solo el paso inicial y las llamadas de datos son requests al servidor.

```
[Paso 1] Revisar Datos
    ↓  btnSiguiente
[Paso 2] Adjuntar Comprobante  ──→  POST PrepararVistaPreviaAjax
    ↓  btnSubirArchivos                  ↓ genera temp/{sessionId}/
[Paso 3] Ver PDF + Datos XML   ──→  GET ObtenerPdfTemporal
    ↓  btnIrPaso4              ──→  GET DatosXmlPartial
[Paso 4] Validar y Enviar      ──→  GET ValidacionPartial
    ↓  btnConfirmarEnvio       ──→  POST ConfirmarEnvio
         ↓ guarda en BD, elimina temp/
```

---

## Paso 1 — Revisar Datos

**Acción:** `GET /Facturas/Subir?guid={guidRegistro}`  
**Controlador:** `FacturasController.Subir(string guid)`

### Qué hace
- Carga desde BD los datos de la producción: sede, importe, fechas, RUC emisor/receptor, cuenta bancaria.
- Evalúa parámetros del sistema:
  - `SHM_VALIDA_CUENTA_BANCARIA` — si requiere cuenta bancaria registrada para continuar.
  - `SHM_REQUIERE_ARCHIVO_CDR` — si el CDR es obligatorio en el paso siguiente.
- Construye el `SubirFacturaViewModel` y renderiza la vista.

### Lo que NO hace
- No guarda nada en BD ni en disco.
- Es **solo lectura**.

### Variables JS inicializadas
```javascript
var sessionIdActual = null;   // aún no hay sesión temp
var esDirty = true;           // fuerza subida de archivos al avanzar
var pasoActual = 1;
```

---

## Paso 2 — Adjuntar Comprobante

**Acción (AJAX):** `POST /Facturas/PrepararVistaPreviaAjax`  
**Trigger:** botón *Siguiente* (`btnSubirArchivos`)

### Formulario que completa el usuario
| Campo | Tipo | Notas |
|-------|------|-------|
| Tipo Comprobante | Texto readonly | Pre-cargado desde producción |
| Fecha Emisión | Date | No puede ser fecha futura |
| Serie | Texto | Mayúsculas automáticas |
| Número | Número | Solo dígitos |
| Archivo PDF | File | Requerido |
| Archivo XML | File | Requerido |
| Archivo CDR | File | Requerido según parámetro |

### Qué hace el servidor
1. Valida que los archivos estén presentes.
2. Parsea el XML según tipo:
   - Factura/Boleta → `FacturaXmlParserService.ParseFacturaXml()`
   - RHE → `RheXmlParserService.ParseRheXml()`
3. Genera un `sessionId` único (`Guid.NewGuid().ToString("N")`).
4. Crea carpeta temporal: `{FileStorage:UploadPath}/temp/{sessionId}/`
5. Guarda los siguientes archivos en disco:

| Archivo | Contenido |
|---------|-----------|
| `factura.pdf` | PDF subido por el usuario |
| `factura.xml` | XML subido por el usuario |
| `cdr.{ext}` | CDR si aplica |
| `datos.json` | `FacturaXmlData` serializado (camelCase) con todos los datos parseados del XML |
| `metadata.json` | Datos del formulario: GuidRegistro, TipoComprobante, Serie, Número, FechaEmision, TieneCdr, CdrExtension |

6. Devuelve `{ success: true, sessionId: "..." }` al cliente.

### Qué hace el JS al recibir la respuesta
```javascript
sessionIdActual = data.sessionId;  // guarda el ID de sesión
esDirty = false;                   // marca como sin cambios pendientes
// Carga el PDF en el iframe del Paso 3
document.getElementById('pdfIframe').src = '/Facturas/ObtenerPdfTemporal?sessionId=' + sessionIdActual;
// Pre-carga datos XML y validaciones en paralelo
cargarDatosXml();
cargarValidacion();
mostrarPanel(3);
```

### Lógica de re-subida al volver del Paso 3/4
Si el usuario regresa al Paso 2 y modifica algo (campos o archivos), `esDirty = true` se activa. Al avanzar nuevamente:
- Si `esDirty = true` → llama `PrepararVistaPreviaAjax` de nuevo, enviando el `sessionIdAnterior` para que el servidor elimine la carpeta temp vieja.
- Si `esDirty = false` → salta directamente al Paso 3 sin re-subir.

### Protecciones implementadas
- **Estado inconsistente (pestaña duplicada):** Al cargar el DOM, si hay texto en los campos pero los file inputs están vacíos (el navegador restaura texto pero nunca archivos por política de seguridad), los campos de texto se limpian automáticamente.
- **Botón deshabilitado:** El botón *Siguiente* permanece deshabilitado hasta que PDF + XML (+ CDR si aplica) estén cargados.

---

## Paso 3 — Ver PDF + Datos XML

**No tiene acción POST.** Lee exclusivamente de los archivos temporales del Paso 2.

### Endpoint PDF
`GET /Facturas/ObtenerPdfTemporal?sessionId={sessionId}`  
Lee `temp/{sessionId}/factura.pdf` y lo sirve como `application/pdf` inline en el `<iframe>`.

### Endpoint Datos XML (Partial)
`GET /Facturas/DatosXmlPartial?sessionId={sessionId}`  
Lee `temp/{sessionId}/datos.json`, deserializa a `FacturaXmlData` y renderiza `_DatosXmlPartial.cshtml` con las secciones:
- Comprobante (Tipo, Número, Fecha Emisión, Moneda)
- Emisor (RUC, Razón Social)
- Cliente (RUC/DNI, Razón Social)
- Importes (Valor Venta, IGV/Retención, Importe Total)
- Items (lista de líneas del comprobante)

---

## Paso 4 — Validar y Enviar

### Validación (Partial)
`GET /Facturas/ValidacionPartial?sessionId={sessionId}`  
- Lee `metadata.json` → datos ingresados por el usuario en el formulario.
- Lee `datos.json` → datos extraídos del XML.
- Compara campo a campo (Tipo, Fecha, Serie, Número, Importe, RUC Emisor, RUC Receptor) y muestra diferencias con indicadores visual OK / Error / Advertencia.

### Confirmación y guardado en BD
`POST /Facturas/ConfirmarEnvio` — recibe únicamente el `sessionId`.

#### Flujo de procesamiento
1. Lee `metadata.json` → obtiene GuidRegistro, TipoComprobante, Serie, Número, FechaEmision.
2. Lee `datos.json` → obtiene datos completos del XML.
3. Verifica cuenta bancaria según parámetro `SHM_VALIDA_CUENTA_BANCARIA`.
4. Valida concepto contra primer ítem del XML si parámetro `SHM_VALIDA_FACTURA_CONCEPTO = S`.
5. Lee archivos físicos del temp (PDF, XML, CDR).
6. Guarda en BD:
   - Registra archivo en `SHM_ARCHIVO`.
   - Registra relación en `SHM_ARCHIVO_COMPROBANTE`.
   - Actualiza estado de producción en `SHM_PRODUCCION`.
   - Registra en bitácora `SHM_BITACORA`.
7. **Elimina la carpeta temporal** `temp/{sessionId}/`.
8. Devuelve `{ success: true, message: "..." }` → el JS redirige a `/Facturas/Enviadas`.

---

## Diagrama de Archivos Temporales

```
uploads/
└── temp/
    └── {sessionId}/          ← creado en Paso 2, eliminado en Paso 4
        ├── factura.pdf
        ├── factura.xml
        ├── cdr.zip            (opcional)
        ├── datos.json         ← FacturaXmlData (camelCase JSON)
        └── metadata.json      ← datos del formulario del usuario
```

---

## Parámetros del Sistema Involucrados

| Código | Descripción | Efecto |
|--------|-------------|--------|
| `SHM_VALIDA_CUENTA_BANCARIA` | `S` = cuenta bancaria obligatoria | Bloquea envío si no hay cuenta registrada |
| `SHM_REQUIERE_ARCHIVO_CDR` | `N` = CDR no requerido | Controla si el campo CDR es obligatorio en Paso 2 |
| `SHM_COMPROBANTE_VALIDA_TIPO` | `N` = no validar | Omite comparación de tipo en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_FECHA_EMISION` | `N` = no validar | Omite comparación de fecha en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_SERIE` | `N` = no validar | Omite comparación de serie en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_NUMERO` | `N` = no validar | Omite comparación de número en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_IMPORTE` | `N` = no validar | Omite comparación de importe en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_RUC_EMISOR` | `N` = no validar | Omite comparación de RUC emisor en Paso 4 |
| `SHM_COMPROBANTE_VALIDA_RUC_RECEPTOR` | `N` = no validar | Omite comparación de RUC receptor en Paso 4 |
| `SHM_VALIDA_FACTURA_CONCEPTO` | `S` = validar | Compara concepto de producción con primer ítem del XML |

---

## Archivos Fuente Principales

| Archivo | Rol |
|---------|-----|
| `Controllers/FacturasController.cs` | Toda la lógica del servidor |
| `Views/Facturas/Subir.cshtml` | Wizard completo (Pasos 1–4) + JS |
| `Views/Facturas/_DatosXmlPartial.cshtml` | Panel XML del Paso 3 |
| `Views/Facturas/_ValidacionPartial.cshtml` | Panel de validación del Paso 4 |
| `Models/FacturasViewModels.cs` | ViewModels del wizard |
| `Models/FacturaXmlData.cs` | Modelo de datos del XML parseado |
| `Services/FacturaXmlParserService.cs` | Parser XML para Facturas/Boletas |
| `Services/RheXmlParserService.cs` | Parser XML para RHE |
