# Flujo de Estados de Orden de Pago
## Sistema de Honorarios Medicos (SHM)

**Version:** 1.0
**Fecha:** 17 de Febrero 2026
**Autor:** ADG Vladimir D

---

## 1. Descripcion General

Este documento describe el flujo de estados del proceso de Orden de Pago en el Sistema de Honorarios Medicos. La Orden de Pago se genera a partir de liquidaciones aprobadas y requiere un flujo de aprobacion por multiples niveles antes de proceder al pago.

---

## 2. Actores del Sistema

| Actor | Portal | Descripcion |
|-------|--------|-------------|
| **Generador** | Portal HHMM (Administrativo) | Personal que genera la Orden de Pago desde la bandeja de Liquidaciones |
| **Aprobador** | Portal HHMM (Administrativo) | Usuarios asignados en SHM_PERFIL_APROBACION_USUARIO para el flujo FLUJO_APROBACION_ORDEN_PAGO |

---

## 3. Tablas Involucradas

| Tabla | Descripcion |
|-------|-------------|
| `SHM_ORDEN_PAGO` | Cabecera de la orden de pago. Contiene totales, banco, sede y estado general |
| `SHM_ORDEN_PAGO_PRODUCCION` | Relacion entre la orden de pago y las producciones incluidas |
| `SHM_ORDEN_PAGO_LIQUIDACION` | Detalle por liquidacion agrupada dentro de la orden de pago |
| `SHM_ORDEN_PAGO_APROBACION` | Registro de aprobacion por cada nivel/perfil. Controla el flujo de aprobacion |
| `SHM_PERFIL_APROBACION` | Plantilla que define los niveles de aprobacion para FLUJO_APROBACION_ORDEN_PAGO |
| `SHM_PERFIL_APROBACION_USUARIO` | Usuarios asignados a cada perfil de aprobacion por sede |
| `SHM_PRODUCCION` | Producciones cuyo estado cambia a FACTURA_ORDEN_PAGO al generar la orden |

---

## 4. Estados del Proceso

### 4.1 Estados de SHM_ORDEN_PAGO

| Estado | Codigo | Descripcion |
|--------|--------|-------------|
| Aprobacion Pendiente | `APROBACION_PENDIENTE` | Estado inicial. La orden fue generada y esta pendiente de aprobacion |
| Aprobado | `APROBADO` | Todos los niveles de aprobacion han sido completados |
| Devuelto | `DEVUELTO` | Algun aprobador rechazo la orden de pago |

### 4.2 Estados de SHM_ORDEN_PAGO_APROBACION

| Estado | Codigo | Descripcion |
|--------|--------|-------------|
| Aprobacion Pendiente | `APROBACION_PENDIENTE` | El nivel de aprobacion aun no ha sido atendido |
| Aprobado | `APROBADO` | El aprobador aprobo la orden en este nivel |
| Devuelto | `DEVUELTO` | El aprobador rechazo la orden en este nivel |

---

## 5. Diagrama de Flujo

```
    +-------------------------------------------------------------------+
    |                                                                   |
    |   FACTURA_LIQUIDADA (producciones en bandeja de Liquidaciones)     |
    |              |                                                    |
    |              | [Generador selecciona liquidaciones]                |
    |              | [y ejecuta "Generar Orden de Pago"]                |
    |              v                                                    |
    |   +---------------------+                                         |
    |   | APROBACION_PENDIENTE|  <- Estado Inicial de Orden de Pago     |
    |   |  (SHM_ORDEN_PAGO)   |                                         |
    |   +----------+----------+                                         |
    |              |                                                    |
    |              | Cada nivel de SHM_ORDEN_PAGO_APROBACION             |
    |              | debe aprobar en orden secuencial                    |
    |              v                                                    |
    |       +------+------+                                             |
    |       |             |                                             |
    |       v             v                                             |
    |  [Rechazar]    [Aprobar]                                          |
    |       |             |                                             |
    |       v             v                                             |
    |   +---------+   +------------------+                              |
    |   | DEVUELTO|   | APROBADO (nivel) |                              |
    |   +---------+   +--------+---------+                              |
    |       |                  |                                        |
    |       |                  | Todos los niveles aprobados?           |
    |       |           +------+------+                                 |
    |       |           |             |                                 |
    |       |           v             v                                 |
    |       |         [No]          [Si]                                |
    |       |           |             |                                 |
    |       |           v             v                                 |
    |       |   (sigue en          +---------+                          |
    |       |    APROBACION_       | APROBADO|  <- Orden aprobada       |
    |       |    PENDIENTE)        +---------+                          |
    |       |                                                           |
    |       | Se debe volver a generar                                  |
    |       | la Orden de Pago                                          |
    |       v                                                           |
    |   (Vuelve a bandeja de Liquidaciones)                             |
    |                                                                   |
    +-------------------------------------------------------------------+
```

---

## 6. Descripcion Detallada de Cada Evento

### 6.1 Generar Orden de Pago

