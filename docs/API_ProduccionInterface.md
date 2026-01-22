# API ProduccionInterface

## Descripción General

API para el registro masivo de producciones médicas a través de una interfaz de integración. Permite crear múltiples registros de producción en una sola llamada, validando duplicados y manejando transacciones de forma segura.

---

## Información del Endpoint

| Atributo | Valor |
|----------|-------|
| **URL** | `POST /api/ProduccionInterface` |
| **Método HTTP** | POST |
| **Content-Type** | application/json |
| **Autenticación** | No requerida (versión actual) |

---

## Request

### Estructura del Body

El endpoint recibe un **array JSON** de objetos con la siguiente estructura:

```json
[
  {
    "codigoSede": "string",
    "codigoEntidad": "string",
    "codigoProduccion": "string",
    "tipoProduccion": "string",
    "tipoMedico": "string",
    "tipoRubro": "string",
    "descripcion": "string",
    "periodo": "string",
    "estadoProduccion": "string",
    "fechaCreacion": "string",
    "mtoConsumo": 0.00,
    "mtoDescuento": 0.00,
    "mtoSubtotal": 0.00,
    "mtoRenta": 0.00,
    "mtoIgv": 0.00,
    "mtoTotal": 0.00,
    "concepto": "string"
  }
]
```

### Descripción de Campos

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| `codigoSede` | string | Sí | Código de la sede. Se utiliza para obtener el IdSede interno. |
| `codigoEntidad` | string | Sí | Código de la entidad médica. Se utiliza para obtener el IdEntidadMedica interno. |
| `codigoProduccion` | string | Sí | Código único de la producción. |
| `tipoProduccion` | string | Sí | Tipo de producción (ej: "CONS", "PROC"). |
| `tipoMedico` | string | Sí | Tipo de médico (ej: "IN", "EX"). |
| `tipoRubro` | string | Sí | Tipo de rubro. |
| `descripcion` | string | Sí | Descripción de la producción. |
| `periodo` | string | Sí | Período de la producción (ej: "202601"). |
| `estadoProduccion` | string | Sí | Estado de la producción (ej: "ACTIVO", "PENDIENTE"). |
| `fechaCreacion` | string | Sí | Fecha de creación en formato texto (ej: "2026-01-19"). |
| `mtoConsumo` | decimal | Sí | Monto de consumo. |
| `mtoDescuento` | decimal | Sí | Monto de descuento. |
| `mtoSubtotal` | decimal | Sí | Monto subtotal. |
| `mtoRenta` | decimal | Sí | Monto de renta. |
| `mtoIgv` | decimal | Sí | Monto de IGV. |
| `mtoTotal` | decimal | Sí | Monto total. |
| `concepto` | string | Sí | Concepto de la producción. |

### Llave Única

La combinación de los siguientes campos forma la **llave única** para identificar registros duplicados:

- `codigoSede` (convertido a IdSede)
- `codigoEntidad` (convertido a IdEntidadMedica)
- `codigoProduccion`

Si un registro con esta llave ya existe en la base de datos, será **omitido** (no se creará duplicado).

---

## Response

### Respuesta Exitosa (HTTP 201 Created)

```json
{
  "cantidadCreados": 8,
  "cantidadObviados": 2,
  "totalProcesados": 10
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `cantidadCreados` | int | Número de registros creados exitosamente. |
| `cantidadObviados` | int | Número de registros omitidos por ya existir (duplicados). |
| `totalProcesados` | int | Total de registros procesados (creados + obviados). |

### Respuesta de Error - Validación (HTTP 400 Bad Request)

**Colección vacía:**
```json
{
  "message": "La coleccion de producciones no puede estar vacia"
}
```

**Sede no encontrada:**
```json
{
  "message": "Sede con codigo 'SEDE001' no encontrada"
}
```

**Entidad médica no encontrada:**
```json
{
  "message": "Entidad medica con codigo 'ENT001' no encontrada"
}
```

**Validación de campos obligatorios:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "[0].CodigoSede": ["The CodigoSede field is required."],
    "[0].MtoTotal": ["The MtoTotal field is required."]
  }
}
```

### Respuesta de Error - Servidor (HTTP 500 Internal Server Error)

```json
{
  "message": "Error interno del servidor"
}
```

---

## Comportamiento Transaccional

El API implementa un comportamiento **transaccional** para garantizar la integridad de los datos:

1. **Fase de Validación**: Se validan todos los registros antes de crear cualquiera.
   - Se verifica que exista cada `codigoSede`
   - Se verifica que exista cada `codigoEntidad`
   - Se identifican los registros duplicados

2. **Fase de Creación**: Los registros válidos se crean dentro de una transacción.
   - Si ocurre un error durante la creación, **se revierte toda la operación**
   - Ningún registro se guarda si hay un fallo

### Diagrama de Flujo

