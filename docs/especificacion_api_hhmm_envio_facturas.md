# Requerimiento de Servicio API-REST - Envio de Comprobantes

---

## Informacion del Proyecto

<table>
<tr>
<td width="50%" align="center">

![San Pablo](images/sanpablo_logo.png)

**Cliente**

**Complejo Hospitalario San Pablo**

</td>
<td width="50%" align="center">

![ADG Systems](images/adg_logo.png)

**Solicitado por**

**ADG Systems**

</td>
</tr>
</table>

| Atributo | Valor |
|----------|-------|
| **Proyecto** | Gestion de Honorarios Medicos (HHMM) |
| **Modulo** | Envio de Comprobantes de Pago |
| **Cliente** | Complejo Hospitalario San Pablo |
| **Solicitante** | ADG Systems |
| **Version** | 1.0 |
| **Fecha** | 2026-02-06 |

---

## Objetivo

El sistema de Honorarios Medicos (HHMM) requiere un servicio API-REST que permita recibir los datos de comprobantes de pago (facturas electronicas) asociados a registros de produccion medica.

El sistema HHMM sera el **consumidor** de este API, enviando los datos de los comprobantes emitidos por las compañias medicas para que sean procesados y registrados en el sistema destino.

### Contexto

Actualmente los comprobantes se cargan manualmente uno a uno desde el portal web de compañias medicas. Se requiere este API para habilitar el **envio masivo** de comprobantes de forma programatica, permitiendo la integracion entre sistemas.

---

## Especificacion del Servicio Requerido

### Informacion del Endpoint

| Atributo | Valor |
|----------|-------|
| **Metodo HTTP** | POST |
| **Content-Type** | application/json |
| **Formato de datos** | JSON |
| **Tipo de envio** | Masivo (array de registros) |

> **Nota:** La URL base y autenticacion seran definidos por el equipo responsable del API.

---

## Datos de Entrada (Request)

El sistema HHMM enviara un **array JSON** con los datos de los comprobantes:

```json
[
  {
    "codigoSede": "string",
    "codigoEntidad": "string",
    "codigoProduccion": "string",
    "numeroProduccion": "string",
    "tipoEntidadMedica": "string",
    "tipoComprobante": "string",
    "rucEmisor": "string",
    "rucReceptor": "string",
    "serie": "string",
    "numero": "string",
    "fechaEmision": "string",
    "glosa": "string",
    "importe": 0.00,
    "moneda": "string"
  }
]
```

### Descripcion de Campos

| # | Campo | Tipo | Obligatorio | Max. Longitud | Descripcion |
|---|-------|------|:-----------:|:-------------:|-------------|
| 1 | `codigoSede` | string | Si | 5 | Codigo de la sede / clinica |
| 2 | `codigoEntidad` | string | Si | 8 | Codigo de la entidad medica (compañia medica) |
| 3 | `codigoProduccion` | string | Si | 8 | Codigo de la produccion asociada al comprobante |
| 4 | `numeroProduccion` | string | Si | 10 | Numero de la produccion |
| 5 | `tipoEntidadMedica` | string | Si | 2 | Tipo de entidad medica (ej: "1", "2") |
| 6 | `tipoComprobante` | string | Si | 2 | Tipo de comprobante electronico (ver catalogo abajo) |
| 7 | `rucEmisor` | string | Si | 11 | RUC de la compañia medica que emite el comprobante |
| 8 | `rucReceptor` | string | Si | 11 | RUC del receptor (sede / clinica) |
| 9 | `serie` | string | Si | 4 | Serie del comprobante (ej: "E001", "F001") |
| 10 | `numero` | string | Si | 10 | Numero del comprobante (ej: "00000017") |
| 11 | `fechaEmision` | string | Si | 10 | Fecha de emision en formato `dd/MM/yyyy` (ej: "10/02/2026") |
| 12 | `glosa` | string | No | 300 | Descripcion o glosa del comprobante |
| 13 | `importe` | decimal | Si | 15,2 | Importe total del comprobante |
| 14 | `moneda` | string | Si | 3 | Moneda del comprobante (ver catalogo abajo) |

