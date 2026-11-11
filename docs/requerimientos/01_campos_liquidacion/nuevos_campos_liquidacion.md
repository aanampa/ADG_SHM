# Requerimiento 01 - Nuevos campos en la interfaz de liquidación

| Atributo | Valor |
|----------|-------|
| **Proyecto** | SHM - Sistema de Honorarios Médicos |
| **Componentes** | API REST (`SHM.AppApiHonorarioMedico`), tabla `SHM_PRODUCCION` y Portal Administrativo (`SHM.AppWebHonorarioMedico`) |
| **Endpoint afectado** | `POST /api/produccioninterface/liquidaciones` |
| **Estado** | Implementado. Pendiente: aplicar el parche en cada ambiente y probar con Oracle (sección 9) |

---

## 1. Objetivo

1. Recibir desde la interfaz de liquidaciones los datos SAP de la factura y de su detracción, y guardarlos en `SHM_PRODUCCION`.
2. Mostrar esos datos a Tesorería en la **orden de pago**: en pantalla, en el reporte PDF y en el Excel.

Antes de este cambio, el endpoint solo recibía los datos propios de la liquidación (número, código, periodo, estado, fecha, descripción y tipo).

---

## 2. Decisiones tomadas

| # | Decisión |
|---|----------|
| 1 | El monto y el porcentaje de la detracción **no se crean como campos nuevos**. Se reutilizan `MTO_DETRACCION` y `PORC_DETRACCION`, que ya existen en `SHM_PRODUCCION`. |
| 2 | **SAP es siempre la fuente oficial.** Al llegar la liquidación, `MTO_DETRACCION` y `PORC_DETRACCION` se **sobrescriben** con el valor de SAP. |
| 3 | Solo se agregan 2 columnas nuevas: `CODSAP_CPM` y `CODSAP_DETRAC`. |
| 4 | `CODSAP_CPM` es **opcional**. |
| 5 | Nombres JSON: `codSapCpm`, `codSapDetrac`, `mtoDetraccion` y `porcDetraccion`. |
| 6 | `mtoDetraccion` y `porcDetraccion` llegan como **número JSON con punto decimal**. |
| 7 | **No se exige consistencia** entre el código de detracción y el monto o porcentaje. |
| 8 | **No se requiere** corrección de datos ni carga para producciones ya liquidadas. |
| 9 | "Total S/." **no cambia**. La detracción es solo informativa. |
| 10 | Los 4 datos se muestran en las 4 vistas de la orden de pago, en el PDF y en el Excel. Maquetación aprobada (sección 7.2). |

**Consecuencia de las decisiones 1 y 2.** Antes, `MTO_DETRACCION` y `PORC_DETRACCION` se llenaban solo al registrar la producción (`POST /api/produccioninterface/producciones`) con la detracción calculada por San Pablo. Ahora la liquidación **sobrescribe** ese valor con el de SAP, y el valor original no se conserva. Un emisor que todavía no envíe estos campos dejará la detracción en `0` al liquidar (sección 5.5).

---

## 3. Alcance

**Incluye**
- Agregar 2 columnas a `SHM_PRODUCCION`.
- Aceptar 4 campos en `POST /api/produccioninterface/liquidaciones` y grabarlos.
- Validar los valores recibidos y devolver errores claros por registro.
- Mostrar los datos en el detalle de la orden de pago (4 vistas), en el PDF y en el Excel.
- Actualizar `docs/API_ProduccionInterface.md`.

**No incluye**
- Columnas de detracción en `SHM_ORDEN_PAGO` ni en `SHM_ORDEN_PAGO_LIQUIDACION`. Los totales de detracción se calculan sumando el detalle.
- Cambios en los endpoints de SAP (`SapInterfaceController`).
- Cambios en el Portal de Compañías.
- Corrección o carga de datos de producciones ya liquidadas.

---

## 4. Modelo de datos

Tabla `SHM_PRODUCCION`:

| Columna | Tipo | Estado | Descripción |
|---------|------|--------|-------------|
| `CODSAP_CPM` | `VARCHAR2(18)` | **Nueva** | Código SAP (número de documento) que representa a la factura |
| `CODSAP_DETRAC` | `VARCHAR2(20)` | **Nueva** | Código SAP (número de documento) de la detracción de la factura |
| `MTO_DETRACCION` | `NUMBER` | Existente | Monto de la detracción. Se sobrescribe al liquidar |
| `PORC_DETRACCION` | `NUMBER(5,2)` | Existente | Porcentaje de la detracción. Se sobrescribe al liquidar |

