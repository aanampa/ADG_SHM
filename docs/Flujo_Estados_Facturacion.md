# Flujo de Estados de Facturación
## Sistema de Honorarios Médicos (SHM)

**Versión:** 1.5
**Fecha:** 29 de Marzo 2026
**Autor:** ADG Vladimir D

---

## 1. Descripción General

Este documento describe el flujo de estados del proceso de facturación entre la Empresa (Portal HHMM) y las Compañías Médicas (Proveedores) en el Sistema de Honorarios Médicos.

---

## 2. Actores del Sistema

| Actor | Portal | Descripción |
|-------|--------|-------------|
| **Empresa** | Portal HHMM (Administrativo) | Personal administrativo que gestiona las producciones y facturas |
| **Cía Médica** | Portal Compañía Médica | Proveedores médicos que envían sus facturas |

---

## 3. Estados del Proceso

| Descripción en Pantalla | Código (BD) | Descripción |
|-------------------------|-------------|-------------|
| Factura Pendiente | `FACTURA_PENDIENTE` | Estado inicial. La producción está registrada pero no se ha solicitado factura |
| Factura Solicitada | `FACTURA_SOLICITADA` | La empresa ha solicitado la factura estableciendo una fecha límite |
| Factura Enviada | `FACTURA_ENVIADA` | La Cía Médica ha registrado y enviado los documentos de factura |
| Factura Aceptada | `FACTURA_ACEPTADA` | La empresa ha revisado y aprobado la factura. Se envía automáticamente a SAP |
| Factura Enviada a HHMM | `FACTURA_ENVIADA_HHMM` | La factura fue registrada exitosamente en el sistema SAP (HHMM) |
| Factura en Orden de Pago | `FACTURA_ORDEN_PAGO` | La factura ha sido incluida en una Orden de Pago generada |
| Factura Liquidada | `FACTURA_LIQUIDADA` | La factura está en proceso de liquidación contable |
| Factura Pagada | `FACTURA_PAGADA` | El pago ha sido realizado |
| Factura Devuelta | `FACTURA_DEVUELTA` | La empresa ha rechazado la factura por observaciones |
| Factura Anulada | `FACTURA_ANULADA` | La factura ha sido anulada |

---

## 4A. Diagrama de Flujo — SHM_COMPROBANTE_ENVIA_HHMM = NO

> La Cía Médica sube la factura sin llamar al API. El admin acepta y el sistema envía automáticamente a SAP.

```
  ┌─────────────────────┐
  │  Factura Pendiente  │  ← Estado Inicial
  └──────────┬──────────┘
             │ [Empresa solicita factura]
             │ ✉ Notifica por correo a Cía Médica
             ▼
  ┌──────────────────────┐
  │  Factura Solicitada  │
  └──────────┬───────────┘
             │ [Cía Médica sube documentos (PDF, XML, CDR)]
             │  → pasa directamente a Factura Enviada
             ▼
  ┌──────────────────────┐◄──────────────────────────────────────────────┐
  │   Factura Enviada    │  ← Revierte aquí si API falla (Portal Admin)  │
  └──────────┬───────────┘                                               │
             │                                                           │
      ┌──────┴──────┐                                                    │
      ▼             ▼                                                    │
 [Rechazar]    [Aprobar]                                                 │
      │             │                                                    │
      ▼             ▼                                                    │
  ┌──────────┐  ┌─────────────────┐                                      │
  │ Factura  │  │ Factura         │                                      │
  │ Devuelta │  │ Aceptada        │                                      │
  └────┬─────┘  └────────┬────────┘                                      │
       │                 │ [Automático: llama API San Pablo]              │
       │                 │   → Si API OK  : → Factura Enviada a HHMM     │
       │                 │   → Si API FALLA: revierte a Factura Enviada ─┘
       │                 ▼
       │   ┌───────────────────────┐
       │   │ Factura Enviada a HHMM│
       │   └──────────┬────────────┘
       │              │ [Procesar liquidación]
       │              ▼
       │      ┌───────────────────┐
       │      │ Factura Liquidada │
       │      └────────┬──────────┘
       │               │ [Registrar pago]
       │               ▼
       │      ┌───────────────────┐
       │      │  Factura Pagada   │  ← Estado Final
       │      └───────────────────┘
       │
       │ [Cía Médica reenvía dentro del plazo]
       └─────────────────────────────────────► Factura Enviada
```