- **Actor:** Generador (Empresa)
- **Origen:** Bandeja de Liquidaciones (`/Liquidacion/Index`)
- **Controlador:** `LiquidacionController.GenerarOrdenPago`
- **Precondiciones:**
  - Las producciones deben estar en estado `FACTURA_LIQUIDADA`
  - Se debe seleccionar al menos una liquidacion
  - Todas las liquidaciones deben pertenecer al mismo banco

**Acciones ejecutadas:**

| # | Tabla | Accion | Detalle |
|---|-------|--------|---------|
| 1 | `SHM_ORDEN_PAGO` | INSERT | Se crea la cabecera con estado `APROBACION_PENDIENTE`, totales acumulados, banco y sede |
| 2 | `SHM_ORDEN_PAGO_PRODUCCION` | INSERT (bulk) | Un registro por cada produccion incluida en la orden |
| 3 | `SHM_ORDEN_PAGO_LIQUIDACION` | INSERT | Un registro por cada codigo de liquidacion agrupado, con totales y datos de la liquidacion |
| 4 | `SHM_ORDEN_PAGO_APROBACION` | INSERT | Un registro por cada perfil de `SHM_PERFIL_APROBACION` donde `GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'`, todos con estado `APROBACION_PENDIENTE` |
| 5 | `SHM_PRODUCCION` | UPDATE | Estado de las producciones cambia de `FACTURA_LIQUIDADA` a `FACTURA_ORDEN_PAGO` |

**Formato del numero de orden:** `OP-YYYYMMDD-HHmmss`

### 6.2 Rechazar Orden de Pago

- **Actor:** Aprobador
- **Precondiciones:** La orden debe estar en estado `APROBACION_PENDIENTE`

**Acciones ejecutadas:**

| # | Tabla | Accion | Detalle |
|---|-------|--------|---------|
| 1 | `SHM_ORDEN_PAGO_APROBACION` | UPDATE | El registro del aprobador que rechaza pasa a estado `DEVUELTO`. Se registra `ID_USUARIO_APROBADOR` y `FECHA_APROBACION` |
| 2 | `SHM_ORDEN_PAGO` | UPDATE | Estado cambia a `DEVUELTO` |

**Consecuencia:** La orden de pago queda invalidada. Se debe volver a generar una nueva Orden de Pago desde la bandeja de Liquidaciones (paso 6.1).

### 6.3 Aprobar Orden de Pago

- **Actor:** Aprobador
- **Precondiciones:** La orden debe estar en estado `APROBACION_PENDIENTE`

**Acciones ejecutadas:**

| # | Tabla | Accion | Detalle |
|---|-------|--------|---------|
| 1 | `SHM_ORDEN_PAGO_APROBACION` | UPDATE | El registro del aprobador pasa a estado `APROBADO`. Se registra `ID_USUARIO_APROBADOR` y `FECHA_APROBACION` |
| 2 | `SHM_ORDEN_PAGO` | UPDATE (condicional) | Si **todos** los registros de `SHM_ORDEN_PAGO_APROBACION` para esta orden estan en estado `APROBADO`, entonces el estado de la orden cambia a `APROBADO` |

**Regla de negocio:** La orden solo se aprueba completamente cuando el ultimo nivel pendiente da su aprobacion.

---

## 7. Flujo de Aprobacion

El flujo de aprobacion se basa en perfiles configurados en `SHM_PERFIL_APROBACION`:

```
SHM_PERFIL_APROBACION (plantilla)
  GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'
  +------------------+---------------------------+-------+
  | CODIGO           | DESCRIPCION               | ORDEN |
  +------------------+---------------------------+-------+
  | NIVEL_1          | Primer nivel aprobacion    |   1   |
  | NIVEL_2          | Segundo nivel aprobacion   |   2   |
  | ...              | ...                        |  ...  |
  +------------------+---------------------------+-------+

SHM_PERFIL_APROBACION_USUARIO (usuarios asignados por sede)
  +----------------------+------------+---------+
  | ID_PERFIL_APROBACION | ID_USUARIO | ID_SEDE |
  +----------------------+------------+---------+

SHM_ORDEN_PAGO_APROBACION (instancias creadas por orden)
  +---------------+----------------------+-----------------------+
  | ID_ORDEN_PAGO | ID_PERFIL_APROBACION | ESTADO                |
  +---------------+----------------------+-----------------------+
  |     100       |          1           | APROBACION_PENDIENTE  |
  |     100       |          2           | APROBACION_PENDIENTE  |
  +---------------+----------------------+-----------------------+
```

---

## 8. Transiciones de Estado

### SHM_ORDEN_PAGO

| # | Estado Origen | Estado Destino | Accion | Condicion |
|---|---------------|----------------|--------|-----------|
| 1 | (nuevo) | APROBACION_PENDIENTE | Generar Orden de Pago | - |
| 2 | APROBACION_PENDIENTE | DEVUELTO | Algun aprobador rechaza | Cualquier nivel rechaza |
| 3 | APROBACION_PENDIENTE | APROBADO | Ultimo aprobador aprueba | Todos los niveles en estado APROBADO |

