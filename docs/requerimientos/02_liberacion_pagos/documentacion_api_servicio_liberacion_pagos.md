# Documentación de API: Servicio de Liberación de Pagos

| Parámetro | Detalle |
| :--- | :--- |
| **Servicio** | Servicio de liberación de pagos |
| **Versión** | v1.0.0 |
| **Entorno** | QAS |
| **Fecha** | 05/11/2026 |
| **Autor / Responsable** | Jorge Ramírez |
| **Módulo / Área** | SAP |

---

## 1. Objetivo y Alcance

El presente documento define los lineamientos y especificaciones técnicas para la ejecución, prueba y consumo del servicio mediante la herramienta Postman o clientes HTTP. Permite a desarrolladores, analistas de calidad y consultores técnicos replicar peticiones, validar respuestas y verificar códigos de estado esperados.

---

## 2. Flujo de Integración

Para ejecutar la liberación de pagos en SAP OData se debe seguir la siguiente secuencia:

1. **Obtención de Token OAuth 2.0**: Generar el token de acceso tipo `Bearer`.
2. **Obtención del CSRF Token**: Consumir el endpoint de metadatos OData (`$metadata`) para obtener el token `x-csrf-token` y las cookies de sesión correspondientes.
3. **Liberación de Pago (POST OData)**: Enviar la transacción adjuntando el Bearer Token, CSRF Token, cookies de sesión y la carga útil (payload) JSON.

---

## 3. Especificación de Endpoints

### 3.1. Autenticación (OAuth 2.0)

Genera el token de acceso para consumir los servicios del sistema SAP.

* **Método:** `POST` *(Nota: en la captura de Postman se utiliza el método POST para solicitud de token OAuth)*
* **URL:** `https://190.12.87.190:8124/sap/bc/sec/oauth2/token`

#### A. Authorization
* **Tipo:** Basic Auth
* **Username:** `OZUNIGA_OAUT`
* **Password:** `987654321`

#### B. Headers
| Header | Valor |
| :--- | :--- |
| `Content-Type` | `application/x-www-form-urlencoded` |

#### C. Body (`x-www-form-urlencoded`)
| Parámetro | Valor | Descripción |
| :--- | :--- | :--- |
| `grant_type` | `client_credentials` | Tipo de flujo OAuth 2.0 |
| `scope` | `ZODGS_PORTAL_HHMM_SRV_0001` | Scope del servicio SAP OData |

#### D. Respuesta Esperada
* **Código de Estado:** `200 OK`
* **Formato:** JSON

```json
{
  "access_token": "n-9UoAELH9G2qprNV9CzOhrPwza3-1MseP0LOL1M1vjtJyOK",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "ZODGS_PORTAL_HHMM_SRV_0001"
}
```

---

### 3.2. Obtención de Token CSRF

Realiza una consulta a los metadatos para obtener un token de protección contra ataques CSRF necesario para cualquier método que altere estado (`POST`, `PUT`, `DELETE`).

* **Método:** `GET`
* **URL:** `https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/$metadata`

#### A. Authorization
* **Tipo:** Bearer Token
* **Token:** `<access_token>` *(Obtenido en el paso anterior)*

#### B. Headers
| Header | Valor | Descripción |
| :--- | :--- | :--- |
| `x-csrf-token` | `fetch` | Solicita el token de seguridad CSRF |

#### C. Respuesta Esperada
* **Código de Estado:** `200 OK`
* **Response Headers Clave:**
  * `x-csrf-token`: `9DxkyNgyQMS69YVD70cRvw==` *(Guardar para el paso de liberación)*
  * `content-type`: `application/xml`
  * *Nota:* Mantener las cookies devueltas en la respuesta para la siguiente petición.

---

### 3.3. Liberar Pago

Ejecuta el registro de liberación para los documentos contables asociados a una orden.

* **Método:** `POST`
* **URL:** `https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/LIBERA_PAGO_CABSet`

#### A. Authorization
* **Tipo:** Bearer Token
* **Token:** `<access_token>`

#### B. Headers
| Header | Valor |
| :--- | :--- |
| `Content-Type` | `application/json` |
| `Accept` | `application/json` |
| `x-csrf-token` | `<x-csrf-token>` *(Token obtenido en el paso 3.2)* |

#### C. Estructura de Datos (Diccionario)