Las dos columnas nuevas son opcionales y sin valor por defecto. En Oracle 11g agregar una columna `NULL` sin `DEFAULT` solo modifica el diccionario de datos y no reescribe la tabla.

Entregables de base de datos:
- Parche: [db/parches/patch_20261105_campos_liquidacion_sap.sql](../../../db/parches/patch_20261105_campos_liquidacion_sap.sql).
- `db/02_create_tables.sql` actualizado con las 2 columnas.

---

## 5. Contrato del API

Se agregan 4 propiedades al elemento de la lista que recibe `POST /api/produccioninterface/liquidaciones` (`UpdateInterfaceLiquidacionDto`). Para el monto y el porcentaje se usan los mismos nombres que ya usa el endpoint de producciones.

| Propiedad JSON | Columna | Tipo JSON | Obligatorio | Reglas |
|----------------|---------|-----------|-------------|--------|
| `codSapCpm` | `CODSAP_CPM` | string | No | Máx. 18 caracteres. Se aplica `Trim()` |
| `codSapDetrac` | `CODSAP_DETRAC` | string | No | Máx. 20 caracteres. Se aplica `Trim()` |
| `mtoDetraccion` | `MTO_DETRACCION` | número | No | Mayor o igual a 0, hasta 2 decimales |
| `porcDetraccion` | `PORC_DETRACCION` | número | No | Entre 0 y 100, hasta 2 decimales |

### 5.1 Factura afecta a detracción

```json
{
  "codigoSede": "01",
  "codigoEntidad": "E001",
  "codigoProduccion": "P001",
  "numeroProduccion": "000123",
  "tipoEntidadMedica": "C",
  "numeroLiquidacion": "L000045",
  "codigoLiquidacion": "LIQ-2026-045",
  "periodoLiquidacion": "202610",
  "estadoLiquidacion": "LIQUIDADO",
  "fechaLiquidacion": "28/10/2026 10:30:00",
  "descripcionLiquidacion": "Liquidación octubre",
  "tipoLiquidacion": "1",
  "codSapCpm": "5100012345",
  "codSapDetrac": "5200067890",
  "mtoDetraccion": 1180.50,
  "porcDetraccion": 12.00
}
```

### 5.2 Factura no afecta a detracción

Los campos de detracción pueden omitirse, venir `null` o venir vacíos:

```json
{
  "...": "...",
  "codSapCpm": "5100012345",
  "codSapDetrac": null,
  "mtoDetraccion": null,
  "porcDetraccion": null
}
```

### 5.3 Reglas de grabado

SAP es la fuente oficial: los valores recibidos siempre sobrescriben los existentes.

| Caso recibido | Valor que se guarda |
|---------------|---------------------|
| `codSapCpm` informado | El valor con `Trim()` |
| `codSapCpm` nulo, vacío o solo espacios | `NULL` |
| `codSapDetrac` informado | El valor con `Trim()` |
| `codSapDetrac` nulo, vacío o solo espacios | `NULL` |
| `mtoDetraccion` o `porcDetraccion` con valor | Ese valor |
| `mtoDetraccion` o `porcDetraccion` nulo o ausente | `0` |

### 5.4 Validaciones y respuesta

Las validaciones se hacen por registro, antes de consultar la base de datos, y no abortan el resto del lote. Un registro inválido se devuelve con estado `ER` y un mensaje.

| Condición | Mensaje |
|-----------|---------|
| `codSapCpm` supera 18 caracteres | `codSapCpm excede la longitud maxima de 18 caracteres` |
| `codSapDetrac` supera 20 caracteres | `codSapDetrac excede la longitud maxima de 20 caracteres` |
| `mtoDetraccion` negativo | `mtoDetraccion no puede ser negativo` |
| `mtoDetraccion` con más de 2 decimales | `mtoDetraccion admite como maximo 2 decimales` |
| `porcDetraccion` fuera del rango 0-100 | `porcDetraccion debe estar entre 0 y 100` |
| `porcDetraccion` con más de 2 decimales | `porcDetraccion admite como maximo 2 decimales` |

No se valida la consistencia entre `codSapDetrac`, `mtoDetraccion` y `porcDetraccion` (decisión 7).

La estructura de respuesta (`InterfaceProduccionResultDto`) no cambia.

### 5.5 Compatibilidad

- Los 4 campos son opcionales, así que un emisor que no los envíe no recibe error.
- **Efecto sobre la detracción:** como SAP es la fuente oficial y el valor se sobrescribe, un emisor que no envíe `mtoDetraccion` y `porcDetraccion` deja la detracción en `0`, aunque la producción tuviera un valor al registrarse. Conviene que el equipo emisor los envíe desde el primer día.
- Se mantiene la regla actual: solo se actualizan producciones en estado `FACTURA_ENVIADA_HHMM`. Una producción ya liquidada responde `ER`, así que **estos datos no se pueden corregir reenviando la liquidación**. Las órdenes de pago con producciones liquidadas antes de este cambio mostrarán `-` en los códigos SAP (decisión 8).

