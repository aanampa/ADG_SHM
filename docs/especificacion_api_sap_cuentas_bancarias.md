# Requerimiento de Servicios API-REST - SAP
## Sistema de Honorarios Medicos (SHM) - Grupo San Pablo

---

## 1. Objetivo

Solicitar al proveedor de SAP el desarrollo de servicios API-REST que permitan al Sistema de Honorarios Medicos (SHM) consultar:
- Informacion de cuentas bancarias de acreedores
- Catalogo de bancos
- Estado de pago de facturas

Estos servicios son necesarios para el proceso de pago de honorarios medicos.

---

## 2. Informacion General

| Campo | Valor |
|-------|-------|
| **Sistema Consumidor** | SHM - Sistema de Honorarios Medicos |
| **Formato de Datos Esperado** | JSON |
| **Codificacion** | UTF-8 |

> **Nota:** Las URLs de los endpoints, nombres de campos en el JSON, mecanismo de autenticacion y seguridad seran definidos por el proveedor SAP.

---

## 3. Servicios Requeridos

### 3.1 Servicio 1: Obtener Cuentas Bancarias por Acreedor

**Descripcion:** Obtener las cuentas bancarias registradas en SAP para un acreedor especifico.

#### Parametro de Entrada

| Parametro | Tipo | Obligatorio | Descripcion |
|-----------|------|-------------|-------------|
| Codigo Acreedor | string | Si | Codigo del acreedor en SAP |

#### Datos de Salida Requeridos

| Dato | Descripcion |
|------|-------------|
| Codigo Acreedor | Codigo del acreedor en SAP |
| Cuenta Corriente | Numero de cuenta corriente |
| Cuenta Corriente Interbancaria | Codigo de Cuenta Interbancario (CCI) |
| Moneda | Codigo de moneda (PEN, USD) |
| Codigo de Banco | Codigo del banco |
| Descripcion de Banco | Nombre del banco |

#### Ejemplo de Respuesta Esperada (Referencial)

```json
{
    "data": [
        {
            "codigoAcreedor": "0000012345",
            "cuentaCorriente": "19100123456789",
            "cuentaCorrienteInterbancaria": "00219100123456789012",
            "moneda": "PEN",
            "codigoBanco": "002",
            "descripcionBanco": "BANCO DE CREDITO DEL PERU"
        }
    ]
}
```

---

### 3.2 Servicio 2: Obtener Datos de Banco por Codigo

**Descripcion:** Obtener la informacion de un banco especifico dado su codigo.

#### Parametro de Entrada

| Parametro | Tipo | Obligatorio | Descripcion |
|-----------|------|-------------|-------------|
| Codigo de Banco | string | Si | Codigo del banco |

#### Datos de Salida Requeridos

| Dato | Descripcion |
|------|-------------|
| Codigo de Banco | Codigo del banco |
| Descripcion de Banco | Nombre completo del banco |

#### Ejemplo de Respuesta Esperada (Referencial)

```json
{
    "data": {
        "codigoBanco": "002",
        "descripcionBanco": "BANCO DE CREDITO DEL PERU"
    }
}
```

---

### 3.3 Servicio 3: Listar Todos los Bancos

**Descripcion:** Obtener el catalogo completo de bancos disponibles en SAP.

#### Parametro de Entrada

Ninguno

#### Datos de Salida Requeridos

| Dato | Descripcion |
|------|-------------|
| Codigo de Banco | Codigo del banco |
| Descripcion de Banco | Nombre completo del banco |

#### Ejemplo de Respuesta Esperada (Referencial)

```json
{
    "data": [
        {
            "codigoBanco": "002",
            "descripcionBanco": "BANCO DE CREDITO DEL PERU"
        },
        {
            "codigoBanco": "003",
            "descripcionBanco": "INTERBANK"
        }
    ]
}
```

---

### 3.4 Servicio 4: Obtener Datos de Pago de una Factura

**Descripcion:** Consultar el estado y datos del pago realizado para una factura especifica de un acreedor.

#### Parametros de Entrada

| Parametro | Tipo | Obligatorio | Descripcion |
|-----------|------|-------------|-------------|
| Codigo Acreedor | string | Si | Codigo del acreedor en SAP |
| Tipo Comprobante | string | Si | Tipo de comprobante (Factura, Boleta, etc.) |
| Serie | string | Si | Serie del comprobante |
| Numero Comprobante | string | Si | Numero del comprobante |

#### Datos de Salida Requeridos

| Dato | Descripcion |
|------|-------------|
| Codigo Acreedor | Codigo del acreedor |
| Tipo Comprobante | Tipo de comprobante |
| Serie | Serie del comprobante |
| Numero Comprobante | Numero del comprobante |
| Estado Pago | Estado del pago (Pendiente, Pagado, Anulado, etc.) |
| Fecha Pago | Fecha en que se realizo el pago |
| Numero Operacion | Numero de operacion bancaria |
| Banco | Banco desde donde se realizo el pago |
| Cuenta Bancaria de Deposito | Cuenta donde se deposito el pago |
| Monto Pagado | Importe total pagado |

#### Ejemplo de Respuesta Esperada (Referencial)

```json
{
    "data": {
        "codigoAcreedor": "0000012345",
        "tipoComprobante": "01",
        "serie": "F001",
        "numeroComprobante": "00012345",
        "estadoPago": "PAGADO",
        "fechaPago": "2026-01-15",
        "numeroOperacion": "OP-2026-0001234",
        "banco": "BANCO DE CREDITO DEL PERU",
        "cuentaBancariaDeposito": "19100123456789",
        "montoPagado": 5250.00
    }
}
```

---

## 4. Resumen de Servicios

| # | Servicio | Parametros de Entrada | Datos de Salida |
|---|----------|----------------------|-----------------|
| 1 | Cuentas Bancarias por Acreedor | Codigo Acreedor | Cuentas bancarias con banco y moneda |
| 2 | Datos de Banco | Codigo Banco | Codigo y descripcion del banco |
| 3 | Lista de Bancos | Ninguno | Catalogo completo de bancos |
| 4 | Datos de Pago de Factura | Codigo Acreedor, Tipo Comprobante, Serie, Numero | Estado pago, fecha, operacion, banco, cuenta, monto |

---

## 5. Consideraciones Tecnicas (Para el Proveedor)

### 5.1 Definiciones a Cargo del Proveedor SAP

El proveedor SAP debera definir y documentar:
- [ ] Nombres de los campos en el JSON de request/response
- [ ] Estructura de respuestas de error
- [ ] Codigos de estado HTTP a utilizar
- [ ] Politicas de seguridad y acceso

### 5.2 Requerimientos del Sistema Consumidor

| Aspecto | Requerimiento |
|---------|---------------|
| Formato | JSON |
| Protocolo | HTTPS |


---

## 6. Entregables Esperados del Proveedor

1. **Documento de Especificacion Tecnica** con:
   - URLs de endpoints
   - Estructura exacta de request/response (JSON)
   - Mecanismo de autenticacion
   - Codigos de error y su significado

---

