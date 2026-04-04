# Flujo de Proceso — Sistema de Honorarios Médicos (SHM)
**Sistema:** SHM - Sistema de Honorarios Médicos
**Fecha:** 2026-03-15

---

## PROCESO GENERAL — Visión Completa del Sistema

El proceso de pago de honorarios médicos involucra múltiples actores y sistemas. A continuación se describe el flujo de extremo a extremo.

### Actores y Sistemas

| Actor / Sistema | Descripción |
|-----------------|-------------|
| **Área de Honorarios Médicos** | Equipo interno que gestiona las producciones y liquidaciones |
| **Sistema HHMM** (legado) | Sistema anterior de honorarios médicos |
| **Sistema SHM** (nuevo) | Nuevo sistema de honorarios médicos (este sistema) |
| **Portal Compañías Médicas** | Portal web para que las compañías emitan y suban sus comprobantes |
| **Compañías Médicas** | Empresas externas que emiten facturas o RHE por los servicios médicos |
| **Jefe de Sede** | Primer aprobador de órdenes de pago |
| **Gerente Corporativo** | Aprobador final de órdenes de pago |
| **Tesorería** | Área que ejecuta el pago una vez aprobado |

---

### Diagrama de Flujo General

```
┌─────────────────────────────────────────────────────────────────────────┐
│  FASE 1 — REGISTRO DE PRODUCCIONES                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Sistema HHMM (legado)                                                  │
│       │                                                                 │
│       │  Envía producciones vía API                                     │
│       ▼                                                                 │
│  Sistema SHM (nuevo)  ──────────────────────────────────────────────►   │
│                          Crea solicitud de comprobante                  │
│                          para cada compañía médica                      │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  FASE 2 — EMISIÓN DE COMPROBANTES (Compañías Médicas)                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Compañía Médica ingresa al Portal SHM                                  │
│       │                                                                 │
│       ▼                                                                 │
│  Ve bandeja de comprobantes pendientes                                  │
│       │                                                                 │
│       ▼                                                                 │
│  Emite la Factura o RHE en su sistema de facturación electrónica        │
│       │                                                                 │
│       ▼                                                                 │
│  Sube al Portal SHM:                                                    │
│    · Datos del comprobante (serie, número, fecha)                       │
│    · Archivo PDF                                                        │
│    · Archivo XML (firmado electrónicamente)                             │
│       │                                                                 │
│       ▼                                                                 │
│  Sistema SHM valida el XML y registra el comprobante                    │
│       │                                                                 │
│       ▼                                                                 │
│  Sistema SHM notifica al Sistema HHMM (legado) vía API                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  FASE 3 — RECEPCIÓN Y LIQUIDACIÓN (Área de Honorarios Médicos)          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Área de Honorarios Médicos recepciona las facturas en Sistema HHMM     │
│       │                                                                 │
│       ▼                                                                 │
│  Sistema HHMM genera las Liquidaciones                                  │
│       │                                                                 │
│       │  Envía liquidaciones vía API                                    │
│       ▼                                                                 │
│  Sistema SHM (nuevo) recibe las liquidaciones                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  FASE 4 — ORDEN DE PAGO                                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Sistema SHM agrupa las facturas recibidas                              │
│       │                                                                 │
│       ▼                                                                 │
│  Se genera una Orden de Pago                                            │
│       │                                                                 │
│       ▼                                                                 │
│  ┌─────────────────────────────────────────────────┐                    │
│  │           FLUJO DE APROBACIÓN                   │                    │
│  │                                                 │                    │
│  │  Orden de Pago creada                           │                    │
│  │       │                                         │                    │
│  │       ▼                                         │                    │
│  │  1° Aprobación: Jefe de Sede                    │                    │
│  │       │                        ┌─ Rechaza ──►   │Devuelve / Corrige  │
│  │       ▼ Aprueba                │                │                    │
│  │  2° Aprobación: Gerente Corp.  │                │                    │
│  │       │                        └─ Rechaza ──►   │Devuelve / Corrige  │
│  │       ▼ Aprueba                                 │                    │
│  │  Orden de Pago APROBADA                         │                    │
│  └─────────────────────────────────────────────────┘                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  FASE 5 — PAGO (Tesorería)                                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Tesorería recibe la Orden de Pago aprobada                             │
│       │                                                                 │
│       ▼                                                                 │
│  Procede a realizar el pago a la compañía médica                        │
│       │                                                                 │
│       ▼                                                                 │
│                       PROCESO COMPLETADO                                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### Resumen de Fases

| Fase | Descripción | Actor Principal | Sistema |
|------|-------------|-----------------|---------|
| **1. Registro** | Las producciones se envían al nuevo sistema | Sistema HHMM → SHM | API |
| **2. Emisión** | Las compañías suben sus comprobantes | Compañías Médicas | Portal SHM |
| **3. Liquidación** | Se generan las liquidaciones en HHMM y se envían al SHM | Área Honorarios | HHMM → SHM |
| **4. Orden de Pago** | Se agrupan facturas, se genera y aprueba la orden | Jefe Sede / Gerente | Sistema SHM |
| **5. Pago** | Tesorería ejecuta el pago | Tesorería | — |

---

## DETALLE — Portal Compañías Médicas

> Esta sección describe en detalle la **Fase 2** del proceso general: cómo las compañías médicas acceden al portal y suben sus comprobantes.

---

## Descripción General

El Portal de Compañías Médicas permite registrar y enviar comprobantes electrónicos (Facturas y Recibos por Honorarios) asociados a producciones médicas pendientes de pago.

---

## FLUJO PRINCIPAL — Envío de Comprobante

```
┌─────────────────────────────────────────────────────────────┐
│                     INICIO DE SESIÓN                        │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │  ¿Credenciales         │
              │  correctas?            │
              └────────┬───────────────┘
                  NO   │   SÍ
                  ▼    │
           [Error y    ▼
           reintento] DASHBOARD
                       │
                       ▼
         ┌─────────────────────────────┐
         │   COMPROBANTES PENDIENTES   │
         │   (lista de producciones)   │
         └─────────────┬───────────────┘
                       │
                       ▼
         ┌─────────────────────────────┐
         │  Seleccionar producción     │
         │  → clic en [Subir]          │
         └─────────────┬───────────────┘
                       │
                       ▼
         ┌─────────────────────────────┐
         │   FORMULARIO DE SUBIDA      │
         │   Adjuntar archivos y       │
         │   completar datos           │
         └─────────────┬───────────────┘
                       │
                       ▼
              ┌────────────────────┐
              │  ¿Archivos y       │
              │  datos válidos?    │
              └────────┬───────────┘
                  NO   │   SÍ
                  ▼    │
           [Corregir   ▼
            errores]  VISTA PREVIA
                      (validación)
                       │
                       ▼
              ┌────────────────────┐
              │  ¿Datos del XML    │
              │  coinciden?        │
              └────────┬───────────┘
                  NO   │   SÍ (o advertencias aceptadas)
                  ▼    │
           [Revisar    ▼
            XML o    CONFIRMAR ENVÍO
            cancelar]  │
                       ▼
              ┌────────────────────┐
              │  ¿Envío a San      │
              │  Pablo exitoso?    │
              └────────┬───────────┘
                  NO   │   SÍ
                  ▼    │
           [Error,     ▼
           producción COMPROBANTE ENVIADO
           revertida]  (aparece en "Enviados")
                       │
                       ▼
                      FIN