---

## 6. Impacto sobre el uso actual de la detracción

`MTO_DETRACCION` y `PORC_DETRACCION` ya se usan en estos lugares:

| Dónde | Uso | Efecto del cambio |
|-------|-----|-------------------|
| `POST /api/produccioninterface/producciones` | Se guardan al registrar la producción | Ninguno |
| Portal de Compañías, `Facturas/Subir.cshtml` | Muestra a la compañía el monto y el porcentaje que debe consignar en su factura | Ninguno si la pantalla solo aplica antes de liquidar |
| Portal de Compañías, `Facturas/_ValidacionPartial.cshtml` | Compara el valor de la producción con la detracción del XML y bloquea el envío si no coinciden (según el parámetro `SHM_COMPROBANTE_VALIDA_DETRACCION`) | Ninguno si solo aplica antes de liquidar |
| Portal Administrativo, `Produccion/Detalle.cshtml` | Muestra el monto de la detracción | Pasa a mostrar el valor de SAP después de liquidar |

**Por verificar:** que ninguna pantalla del Portal de Compañías permita subir o validar una factura de una producción ya liquidada. Si existiera, la validación compararía el XML contra el valor de SAP y no contra el original.

---

## 7. Visualización en la orden de pago

### 7.1 Datos y salidas

Una sola consulta, `GetDetalleLiquidacionesByOrdenPagoIdAsync`, y un solo DTO, `DetalleLiquidacionItemDto`, alimentan todas las salidas.

| Salida | Archivo | Cambio |
|--------|---------|--------|
| Consulta | `Repositories/OrdenPagoLiquidacionRepository.cs` | Agrega `CODSAP_CPM`, `CODSAP_DETRAC`, `MTO_DETRACCION` y `PORC_DETRACCION` |
| DTO | `DTOs/OrdenPagoLiquidacion/DetalleLiquidacionItemDto.cs` | 4 propiedades y `TieneDetraccion` |
| Detalle de orden (usuario) | `Views/OrdenPago/Detalle.cshtml` | Tabla de comprobantes |
| Aprobación | `Views/OrdenPagoAprobacion/Detalle.cshtml` | Igual |
| Orden aprobada | `Views/OrdenPagoAprobacion/DetalleAprobada.cshtml` | Igual |
| Tesorería | `Views/Tesoreria/Detalle.cshtml` | Igual, y el JavaScript de totales por fila visible |
| PDF | `Reports/OrdenPagoDocument.cs` (`ComposeDetalle`) | Columna de detracción y código SAP bajo el comprobante |
| Excel de la orden | `TesoreriaController.DescargarExcelOrdenPago` | 17 columnas. La columna "Detracción S/." deja de estar fija en `0` |
| Excel de listado | `TesoreriaController.ExportarExcel`, hoja "Detalle de Liquidaciones" | 20 columnas, mismo detalle |

### 7.2 Maquetación

| Salida | Maquetación |
|--------|-------------|
| Vistas y PDF | `CODSAP_CPM` bajo el comprobante, como `SAP: <código>`. Una sola columna "Detracción S/." antes del Total, con el monto y, debajo, el porcentaje y `SAP: <código>` |
| Excel | Columnas separadas: `Cód. SAP Factura` (tras el comprobante) y, antes del Total, `Cód. SAP Detracción`, `% Detracción` y `Detracción S/.` |

### 7.3 Reglas de presentación

- Una factura **tiene detracción** si `CODSAP_DETRAC` está informado o `MTO_DETRACCION` es mayor a 0 (propiedad `TieneDetraccion`). Esto cubre producciones liquidadas antes del cambio que ya tenían monto de detracción pero no código SAP.
- Si no tiene detracción, se muestra `-` en la columna de detracción. En el Excel, el monto `0` se muestra como `-` mediante el formato numérico, y la suma sigue siendo numérica.
- Si `CODSAP_CPM` o `CODSAP_DETRAC` es `NULL`, no se muestra la línea `SAP:` en vistas y PDF, y se muestra `-` en el Excel.
- La fila de totales suma `MTO_DETRACCION` del detalle. En Tesorería, el total se recalcula con las filas visibles al filtrar por liquidación.
- "Total S/." no cambia (decisión 9).

---

## 8. Impacto técnico (implementado)