```
┌─────────────────────────────────────────────────────────────┐
│                    Recibe Colección                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              FASE 1: VALIDACIÓN                             │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Para cada registro:                                    │ │
│  │   • Buscar IdSede por CodigoSede                      │ │
│  │   • Buscar IdEntidadMedica por CodigoEntidad          │ │
│  │   • Verificar si existe por llave compuesta           │ │
│  │     - Si existe → Marcar como "obviado"               │ │
│  │     - Si no existe → Agregar a lista de creación      │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              │ ¿Error en validación?         │
              └───────────────┬───────────────┘
                    Sí │             │ No
                       ▼             ▼
              ┌─────────────┐ ┌─────────────────────────────────┐
              │ HTTP 400    │ │      FASE 2: CREACIÓN           │
              │ Bad Request │ │  ┌─────────────────────────────┐ │
              └─────────────┘ │  │ BEGIN TRANSACTION           │ │
                              │  │   Para cada registro válido:│ │
                              │  │     • Crear en BD           │ │
                              │  │   Si error → ROLLBACK       │ │
                              │  │   Si OK → COMMIT            │ │
                              │  └─────────────────────────────┘ │
                              └─────────────────────────────────┘
                                            │
                                            ▼
                              ┌─────────────────────────────────┐
                              │         HTTP 201 Created        │
                              │  {                              │
                              │    cantidadCreados: N,          │
                              │    cantidadObviados: M,         │
                              │    totalProcesados: N+M         │
                              │  }                              │
                              └─────────────────────────────────┘
```

---

## Ejemplos de Uso

### Ejemplo 1: Creación exitosa de múltiples registros

**Request:**
```http
POST /api/ProduccionInterface HTTP/1.1
Host: localhost:5000
Content-Type: application/json

[
  {
    "codigoSede": "SEDE001",
    "codigoEntidad": "MED001",
    "codigoProduccion": "PROD0001",
    "tipoProduccion": "CONS",
    "tipoMedico": "IN",
    "tipoRubro": "RUB01",
    "descripcion": "Consulta médica general",
    "periodo": "202601",
    "estadoProduccion": "ACTIVO",
    "fechaCreacion": "2026-01-19",
    "mtoConsumo": 150.00,
    "mtoDescuento": 0.00,
    "mtoSubtotal": 150.00,
    "mtoRenta": 15.00,
    "mtoIgv": 27.00,
    "mtoTotal": 162.00,
    "concepto": "HONORARIO"
  },
  {
    "codigoSede": "SEDE001",
    "codigoEntidad": "MED002",
    "codigoProduccion": "PROD0002",
    "tipoProduccion": "PROC",
    "tipoMedico": "EX",
    "tipoRubro": "RUB02",
    "descripcion": "Procedimiento quirúrgico menor",
    "periodo": "202601",
    "estadoProduccion": "PENDIENTE",
    "fechaCreacion": "2026-01-19",
    "mtoConsumo": 500.00,
    "mtoDescuento": 50.00,
    "mtoSubtotal": 450.00,
    "mtoRenta": 45.00,
    "mtoIgv": 81.00,
    "mtoTotal": 486.00,
    "concepto": "HONORARIO"
  }
]
```

**Response (201 Created):**
```json
{
  "cantidadCreados": 2,
  "cantidadObviados": 0,
  "totalProcesados": 2
}
```

### Ejemplo 2: Algunos registros duplicados

**Request:** (enviando registros donde algunos ya existen)

**Response (201 Created):**
```json
{
  "cantidadCreados": 5,
  "cantidadObviados": 3,
  "totalProcesados": 8
}
```

### Ejemplo 3: Error por sede inexistente

**Request:**
```json
[
  {
    "codigoSede": "SEDE_INVALIDA",
    "codigoEntidad": "MED001",
    "codigoProduccion": "PROD0003",
    ...
  }
]
```

**Response (400 Bad Request):**
```json
{
  "message": "Sede con codigo 'SEDE_INVALIDA' no encontrada"
}
```

---

## Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| **201 Created** | Operación exitosa. Retorna el resumen de registros procesados. |
| **400 Bad Request** | Error de validación: campos faltantes, sede/entidad no encontrada, o colección vacía. |
| **500 Internal Server Error** | Error interno del servidor. |

---

## Consideraciones Importantes

1. **Idempotencia**: El API es seguro para reintentos. Si se envía la misma colección múltiples veces, los registros duplicados serán omitidos automáticamente.

2. **Atomicidad**: Si ocurre un error durante la creación de cualquier registro, **ningún registro se guarda**. La operación es todo o nada.

3. **Validación previa**: Todos los códigos de sede y entidad médica son validados antes de iniciar la creación, evitando creaciones parciales.

4. **Rendimiento**: Para grandes volúmenes de datos, considere enviar lotes de máximo 1000 registros por llamada.

---

## Información Técnica

| Atributo | Valor |
|----------|-------|
| **Autor** | ADG Antonio |
| **Fecha de Creación** | 2026-01-19 |
| **Versión** | 1.0 |
| **Proyecto** | SHM.AppApiHonorarioMedico |

---

## Changelog

| Versión | Fecha | Descripción |
|---------|-------|-------------|
| 1.0 | 2026-01-19 | Versión inicial del API |