```

---

## PASO 1 — Inicio de Sesión

**Ruta:** `/Auth/Login`

1. Ingresar **usuario** y **contraseña** proporcionados por el administrador.
2. Resolver el **captcha** de verificación.
3. Hacer clic en **Ingresar**.

> **Si olvidó su contraseña:**
> Haga clic en _"¿Olvidaste tu contraseña?"_ e ingrese su correo electrónico registrado. Recibirá un enlace de recuperación.

| Problema | Solución |
|----------|----------|
| "Usuario o contraseña incorrectos" | Verificar mayúsculas/minúsculas y volver a intentar |
| "Captcha incorrecto" | Refrescar el captcha y reintentar |
| No recibe email de recuperación | Revisar carpeta de spam o contactar al administrador |

---

## PASO 2 — Dashboard

Al ingresar verá el panel principal con:
- **Resumen** de producciones pendientes.
- Acceso rápido al menú de **Comprobantes**.

---

## PASO 3 — Comprobantes Pendientes

**Ruta:** `/Facturas/Pendientes`

Esta pantalla muestra todas las producciones médicas que están esperando que usted adjunte el comprobante electrónico correspondiente.

**Información visible en la lista:**
- Código de producción
- Sede
- Tipo de comprobante requerido (Factura / RHE)
- Período
- Monto total

**Para buscar una producción específica:**
1. Ingresar el código o nombre en el campo de búsqueda.
2. Presionar **Buscar**.

**Para iniciar el envío:**
1. Ubicar la producción deseada.
2. Hacer clic en el botón **[Subir]**.

---

## PASO 4 — Formulario de Subida

**Ruta:** `/Facturas/Subir`

Completar los siguientes campos:

### 4.1 Datos del Comprobante

| Campo | Descripción | Requerido |
|-------|-------------|-----------|
| Tipo de Comprobante | Factura o Recibo por Honorarios (pre-llenado) | Sí |
| Serie | Serie del comprobante (Ej: E001, F001) | Sí |
| Número | Número correlativo del comprobante | Sí |
| Fecha de Emisión | Fecha en que fue emitido el comprobante | Sí |

> ⚠️ **Importante:** La fecha de emisión no puede ser una fecha futura.

### 4.2 Archivos Requeridos

| Archivo | Descripción |
|---------|-------------|
| PDF | Representación impresa del comprobante |
| XML | Archivo electrónico firmado del comprobante |
| CDR | Constancia de Recepción de SUNAT (si aplica) |

> ⚠️ **El archivo XML debe corresponder exactamente al tipo de comprobante seleccionado.**
> Un XML de Factura no es válido para una producción de RHE y viceversa.

### 4.3 Hacer clic en **[Continuar]**

El sistema validará automáticamente:
- Que todos los archivos estén adjuntos.
- Que el XML sea un documento electrónico válido.
- Que el XML contenga el RUC del emisor y del cliente.
- Que la fecha no sea futura.

**Si hay errores:** aparecerá una alerta indicando qué corregir. No se avanzará hasta resolver todos los errores.

---

## PASO 5 — Vista Previa y Validación

**Ruta:** `/Facturas/VistaPrevia`

Esta pantalla compara los datos ingresados en el formulario con los datos extraídos del archivo XML, para garantizar que el comprobante sea correcto antes de enviarlo.

### 5.1 Tabla de Validaciones

Cada campo muestra uno de los siguientes estados:

| Icono | Estado | Significado |
|-------|--------|-------------|
| ✅ | **Correcto** | El dato del formulario coincide con el XML |
| ⚠️ | **Observado** | Hay una diferencia entre el formulario y el XML |
| ➖ | **No validado** | El campo no está habilitado para validación |

### 5.2 Campos Validados

| Campo | Qué se verifica |
|-------|----------------|
| Tipo de Comprobante | El tipo del formulario coincide con el tipo en el XML |
| Serie | La serie ingresada coincide con la del XML |
| Número | El número ingresado coincide con el del XML |
| Fecha de Emisión | La fecha ingresada coincide con la del XML |
| RUC Emisor | El RUC de quien emite coincide con el del XML |
| RUC Receptor | El RUC del cliente (su empresa) coincide con el XML |
| Importe Total | El monto de la producción coincide con el del XML (tolerancia ±0.01) |

### 5.3 ¿Qué hacer si hay observaciones?

- **Observación en Serie/Número/Fecha:** Volver atrás y corregir el formulario o verificar que subió el XML correcto.
- **Observación en RUC:** Verificar que el comprobante fue emitido a la empresa correcta.
- **Observación en Importe:** El monto del comprobante debe coincidir con el monto de la producción.

### 5.4 Botones disponibles

| Botón | Acción |
|-------|--------|
| **[Confirmar Envío]** | Procede a enviar el comprobante (disponible si no hay errores bloqueantes) |
| **[Cancelar]** | Descarta los archivos cargados y vuelve al formulario de Subida |

---

## PASO 6 — Confirmar Envío

Al hacer clic en **[Confirmar Envío]**:

1. El sistema guardará los archivos (PDF, XML, CDR) de forma permanente.
2. Registrará el comprobante en el sistema local.
3. Enviará la información al sistema de San Pablo (HHMM).
4. Actualizará el estado de la producción.

### Posibles resultados:

| Resultado | Mensaje | Acción recomendada |
|-----------|---------|-------------------|
| ✅ Exitoso | "Factura enviada exitosamente" | La producción desaparece de Pendientes y aparece en Enviados |
| ❌ Error de envío | "Error al enviar el comprobante: [detalle]" | La producción vuelve a estado anterior. Intentar nuevamente o contactar al administrador |

---

## PASO 7 — Comprobantes Enviados

**Ruta:** `/Facturas/Enviadas`

Aquí podrá consultar todos los comprobantes que ya fueron enviados exitosamente.

**Funciones disponibles:**
- **Buscar** por código de producción, serie o número.
- **Ver detalle** del comprobante.
- **Descargar** los archivos adjuntos (PDF, XML, CDR).

---

## FLUJO — Recuperación de Contraseña

```
  Usuario en Login
       │
       ▼
  Clic en "¿Olvidaste tu contraseña?"
       │
       ▼
  Ingresar email o usuario registrado
       │
       ▼
  ¿Email encontrado?
  ├── NO  → Mensaje informativo (sin revelar si existe)
  └── SÍ  → Email de recuperación enviado
                 │
                 ▼
            Abrir enlace del email
                 │
                 ▼
            ¿Token vigente?
            ├── NO  → Error: token expirado. Repetir proceso.
            └── SÍ  → Formulario para nueva contraseña
                            │
                            ▼
                       Ingresar nueva contraseña
                       (confirmar dos veces)
                            │
                            ▼
                       Contraseña actualizada
                            │
                            ▼
                       Redirige al Login