---

## 4B. Diagrama de Flujo — SHM_COMPROBANTE_ENVIA_HHMM = SI

> Al subir la factura, la Cía Médica llama al API San Pablo directamente. Si el API responde OK, la factura queda aprobada **implícitamente** y pasa de forma automática a Factura Enviada a HHMM. **No hay revisión manual del admin (sin Aceptar / Rechazar / Factura Devuelta).**

```
  ┌─────────────────────┐
  │  Factura Pendiente  │  ← Estado Inicial
  └──────────┬──────────┘
             │ [Empresa solicita factura]
             │ ✉ Notifica por correo a Cía Médica
             ▼
  ┌──────────────────────┐◄──────────────────────────────────────────────┐
  │  Factura Solicitada  │  ← Revierte aquí si API falla (Portal Cía Med)│
  └──────────┬───────────┘                                               │
             │ [Cía Médica sube documentos (PDF, XML, CDR)]              │
             │  → Llama API San Pablo automáticamente                    │
             │    → Si API FALLA: revierte a Factura Solicitada ─────────┘
             │    → Si API OK  : aprobación implícita → pasa directo a:
             ▼
  ┌───────────────────────┐
  │ Factura Enviada a HHMM│  ← Sin revisión del admin
  └──────────┬────────────┘
             │ [Procesar liquidación]
             ▼
     ┌───────────────────┐
     │ Factura Liquidada │
     └────────┬──────────┘
              │ [Registrar pago]
              ▼
     ┌───────────────────┐
     │  Factura Pagada   │  ← Estado Final
     └───────────────────┘
```

---

## 5. Transiciones de Estado

| # | Estado Origen | Estado Destino | Acción | Actor | Condición |
|---|---------------|----------------|--------|-------|-----------|
| 1 | FACTURA_PENDIENTE | FACTURA_SOLICITADA | Solicitar Factura | Empresa | Siempre |
| 2 | FACTURA_SOLICITADA | FACTURA_SOLICITADA | Actualizar Fecha Límite | Empresa | Siempre |
| 3 | FACTURA_SOLICITADA | FACTURA_ENVIADA | Enviar Factura (sin API) | Cía Médica | Param = NO |
| 4 | FACTURA_SOLICITADA | FACTURA_ENVIADA_HHMM | Enviar Factura + API OK (aprobación implícita) | Cía Médica | Param = SI |
| 5 | FACTURA_SOLICITADA | FACTURA_SOLICITADA | Enviar Factura + API falla (revertir) | Sistema | Param = SI |
| 6 | FACTURA_ENVIADA | FACTURA_ACEPTADA | Aceptar Factura | Empresa | Param = NO |
| 7 | FACTURA_ACEPTADA | FACTURA_ENVIADA_HHMM | Envío automático a SAP (API OK) | Sistema | Param = NO |
| 8 | FACTURA_ACEPTADA | FACTURA_ENVIADA | Envío automático a SAP (API falla, revertir) | Sistema | Param = NO |
| 9 | FACTURA_ENVIADA | FACTURA_DEVUELTA | Devolver Factura | Empresa | Param = NO |
| 10 | FACTURA_DEVUELTA | FACTURA_ENVIADA | Reenviar Factura | Cía Médica | Param = NO |
| 11 | FACTURA_ENVIADA_HHMM | FACTURA_LIQUIDADA | Procesar Liquidación | Empresa | Siempre |
| 12 | FACTURA_LIQUIDADA | FACTURA_PAGADA | Registrar Pago | Empresa | Siempre |