### SHM_ORDEN_PAGO_APROBACION

| # | Estado Origen | Estado Destino | Accion | Actor |
|---|---------------|----------------|--------|-------|
| 1 | (nuevo) | APROBACION_PENDIENTE | Generar Orden de Pago | Sistema |
| 2 | APROBACION_PENDIENTE | APROBADO | Aprobar | Aprobador del nivel |
| 3 | APROBACION_PENDIENTE | DEVUELTO | Rechazar | Aprobador del nivel |

### SHM_PRODUCCION (estados relacionados)

| # | Estado Origen | Estado Destino | Accion |
|---|---------------|----------------|--------|
| 1 | FACTURA_LIQUIDADA | FACTURA_ORDEN_PAGO | Generar Orden de Pago |

---

## 9. Estructura de Tablas

### SHM_ORDEN_PAGO
```sql
ID_ORDEN_PAGO         Number NOT NULL    -- PK
ID_SEDE               Number NOT NULL    -- FK a SHM_SEDE
ID_BANCO              Number NOT NULL    -- FK a SHM_BANCO
NUMERO_ORDEN_PAGO     Varchar2(20)       -- Formato: OP-YYYYMMDD-HHmmss
FECHA_GENERACION      Date
ESTADO                Varchar2(30)       -- APROBACION_PENDIENTE | APROBADO | DEVUELTO
MTO_CONSUMO_ACUM      Number
MTO_DESCUENTO_ACUM    Number
MTO_SUBTOTAL_ACUM     Number
MTO_RENTA_ACUM        Number
MTO_IGV_ACUM          Number
MTO_TOTAL_ACUM        Number
CANT_COMPROBANTES     Number
CANT_LIQUIDACIONES    Number
COMENTARIOS           Varchar2(1000)
-- Campos de auditoria estandar
```

### SHM_ORDEN_PAGO_APROBACION
```sql
ID_ORDEN_PAGO_APROBACION  Number NOT NULL   -- PK
ID_ORDEN_PAGO             Number NOT NULL   -- FK a SHM_ORDEN_PAGO
ID_PERFIL_APROBACION      Number NOT NULL   -- FK a SHM_PERFIL_APROBACION
ESTADO                    Varchar2(30)      -- APROBACION_PENDIENTE | APROBADO | DEVUELTO
ID_USUARIO_APROBADOR      Number            -- FK a SHM_SEG_USUARIO (se llena al aprobar/rechazar)
FECHA_APROBACION          Date              -- Se llena al aprobar/rechazar
ORDEN                     Number(3)         -- Orden secuencial del nivel
-- Campos de auditoria estandar
```

### SHM_ORDEN_PAGO_PRODUCCION
```sql
ID_ORDEN_PAGO_PRODUCCION  Number NOT NULL   -- PK
ID_ORDEN_PAGO             Number NOT NULL   -- FK a SHM_ORDEN_PAGO
ID_PRODUCCION             Number NOT NULL   -- FK a SHM_PRODUCCION
-- Campos de auditoria estandar
```

### SHM_ORDEN_PAGO_LIQUIDACION
```sql
ID_ORDEN_PAGO_LIQUIDACION Number NOT NULL   -- PK
ID_ORDEN_PAGO             Number            -- FK a SHM_ORDEN_PAGO
NUMERO_LIQUIDACION        Varchar2(20)
CODIGO_LIQUIDACION        Varchar2(20)
TIPO_LIQUIDACION          Varchar2(5)
DESCRIPCION_LIQUIDACION   Varchar2(300)
PERIODO_LIQUIDACION       Varchar2(10)
ID_BANCO                  Number
MTO_CONSUMO_ACUM          Number
MTO_DESCUENTO_ACUM        Number
MTO_SUBTOTAL_ACUM         Number
MTO_RENTA_ACUM            Number
MTO_IGV_ACUM              Number
MTO_TOTAL_ACUM            Number
CANT_COMPROBANTES         Number
COMENTARIOS               Varchar2(1000)
-- Campos de auditoria estandar
```

---

## 10. Reglas de Negocio

1. **Mismo banco:** Todas las liquidaciones seleccionadas para una orden deben pertenecer al mismo banco
2. **Estado previo:** Solo producciones en estado `FACTURA_LIQUIDADA` pueden incluirse en una orden de pago
3. **Flujo de aprobacion:** Los niveles de aprobacion se crean automaticamente desde `SHM_PERFIL_APROBACION` con `GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'`
4. **Rechazo inmediato:** Si cualquier aprobador rechaza, toda la orden pasa a estado `DEVUELTO`
5. **Aprobacion completa:** La orden solo pasa a `APROBADO` cuando todos los registros de `SHM_ORDEN_PAGO_APROBACION` estan en estado `APROBADO`
6. **Regeneracion:** Una orden devuelta requiere generar una nueva orden de pago desde la bandeja de Liquidaciones
7. **Trazabilidad:** Cada aprobacion/rechazo registra el usuario aprobador y la fecha

---

*Documento generado para el Sistema de Honorarios Medicos (SHM) - Grupo San Pablo*