### Catalogo: Tipo de Comprobante

| Codigo | Descripcion |
|--------|-------------|
| `01` | Factura |
| `02` | Recibo por Honorarios |
| `03` | Boleta de Venta |

### Catalogo: Moneda

| Codigo | Descripcion |
|--------|-------------|
| `PEN` | Soles |
| `USD` | Dolares Americanos |

### Llave de Identificacion de la Produccion

La combinacion de los siguientes campos identifica de forma unica la produccion a la cual se asocia el comprobante:

| # | Campo |
|---|-------|
| 1 | `codigoSede` |
| 2 | `codigoEntidad` |
| 3 | `codigoProduccion` |
| 4 | `numeroProduccion` |
| 5 | `tipoEntidadMedica` |

---

## Datos de Salida (Response Esperado)

El sistema HHMM espera recibir una respuesta con el resultado del procesamiento de cada registro enviado.

### Estructura del Response

```json
{
  "isSuccess": true,
  "message": "Correcto.",
  "data": {
    "cantidadProcesados": 0,
    "cantidadExitosos": 0,
    "cantidadErrores": 0,
    "detalle": [
      {
        "codigoSede": "string",
        "codigoEntidad": "string",
        "codigoProduccion": "string",
        "numeroProduccion": "string",
        "tipoEntidadMedica": "string",
        "estado": "OK",
        "mensaje": "string"
      }
    ]
  },
  "errors": []
}
```

### Descripcion de Campos del Response

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `isSuccess` | boolean | `true` si la operacion fue procesada, `false` si hubo error general |
| `message` | string | Mensaje descriptivo del resultado general |
| `data.cantidadProcesados` | int | Total de registros recibidos y procesados |
| `data.cantidadExitosos` | int | Cantidad de registros procesados correctamente |
| `data.cantidadErrores` | int | Cantidad de registros con error |
| `data.detalle` | array | Detalle del resultado por cada registro enviado |
| `errors` | array | Lista de errores generales (ej: coleccion vacia, error de formato) |

### Descripcion de Campos del Detalle

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `codigoSede` | string | Codigo de sede del registro procesado (eco del request) |
| `codigoEntidad` | string | Codigo de entidad del registro procesado (eco del request) |
| `codigoProduccion` | string | Codigo de produccion del registro procesado (eco del request) |
| `numeroProduccion` | string | Numero de produccion del registro procesado (eco del request) |
| `tipoEntidadMedica` | string | Tipo de entidad medica (eco del request) |
| `estado` | string | `OK` = Procesado correctamente, `ER` = Error |
| `mensaje` | string | Mensaje descriptivo. En caso de error, el detalle del motivo |

---

## Ejemplo de Invocacion

### Request

```http
POST /api/[ruta-definida-por-el-equipo] HTTP/1.1
Content-Type: application/json

[
  {
    "codigoSede": "01",
    "codigoEntidad": "0994",
    "codigoProduccion": "20251017",
    "numeroProduccion": "20251039",
    "tipoEntidadMedica": "1",
    "tipoComprobante": "01",
    "rucEmisor": "20123456789",
    "rucReceptor": "20100055237",
    "serie": "E001",
    "numero": "00000017",
    "fechaEmision": "10/02/2026",
    "glosa": "HONORARIOS MEDICOS - SEGUNDA SEMANA OCTUBRE 2025",
    "importe": 677.59,
    "moneda": "PEN"
  },
  {
    "codigoSede": "01",
    "codigoEntidad": "0995",
    "codigoProduccion": "20251055",
    "numeroProduccion": "20251055",
    "tipoEntidadMedica": "1",
    "tipoComprobante": "02",
    "rucEmisor": "10987654321",
    "rucReceptor": "20100055237",
    "serie": "E001",
    "numero": "00000042",
    "fechaEmision": "10/02/2026",
    "glosa": "HONORARIOS MEDICOS - OCTUBRE 2025",
    "importe": 184.00,
    "moneda": "PEN"
  }
]
```