> **Param** = valor del parámetro `SHM_COMPROBANTE_ENVIA_HHMM` (SI / NO)
> **Nota:** Cuando Param = SI, los estados FACTURA_ENVIADA, FACTURA_ACEPTADA y FACTURA_DEVUELTA no forman parte del flujo principal.

---

## 6. Descripción Detallada de Cada Estado

### 6.1 FACTURA_PENDIENTE
- **Descripción:** Estado inicial cuando se crea una producción médica
- **Acciones disponibles:** Solicitar Factura
- **Actor responsable:** Empresa

### 6.2 FACTURA_SOLICITADA
- **Descripción:** La empresa ha solicitado la factura y establecido una fecha límite
- **Datos requeridos:** Fecha y hora límite para entrega
- **Notificación:** Se envía correo electrónico a la Cía Médica informando la solicitud de factura y la fecha límite
- **Acciones disponibles:**
  - Enviar Factura (Cía Médica)
  - Actualizar Fecha Límite (Empresa) - permite modificar la fecha límite si es necesario
- **Actor responsable:** Cía Médica / Empresa

### 6.3 FACTURA_ENVIADA
- **Descripción:** La Cía Médica ha subido los documentos de facturación
- **Documentos requeridos:** PDF, XML electrónico, CDR
- **Acciones disponibles:** Aceptar o Devolver
- **Actor responsable:** Empresa
- **También se llega aquí por reversion:** Si el admin acepta la factura pero el API San Pablo falla, el sistema revierte automáticamente a este estado (conservando los datos del comprobante: Serie, Número, Fecha Emisión)

### 6.4 FACTURA_ACEPTADA
- **Descripción:** La factura ha sido revisada y aprobada por la empresa
- **Acción automática:** Al aceptar, el sistema llama inmediatamente al API San Pablo para registrar el comprobante
  - **API OK:** transiciona a `FACTURA_ENVIADA_HHMM`
  - **API falla:** revierte a `FACTURA_ENVIADA` (sin limpiar datos del comprobante) y muestra error al admin
- **Actor responsable:** Empresa

### 6.4.1 FACTURA_ENVIADA_HHMM
- **Descripción:** La factura fue registrada exitosamente en el sistema SAP (HHMM)
- **Origen posibles:**
  - Transición automática desde `FACTURA_ACEPTADA` (Portal Admin) — siempre llama al API
  - Transición directa desde `FACTURA_SOLICITADA` (Portal Cía Médica) — solo si `SHM_COMPROBANTE_ENVIA_HHMM = SI` y el API responde OK
- **Acciones disponibles:** Procesar Liquidación
- **Actor responsable:** Empresa

### 6.5 FACTURA_DEVUELTA
- **Descripción:** La factura fue rechazada por observaciones
- **Motivos comunes:**
  - Datos incorrectos en la factura
  - Montos no coinciden
  - Documentos incompletos
- **Acciones disponibles:** Reenviar Factura (dentro del plazo)
- **Actor responsable:** Cía Médica

### 6.6 FACTURA_LIQUIDADA
- **Descripción:** La factura está en proceso de liquidación contable
- **Acciones disponibles:** Registrar Pago
- **Actor responsable:** Empresa

### 6.7 FACTURA_PAGADA
- **Descripción:** Estado final - El pago ha sido realizado
- **Acciones disponibles:** Ninguna (estado final)

---

## 7. Reglas de Negocio