| Capa | Archivo | Cambio |
|------|---------|--------|
| BD | `db/parches/patch_20261105_campos_liquidacion_sap.sql` y `db/02_create_tables.sql` | Agrega `CODSAP_CPM` y `CODSAP_DETRAC` |
| Dominio | `Entities/Produccion.cs` | 2 propiedades: `CodSapCpm` y `CodSapDetrac` |
| Dominio | `DTOs/Produccion/UpdateInterfaceLiquidacionDto.cs` | `CodSapCpm`, `CodSapDetrac`, `MtoDetraccion` y `PorcDetraccion` |
| Dominio | `Interfaces/Repositories/IProduccionRepository.cs` | Nuevos parámetros en `UpdateLiquidacionByKeyAsync` |
| Dominio | `DTOs/OrdenPagoLiquidacion/DetalleLiquidacionItemDto.cs` | 4 propiedades y `TieneDetraccion` |
| Aplicación | `Services/ProduccionInterfaceService.cs` | `ValidarDatosSapLiquidacion` y paso de valores al repositorio |
| Infraestructura | `Repositories/ProduccionRepository.cs` | `UPDATE` de `UpdateLiquidacionByKeyAsync` con las 4 columnas, y las 2 columnas nuevas en el `SELECT` base |
| Infraestructura | `Repositories/OrdenPagoLiquidacionRepository.cs` | 4 columnas en la consulta del detalle |
| Presentación | 4 vistas, `OrdenPagoDocument.cs` y `TesoreriaController.cs` | Según la sección 7 |
| Documentación | `docs/API_ProduccionInterface.md` | Campos, reglas y ejemplo con factura afecta y no afecta |

---

## 9. Despliegue y pruebas pendientes

**Orden de despliegue.** Aplicar el parche de base de datos **antes** de publicar el código. El `SELECT` base de `ProduccionRepository` ahora incluye `CODSAP_CPM` y `CODSAP_DETRAC`. Con el código nuevo y sin el parche, las consultas de producciones fallan con `ORA-00904: invalid identifier`.

**Pruebas pendientes con Oracle** (la solución compila sin errores, pero no se probó contra la base de datos):
1. Aplicar el parche y verificar la consulta de columnas del PASO 3.
2. Enviar una liquidación con los 4 campos y comprobar lo grabado en `SHM_PRODUCCION`.
3. Enviar una liquidación sin los campos y comprobar que la detracción queda en `0` y los códigos en `NULL`.
4. Enviar valores inválidos (longitud, rango, decimales) y comprobar el `ER` por registro.
5. Abrir una orden de pago en las 4 vistas, descargar el PDF y los dos Excel, con y sin datos SAP.
6. En Tesorería, filtrar por liquidación y comprobar que el total de detracción se recalcula.

---

## 10. Criterios de aceptación

**Base de datos**
1. El parche crea `CODSAP_CPM VARCHAR2(18)` y `CODSAP_DETRAC VARCHAR2(20)` en `SHM_PRODUCCION`, sin alterar las filas existentes.

**API**
2. Una liquidación con los 4 campos válidos queda `OK`, y `CODSAP_CPM`, `CODSAP_DETRAC`, `MTO_DETRACCION` y `PORC_DETRACCION` quedan grabados con los valores recibidos.
3. Una liquidación sin los campos de detracción queda `OK`, con `MTO_DETRACCION = 0`, `PORC_DETRACCION = 0` y `CODSAP_DETRAC = NULL`, aunque la producción tuviera detracción antes.
4. Una liquidación sin `codSapCpm` queda `OK` con `CODSAP_CPM = NULL`.
5. Un valor que excede la longitud, un porcentaje fuera de rango, un monto negativo o más de 2 decimales devuelven `ER` con el mensaje de la sección 5.4, sin afectar a los demás registros del lote.
6. Una producción que no está en `FACTURA_ENVIADA_HHMM` sigue respondiendo `ER` y no modifica ningún campo.

**Visualización**
7. El detalle de la orden de pago muestra el código SAP de la factura, el código SAP de la detracción, el porcentaje y el monto en las 4 vistas, en el PDF y en el Excel.
8. En el Excel de la orden, la columna "Detracción S/." muestra `MTO_DETRACCION` y deja de estar fija en `0`.
9. Una producción sin datos SAP ni detracción muestra `-` y no rompe ninguna salida.
10. La fila de totales del PDF, del Excel y de las vistas suma el monto de detracción del detalle, y "Total S/." no cambia.
11. El PDF mantiene el formato A4 horizontal y no corta columnas.

**Documentación**
12. `docs/API_ProduccionInterface.md` documenta los campos nuevos con un ejemplo de factura afecta y otro de factura no afecta a detracción.