### Response Esperado (Exitoso)

```json
{
  "isSuccess": true,
  "message": "Correcto.",
  "data": {
    "cantidadProcesados": 2,
    "cantidadExitosos": 2,
    "cantidadErrores": 0,
    "detalle": [
      {
        "codigoSede": "01",
        "codigoEntidad": "0994",
        "codigoProduccion": "20251017",
        "numeroProduccion": "20251039",
        "tipoEntidadMedica": "1",
        "estado": "OK",
        "mensaje": "Comprobante registrado"
      },
      {
        "codigoSede": "01",
        "codigoEntidad": "0995",
        "codigoProduccion": "20251055",
        "numeroProduccion": "20251055",
        "tipoEntidadMedica": "1",
        "estado": "OK",
        "mensaje": "Comprobante registrado"
      }
    ]
  },
  "errors": []
}
```

### Response Esperado (Con Errores Parciales)

```json
{
  "isSuccess": true,
  "message": "Correcto.",
  "data": {
    "cantidadProcesados": 2,
    "cantidadExitosos": 1,
    "cantidadErrores": 1,
    "detalle": [
      {
        "codigoSede": "01",
        "codigoEntidad": "0994",
        "codigoProduccion": "20251017",
        "numeroProduccion": "20251039",
        "tipoEntidadMedica": "1",
        "estado": "OK",
        "mensaje": "Comprobante registrado"
      },
      {
        "codigoSede": "XX",
        "codigoEntidad": "0995",
        "codigoProduccion": "20251055",
        "numeroProduccion": "20251055",
        "tipoEntidadMedica": "1",
        "estado": "ER",
        "mensaje": "Produccion no encontrada"
      }
    ]
  },
  "errors": []
}
```

### Response Esperado (Error General)

```json
{
  "isSuccess": false,
  "message": "Error de validacion.",
  "data": null,
  "errors": ["La coleccion de comprobantes no puede estar vacia"]
}
```

---

## Validaciones Sugeridas

Se sugiere que el API implemente las siguientes validaciones:

| # | Validacion | Descripcion |
|---|-----------|-------------|
| 1 | Campos obligatorios | Todos los campos marcados como obligatorios deben estar presentes |
| 2 | Formato de fecha | `fechaEmision` debe tener formato `dd/MM/yyyy` |
| 3 | Fecha no futura | `fechaEmision` no debe ser posterior a la fecha actual |
| 4 | Produccion existente | La produccion identificada por la llave compuesta debe existir |
| 5 | RUC Emisor valido | `rucEmisor` debe ser un RUC valido (11 digitos) |
| 6 | RUC Receptor valido | `rucReceptor` debe ser un RUC valido (11 digitos) |
| 7 | Importe positivo | `importe` debe ser mayor a cero |

---


## Codigos de Estado HTTP Esperados

| Codigo | Cuando se usa |
|--------|---------------|
| **200 OK** | Operacion procesada correctamente (puede contener errores parciales en `detalle`) |
| **400 Bad Request** | Error de validacion general (coleccion vacia, campos obligatorios faltantes) |
| **401 Unauthorized** | Credenciales de autenticacion invalidas (si aplica) |
| **500 Internal Server Error** | Error interno del servidor |

---


## Informacion del Documento

| Atributo | Valor |
|----------|-------|
| **Autor** | ADG Antonio |
| **Fecha de Creacion** | 2026-02-06 |
| **Version** | 1.0 |

---

## Changelog

| Version | Fecha | Descripcion |
|---------|-------|-------------|
| 1.0 | 2026-02-10 | Version inicial del requerimiento |