1. **Fecha Límite:** Al solicitar factura, se debe establecer una fecha y hora límite
2. **Documentos Obligatorios:** Para enviar factura se requiere PDF, XML y CDR
3. **Reenvío:** Una factura devuelta puede ser reenviada solo dentro del plazo establecido
4. **Trazabilidad:** Cada cambio de estado registra fecha, hora y usuario que realizó la acción en la bitácora (`SHM_BITACORA`)
5. **Notificación por Correo:** Al solicitar factura, se notifica automáticamente a la Cía Médica
6. **Parámetro SHM_COMPROBANTE_ENVIA_HHMM:** Controla si el Portal de Compañías llama al API San Pablo al enviar la factura
   - `SI`: La Cía Médica envía → llama API → si OK la factura se **aprueba implícitamente** y pasa directo a `FACTURA_ENVIADA_HHMM` (sin revisión del admin, sin estados FACTURA_ENVIADA / FACTURA_ACEPTADA / FACTURA_DEVUELTA); si falla revierte a `FACTURA_SOLICITADA`
   - `NO`: La Cía Médica envía → pasa a `FACTURA_ENVIADA` → el admin revisa y puede Aceptar o Rechazar
7. **Envío a SAP desde Portal Admin (solo Param = NO):** Cuando el admin acepta una factura (flujo Param = NO), el sistema llama automáticamente al API San Pablo
   - Si API OK: transiciona a `FACTURA_ENVIADA_HHMM`
   - Si API falla: revierte a `FACTURA_ENVIADA` (conservando datos del comprobante) y notifica error al admin
8. **Reversión sin pérdida de datos:** Cuando el sistema revierte desde `FACTURA_ACEPTADA` a `FACTURA_ENVIADA` por fallo del API, los campos del comprobante (Serie, Número, Fecha Emisión) se conservan para que el admin pueda reintentar

---

## 8. Notificaciones por Correo

| Evento | Destinatario | Contenido |
|--------|--------------|-----------|
| Solicitud de Factura | Cía Médica | Código de producción, monto total, fecha límite de entrega |

> **Nota:** Las notificaciones se envían al correo registrado en la entidad médica

---

## 9. Campos de Fecha por Estado

| Campo | Se registra cuando |
|-------|-------------------|
| `FacturaFechaSolicitud` | Se solicita la factura |
| `FacturaFechaEnvio` | La Cía Médica envía la factura |
| `FacturaFechaAceptacion` | Se acepta la factura |
| `FacturaFechaPago` | Se registra el pago |
| `FechaLimite` | Fecha límite para entrega (establecida al solicitar) |

---

## 10. Colores de Estado en la Interfaz

| Descripción en Pantalla | Código (BD) | Color | Clase CSS | Código Color |
|-------------------------|-------------|-------|-----------|--------------|
| Factura Pendiente | `FACTURA_PENDIENTE` | Amarillo | `badge-estado-pendiente` | #ffc107 |
| Factura Solicitada | `FACTURA_SOLICITADA` | Celeste | `badge-estado-solicitada` | #17a2b8 |
| Factura Enviada | `FACTURA_ENVIADA` | Azul | `badge-estado-enviada` | #007bff |
| Factura Aceptada | `FACTURA_ACEPTADA` | Verde Azulado | `badge-estado-aceptada` | #20c997 |
| Factura Enviada a HHMM | `FACTURA_ENVIADA_HHMM` | Verde Oscuro | `badge-estado-enviada-hhmm` | #198754 |
| Factura en Orden de Pago | `FACTURA_ORDEN_PAGO` | Naranja | `badge-estado-orden-pago` | #f26522 |
| Factura Liquidada | `FACTURA_LIQUIDADA` | Morado | `badge-estado-liquidada` | #6f42c1 |
| Factura Pagada | `FACTURA_PAGADA` | Verde | `badge-estado-pagada` | #28a745 |
| Factura Devuelta | `FACTURA_DEVUELTA` | Rojo | `badge-estado-devuelta` | #dc3545 |
| Factura Anulada | `FACTURA_ANULADA` | Gris | `badge-estado-anulada` | #6c757d |

> **Nota:** Los estilos están definidos en `/wwwroot/css/site.css`

---

*Documento generado para el Sistema de Honorarios Médicos (SHM) - Grupo San Pablo*