| Campo (JSON) | Campo ABAP | Tipo | Long. | Clave / Req. | Descripción y Reglas |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **IdOrden** | `ID_ORDEN` | Edm.String | 20 | Key / Oblig. | Identificador alfanumérico único de la orden |
| **Fecha** | `FECHA` | Edm.String | 10 | Opcional | Fecha de registro del proceso (`YYYY-MM-DD` / `YYYYMMDD`) |
| **DocumentoDt** | `DOCUMENTO_DT` | Edm.String | 10 | Opcional | Número de documento de detracción / orden DT |
| **Sociedad** | `SOCIEDAD` | Edm.String | 4 | Key / Oblig. | Código de sociedad SAP (ej. `1001`) |
| **Documento** | `DOCUMENTO` | Edm.String | 10 | Key / Oblig. | Número de documento contable / compras SAP |
| **Ejercicio** | `EJERCICIO` | Edm.String | 4 | Key / Oblig. | Año del ejercicio fiscal (ej. `2026`) |
| **Referencia** | `REFERENCIA` | Edm.String | 16 | Opcional | Texto de referencia externa o número de factura |
| **Ruc** | `RUC` | Edm.String | 20 | Opcional | Número de identificación tributaria (RUC) |
| **ProveedorName** | `PROVEEDOR_NAME` | Edm.String | 35 | Opcional | Razón social o nombre comercial del proveedor |
| **TipoComprobante** | `TIPO_COMPROBANTE` | Edm.String | 2 | Opcional | Tipo de comprobante (ej. `01`: Factura, `03`: Boleta) |
| **FechaEmision** | `FECHA_EMISION` | Edm.String | 10 | Opcional | Fecha de emisión del comprobante (`YYYY-MM-DD`) |
| **Importe** | `IMPORTE` | Edm.Decimal | 24,3 | Opcional | Monto o importe en moneda local (hasta 3 decimales) |
| **Moneda** | `MONEDA` | Edm.String | 5 | Opcional | Clave de moneda según estándar SAP/ISO (ej. `PEN`, `USD`) |
| **UserLiberador** | `USER_LIBERADOR` | Edm.String | 12 | Opcional | Usuario SAP que ejecuta la liberación |
| **FechaLibera** | `FECHA_LIBERA` | Edm.String | 10 | Opcional | Fecha en que se autoriza/libera (`YYYY-MM-DD`) |
| **HoraLibera** | `HORA_LIBERA` | Edm.String | 10 | Opcional | Hora de liberación (`HH:MM:SS`) |
| **Estado** | `ESTADO` | Edm.String | 10 | Opcional | Estado del registro (ej. `APROBADO`, `RECHAZADO`, `PENDIENTE`) |

#### D. Payload de Entrada (Ejemplo JSON)

```json
{
  "IdOrden": "1234567890",
  "Fecha": "2026-11-05",
  "Documentos": [
    {
      "IdOrden": "1234567890",
      "Sociedad": "1001",
      "Documento": "5100000000",
      "Ejercicio": "2026",
      "DocumentoDt": "1000000001",
      "Referencia": "01-F001-0004521",
      "Ruc": "20100070970",
      "ProveedorName": "SERVICIOS LOGISTICOS GLOBALES S.A.C.",
      "TipoComprobante": "01",
      "FechaEmision": "2026-11-01",
      "Importe": "12500.500",
      "Moneda": "PEN",
      "UserLiberador": "SP_CONSULT",
      "FechaLibera": "2026-11-05",
      "HoraLibera": "15:05:20",
      "Estado": "APROBADO"
    },
    {
      "IdOrden": "1234567890",
      "Sociedad": "1001",
      "Documento": "5100000001",
      "Ejercicio": "2026",
      "DocumentoDt": "1000000002",
      "Referencia": "01-F001-0004522",
      "Ruc": "20555888123",
      "ProveedorName": "DISTRIBUIDORA INDUSTRIAL DEL SUR S.A.",
      "TipoComprobante": "01",
      "FechaEmision": "2026-11-02",
      "Importe": "8320.000",
      "Moneda": "PEN",
      "UserLiberador": "SP_CONSULT",
      "FechaLibera": "2026-11-05",
      "HoraLibera": "15:05:20",
      "Estado": "APROBADO"
    }
  ]
}
```

#### E. Respuesta Esperada
* **Código de Estado:** `201 Created`
* **Formato:** JSON

```json
{
  "d": {
    "__metadata": {
      "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/LIBERA_PAGO_CABSet('')",
      "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/LIBERA_PAGO_CABSet('')",
      "type": "ZODGS_PORTAL_HHMM_SRV.LIBERA_PAGO_CAB"
    },
    "IdOrden": "",
    "Status": "OK",
    "Fecha": "",
    "Mensaje": "Documentos recibidos",
    "Documentos": {
      "results": []
    }
  }
}
```