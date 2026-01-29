# Flujo de Estados de Facturación
## Sistema de Honorarios Médicos (SHM)

**Versión:** 1.1
**Fecha:** 25 de Enero 2026
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

| Estado | Código | Descripción |
|--------|--------|-------------|
| Factura Pendiente | `FACTURA_PENDIENTE` | Estado inicial. La producción está registrada pero no se ha solicitado factura |
| Factura Solicitada | `FACTURA_SOLICITADA` | La empresa ha solicitado la factura estableciendo una fecha límite |
| Factura Enviada | `FACTURA_ENVIADA` | La Cía Médica ha registrado y enviado los documentos de factura |
| Factura Aceptada | `FACTURA_ACEPTADA` | La empresa ha revisado y aprobado la factura |
| Factura Devuelta | `FACTURA_DEVUELTA` | La empresa ha rechazado la factura por observaciones |
| Factura Liquidada | `FACTURA_LIQUIDADA` | La factura está en proceso de liquidación |
| Factura Pagada | `FACTURA_PAGADA` | El pago ha sido realizado |

---

## 4. Diagrama de Flujo

```
    ┌───────────────────────────────────────────────────────────────────┐
    │                                                                   │
    │   ┌─────────────────────┐                                         │
    │   │  FACTURA_PENDIENTE  │  ← Estado Inicial                       │
    │   │   (Estado Inicial)  │                                         │
    │   └──────────┬──────────┘                                         │
    │              │                                                    │
    │              │ [Empresa solicita factura]                         │
    │              │ (establece fecha límite)                           │
    │              │ ✉ Notifica por correo a Cía Médica                 │
    │              ▼                                                    │
    │   ┌─────────────────────┐                                         │
    │   │ FACTURA_SOLICITADA  │                                         │
    │   └──────────┬──────────┘                                         │
    │              │                                                    │
    │              │ [Cía Médica envía documentos]                      │
    │              │ (PDF, XML, CDR)                                    │
    │              ▼                                                    │
    │   ┌─────────────────────┐                                         │
    │   │  FACTURA_ENVIADA    │◄──────────────────────┐                 │
    │   └──────────┬──────────┘                       │                 │
    │              │                                  │                 │
    │       ┌──────┴──────┐                           │                 │
    │       │             │                           │                 │
    │       ▼             ▼                           │                 │
    │  [Rechazar]    [Aprobar]                        │                 │
    │       │             │                           │                 │
    │       ▼             ▼                           │                 │
    │   ┌─────────┐   ┌─────────────┐                 │                 │
    │   │DEVUELTA │   │  ACEPTADA   │                 │                 │
    │   └────┬────┘   └──────┬──────┘                 │                 │
    │        │               │                        │                 │
    │        │               │ [Procesar liquidación] │                 │
    │        │               ▼                        │                 │
    │        │        ┌─────────────┐                 │                 │
    │        │        │  LIQUIDADA  │                 │                 │
    │        │        └──────┬──────┘                 │                 │
    │        │               │                        │                 │
    │        │               │ [Registrar pago]       │                 │
    │        │               ▼                        │                 │
    │        │        ┌─────────────┐                 │                 │
    │        │        │   PAGADA    │  ← Estado Final │                 │
    │        │        └─────────────┘                 │                 │
    │        │                                        │                 │
    │        │ [Cía Médica reenvía]                   │                 │
    │        │ (dentro del plazo)                     │                 │
    │        └────────────────────────────────────────┘                 │
    │                                                                   │
    └───────────────────────────────────────────────────────────────────┘
```

---

## 5. Transiciones de Estado

| # | Estado Origen | Estado Destino | Acción | Actor | Notifica |
|---|---------------|----------------|--------|-------|----------|
| 1 | FACTURA_PENDIENTE | FACTURA_SOLICITADA | Solicitar Factura | Empresa | ✉ Cía Médica |
| 2 | FACTURA_SOLICITADA | FACTURA_SOLICITADA | Actualizar Fecha Límite | Empresa | - |
| 3 | FACTURA_SOLICITADA | FACTURA_ENVIADA | Enviar Factura | Cía Médica | - |
| 4 | FACTURA_ENVIADA | FACTURA_ACEPTADA | Aceptar Factura | Empresa | - |
| 5 | FACTURA_ENVIADA | FACTURA_DEVUELTA | Devolver Factura | Empresa | - |
| 6 | FACTURA_DEVUELTA | FACTURA_ENVIADA | Reenviar Factura | Cía Médica | - |
| 7 | FACTURA_ACEPTADA | FACTURA_LIQUIDADA | Procesar Liquidación | Empresa | - |
| 8 | FACTURA_LIQUIDADA | FACTURA_PAGADA | Registrar Pago | Empresa | - |

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
- **Documentos requeridos:**
  - PDF de la factura
  - XML electrónico
  - CDR (Constancia de Recepción)
- **Acciones disponibles:** Aceptar o Devolver
- **Actor responsable:** Empresa

### 6.4 FACTURA_ACEPTADA
- **Descripción:** La factura ha sido revisada y aprobada
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
4. **Trazabilidad:** Cada cambio de estado registra fecha, hora y usuario que realizó la acción
5. **Notificación por Correo:** Al solicitar factura, se notifica automáticamente a la Cía Médica

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

| Estado | Color | Clase CSS | Código Color |
|--------|-------|-----------|--------------|
| FACTURA_PENDIENTE | Amarillo | `badge-estado-pendiente` | #ffc107 |
| FACTURA_SOLICITADA | Celeste | `badge-estado-solicitada` | #17a2b8 |
| FACTURA_ENVIADA | Azul | `badge-estado-enviada` | #007bff |
| FACTURA_ACEPTADA | Verde Azulado | `badge-estado-aceptada` | #20c997 |
| FACTURA_LIQUIDADA | Morado | `badge-estado-liquidada` | #6f42c1 |
| FACTURA_PAGADA | Verde | `badge-estado-pagada` | #28a745 |
| FACTURA_DEVUELTA | Rojo | `badge-estado-devuelta` | #dc3545 |

> **Nota:** Los estilos están definidos en `/wwwroot/css/site.css`

---

*Documento generado para el Sistema de Honorarios Médicos (SHM) - Grupo San Pablo*