```

---

## HERRAMIENTA — Evaluador de XML

**Ruta:** `/Herramientas/EvaluadorXml`

Herramienta de diagnóstico que permite verificar un archivo XML antes de subirlo como comprobante.

**Cuándo usarla:**
- Cuando no sabe si su XML es de Factura o de RHE.
- Cuando quiere verificar que su XML está bien formado antes de subirlo.
- Para ver todos los datos que contiene el XML.

**Pasos:**
1. Seleccionar el tipo de comprobante (Factura / Recibo por Honorarios).
2. Hacer clic en **[Seleccionar archivo]** y elegir el XML.
3. Hacer clic en **[Evaluar XML]**.
4. Revisar el resultado: estado de validación y datos extraídos.

---

## Preguntas Frecuentes

**¿Por qué mi XML dice "El XML no contiene el RUC del emisor"?**
El archivo XML no tiene la estructura correcta o está incompleto. Verifique que descargó el XML firmado desde su sistema de emisión electrónica (no el XML de borrador).

**¿Puedo subir un XML de otro período?**
Sí, pero la fecha de emisión del XML debe coincidir con la que ingresa en el formulario. Si hay diferencia aparecerá como Observado en la Vista Previa.

**¿Qué pasa si cancelo en la Vista Previa?**
Los archivos cargados se eliminan automáticamente. La producción vuelve a aparecer en Pendientes y puede volver a intentarlo.

**¿Qué significa que la producción fue "revertida"?**
Si ocurre un error al enviar a San Pablo, el sistema deshace el registro automáticamente para que pueda intentar el envío nuevamente sin datos inconsistentes.

**¿Qué hago si el sistema muestra "Error al enviar el comprobante"?**
1. Anote el mensaje de error exacto.
2. Verifique su conexión a internet.
3. Intente nuevamente en unos minutos.
4. Si el error persiste, contacte al administrador indicando el código de producción y el mensaje de error.

---

## Glosario

| Término | Descripción |
|---------|-------------|
| **Producción** | Registro de servicio médico que requiere un comprobante de pago |
| **Comprobante** | Factura electrónica o Recibo por Honorarios Electrónico (RHE) |
| **XML** | Archivo electrónico firmado que representa el comprobante ante SUNAT |
| **CDR** | Constancia de Recepción, respuesta de SUNAT confirmando que aceptó el comprobante |
| **RHE** | Recibo por Honorarios Electrónico, comprobante emitido por personas naturales |
| **Vista Previa** | Pantalla de validación antes de confirmar el envío definitivo |
| **HHMM** | Sistema interno de San Pablo al que se registran los comprobantes |
| **Serie** | Código alfanumérico que identifica el punto de emisión (Ej: E001, F001) |

---

*Documento generado: 2026-03-31*
