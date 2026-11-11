<div align="center">

<br><br><br>

<img src="../../images/adg_logo.png" alt="ADG Systems" width="320">

<br><br><br><br>

# Guía de Despliegue

## Cambios realizados y puesta en producción

### Requerimiento 01
**Nuevos campos SAP en la interfaz de liquidación y en la orden de pago**

<br><br><br>

**SHM - Sistema de Honorarios Médicos**

Complejo Hospitalario San Pablo

<br><br><br>

| | |
|---|---|
| **Documento** | Guía de Despliegue |
| **Requerimiento** | 01 - Campos de liquidación |
| **Cliente** | Complejo Hospitalario San Pablo |
| **Desarrollado por** | ADG Systems |
| **Framework** | ASP.NET Core 8 |
| **Base de datos** | Oracle 11g |
| **Versión** | 1.0 |
| **Fecha** | Noviembre 2026 |

</div>

<div style="page-break-after: always;"></div>

---

# Guía de Despliegue - Requerimiento 01

| Atributo | Valor |
|----------|-------|
| **Proyecto** | SHM - Sistema de Honorarios Médicos |
| **Requerimiento** | Nuevos campos SAP en la interfaz de liquidación y en la orden de pago |
| **Especificación** | [nuevos_campos_liquidacion.md](nuevos_campos_liquidacion.md) |
| **Fecha de desarrollo** | 2026-11-05 |
| **Rama** | `antonio_v1` |
| **Commit** | Pendiente (anotar aquí el hash al hacer el commit) |
| **Compilación** | Correcta (0 errores) |
| **Pruebas con Oracle** | **Pendientes** (ver sección 9) |

---

## 1. Resumen

**Qué cambia para el usuario**
- El API de liquidaciones recibe 4 datos nuevos de SAP: código de la factura, código de la detracción, monto de la detracción y porcentaje de la detracción.
- La orden de pago muestra esos datos a Tesorería en pantalla, en el PDF y en el Excel.

**Qué cambia en la base de datos**
- Se agregan 2 columnas a `SHM_PRODUCCION`: `CODSAP_CPM` y `CODSAP_DETRAC`.
- El monto y el porcentaje de la detracción usan las columnas existentes `MTO_DETRACCION` y `PORC_DETRACCION`.

**Qué no cambia**
- No hay cambios de configuración (`appsettings.json`), de parámetros del sistema ni de paquetes NuGet.
- No hay migración de datos.
- "Total S/." de la orden de pago no cambia. La detracción es solo informativa.

---

## 2. Componentes a publicar

Los tres proyectos web comparten las bibliotecas `SHM.AppDomain`, `SHM.AppApplication` y `SHM.AppInfrastructure`, y las tres tuvieron cambios.

| Componente | Ruta en el servidor 192.1.0.173 | Puerto | ¿Publicar? | Motivo |
|------------|-------------------------------|--------|------------|--------|
| **API REST** (`SHM.AppApiHonorarioMedico`) | `D:\appweb\shmappapi` | 92 | **Sí** | Recibe los datos nuevos en `POST /api/produccioninterface/liquidaciones` |
| **Portal Administrativo** (`SHM.AppWebHonorarioMedico`) | `D:\appweb\shmappwebadmin` | 91 | **Sí** | Vistas, PDF y Excel de la orden de pago |
| **Portal de Compañías** (`SHM.AppWebCompaniaMedica`) | `D:\appweb\shmappwebcompania` | 93 | No es necesario | No tiene cambios funcionales. Puede quedar con la versión anterior (ver sección 6) |

Application Pools: `shm_api_pool` y `shm_webadmin_pool` (ver `docs/despliegue_solucion.md`).

---

## 3. Base de datos

### 3.1 Parche

Archivo: [db/parches/patch_20261105_campos_liquidacion_sap.sql](../../../db/parches/patch_20261105_campos_liquidacion_sap.sql)

```sql
ALTER TABLE SHM_PRODUCCION ADD CODSAP_CPM VARCHAR2(18)
/
ALTER TABLE SHM_PRODUCCION ADD CODSAP_DETRAC VARCHAR2(20)
/
COMMENT ON COLUMN SHM_PRODUCCION.CODSAP_CPM IS 'Codigo SAP (numero de documento) que representa a la factura'
/
COMMENT ON COLUMN SHM_PRODUCCION.CODSAP_DETRAC IS 'Codigo SAP (numero de documento) que representa a la detraccion de la factura'
/
```

| Aspecto | Detalle |
|---------|---------|
| Usuario | `shm_dev` (esquema dueño de la tabla) |
| Tipo de cambio | Solo agrega columnas `NULL`, sin `DEFAULT` |
| Impacto | En Oracle 11g solo modifica el diccionario de datos. No reescribe la tabla ni bloquea por tiempo prolongado |
| Compatibilidad | El código anterior sigue funcionando con las columnas nuevas, porque no las usa |
| Datos existentes | No se modifican |
| Repetible | **No.** Ejecutarlo dos veces falla con `ORA-01430` (columna ya existe) |

### 3.2 Verificación del parche

El mismo parche incluye la consulta de verificación (PASO 3). Resultado esperado: 4 filas.

```sql
SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE
  FROM USER_TAB_COLUMNS
 WHERE TABLE_NAME = 'SHM_PRODUCCION'
   AND (COLUMN_NAME LIKE 'CODSAP%' OR COLUMN_NAME LIKE '%DETRAC%')
 ORDER BY COLUMN_NAME;
```

| COLUMN_NAME | DATA_TYPE | Longitud / precisión | NULLABLE |
|-------------|-----------|----------------------|----------|
| `CODSAP_CPM` | VARCHAR2 | 18 | Y |
| `CODSAP_DETRAC` | VARCHAR2 | 20 | Y |
| `MTO_DETRACCION` | NUMBER | - | Y |
| `PORC_DETRACCION` | NUMBER | precisión 5, escala 2 | Y |

### 3.3 Respaldo recomendado antes del despliegue

Con este cambio, la liquidación **sobrescribe** `MTO_DETRACCION` y `PORC_DETRACCION` con el valor de SAP, y el valor original que registró San Pablo no se conserva. Si se desea poder consultarlo después, crear antes un respaldo:

```sql
CREATE TABLE SHM_PRODUCCION_DETRAC_BK_20261105 AS
SELECT ID_PRODUCCION, MTO_DETRACCION, PORC_DETRACCION, ESTADO
  FROM SHM_PRODUCCION;
```

Es opcional y no forma parte del parche. Eliminar la tabla de respaldo cuando ya no se necesite.

---

## 4. Orden de despliegue

1. **Respaldo:** copiar las carpetas actuales `D:\appweb\shmappapi` y `D:\appweb\shmappwebadmin` (para poder volver atrás).
2. **Base de datos:** aplicar el parche y verificar con la consulta de la sección 3.2.
3. **Publicar el API REST** (`shm_api_pool`).
4. **Publicar el Portal Administrativo** (`shm_webadmin_pool`).
5. **Pruebas posteriores** (sección 8).
6. **Avisar al equipo que envía las liquidaciones** (sección 7).

> **El parche va siempre antes que el código.** El `SELECT` base de `ProduccionRepository` ahora incluye `CODSAP_CPM` y `CODSAP_DETRAC`. Con el código nuevo y sin el parche, las consultas de producciones fallan con `ORA-00904: invalid identifier`.

---

## 5. Cambios por archivo

### 5.1 Base de datos
| Archivo | Cambio |
|---------|--------|
| `db/parches/patch_20261105_campos_liquidacion_sap.sql` | **Nuevo.** Parche con las 2 columnas |

### 5.2 Dominio (`SHM.AppDomain`)
| Archivo | Cambio |
|---------|--------|
| `Entities/Produccion.cs` | Propiedades `CodSapCpm` y `CodSapDetrac` |
| `DTOs/Produccion/UpdateInterfaceLiquidacionDto.cs` | Propiedades de entrada `CodSapCpm`, `CodSapDetrac`, `MtoDetraccion` y `PorcDetraccion` (todas opcionales) |
| `Interfaces/Repositories/IProduccionRepository.cs` | `UpdateLiquidacionByKeyAsync` recibe 4 parámetros nuevos |
| `DTOs/OrdenPagoLiquidacion/DetalleLiquidacionItemDto.cs` | 4 propiedades nuevas y `TieneDetraccion` (código SAP de detracción informado, o monto mayor a 0) |

### 5.3 Aplicación (`SHM.AppApplication`)
| Archivo | Cambio |
|---------|--------|
| `Services/ProduccionInterfaceService.cs` | Método nuevo `ValidarDatosSapLiquidacion`. `UpdateLiquidacionesAsync` valida y normaliza los datos antes de consultar la base de datos y los pasa al repositorio |

### 5.4 Infraestructura (`SHM.AppInfrastructure`)
| Archivo | Cambio |
|---------|--------|
| `Repositories/ProduccionRepository.cs` | El `UPDATE` de `UpdateLiquidacionByKeyAsync` graba `CODSAP_CPM`, `CODSAP_DETRAC`, `MTO_DETRACCION` y `PORC_DETRACCION`. El `SELECT` base incluye `CODSAP_CPM` y `CODSAP_DETRAC` |
| `Repositories/OrdenPagoLiquidacionRepository.cs` | La consulta del detalle de la orden agrega `CODSAP_CPM`, `CODSAP_DETRAC`, `MTO_DETRACCION` y `PORC_DETRACCION` |

### 5.5 Portal Administrativo (`SHM.AppWebHonorarioMedico`)
| Archivo | Cambio |
|---------|--------|
| `Views/OrdenPago/Detalle.cshtml` | Tabla de comprobantes: código SAP bajo el comprobante y columna "Detracción S/." antes del Total, con total en el pie |
| `Views/OrdenPagoAprobacion/Detalle.cshtml` | Igual |
| `Views/OrdenPagoAprobacion/DetalleAprobada.cshtml` | Igual |
| `Views/Tesoreria/Detalle.cshtml` | Igual. Además se corrigió el JavaScript de `recalcularTotales`, que suma por índice de columna: Sub Total 13, IGV 14, Imp. Renta 15, Detracción 16 y Total 17 (ver sección 7, punto 7) |
| `Reports/OrdenPagoDocument.cs` | PDF: código SAP de la factura bajo el comprobante, columna "Detracción S/." (monto, porcentaje y código SAP) y su total |
| `Controllers/TesoreriaController.cs` | Excel de la orden (`DescargarExcelOrdenPago`) y hoja "Detalle de Liquidaciones" del Excel de listado (`ExportarExcel`) |

### 5.6 Documentación
| Archivo | Cambio |
|---------|--------|
| `docs/API_ProduccionInterface.md` | Campos nuevos del endpoint de liquidaciones, reglas y ejemplo con factura afecta y no afecta a detracción |
| `docs/requerimientos/01_campos_liquidacion/nuevos_campos_liquidacion.md` | **Nuevo.** Especificación del requerimiento |
| `docs/requerimientos/01_campos_liquidacion/req_01_guia_despliegue.md` | **Nuevo.** Este documento |

---

## 6. Comportamiento nuevo

### 6.1 API `POST /api/produccioninterface/liquidaciones`

Campos nuevos, todos opcionales:

| Campo JSON | Columna | Reglas |
|------------|---------|--------|
| `codSapCpm` | `CODSAP_CPM` | Máx. 18 caracteres |
| `codSapDetrac` | `CODSAP_DETRAC` | Máx. 20 caracteres. Vacío si la factura no es afecta |
| `mtoDetraccion` | `MTO_DETRACCION` | Mayor o igual a 0, hasta 2 decimales, punto decimal |
| `porcDetraccion` | `PORC_DETRACCION` | Entre 0 y 100, hasta 2 decimales, punto decimal |

**Reglas de grabado**
- SAP es la fuente oficial: los valores recibidos **siempre sobrescriben** los existentes.
- `mtoDetraccion` o `porcDetraccion` nulos o ausentes se guardan como `0`.
- Los códigos se guardan sin espacios al inicio ni al final. Un código vacío se guarda como `NULL`.

**Validaciones por registro.** Un valor inválido devuelve `ER` con un mensaje y no afecta al resto del lote:
- `codSapCpm excede la longitud maxima de 18 caracteres`
- `codSapDetrac excede la longitud maxima de 20 caracteres`
- `mtoDetraccion no puede ser negativo`
- `mtoDetraccion admite como maximo 2 decimales`
- `porcDetraccion debe estar entre 0 y 100`
- `porcDetraccion admite como maximo 2 decimales`

No se valida la consistencia entre el código de detracción y el monto o porcentaje.

**Sin cambios:** solo se actualizan producciones en estado `FACTURA_ENVIADA_HHMM`. La respuesta del endpoint mantiene la misma estructura.

### 6.2 Orden de pago

| Salida | Qué muestra |
|--------|-------------|
| **Vistas** (4) | Código SAP de la factura (`SAP: ...`) bajo el comprobante. Columna "Detracción S/." antes del Total, con el monto, el porcentaje y el código SAP de la detracción. Total de detracción en el pie |
| **PDF** | Igual que las vistas, con total de detracción en "TOTAL GENERAL" |
| **Excel de la orden** | 17 columnas: `Cód. SAP Factura`, y antes del Total `Cód. SAP Detracción`, `% Detracción` y `Detracción S/.` |
| **Excel de listado, hoja "Detalle de Liquidaciones"** | 20 columnas, con las mismas 4 columnas nuevas |

**Reglas de presentación**
- Una factura **tiene detracción** si el código SAP de detracción está informado o el monto es mayor a 0.
- Si no tiene detracción, se muestra `-`. En el Excel, un monto `0` se muestra como `-` por formato, y la suma sigue siendo numérica.
- Los códigos SAP vacíos no se muestran en vistas y PDF, y aparecen como `-` en el Excel.
- "Total S/." no cambia.

### 6.3 Portal de Compañías

No se modificó, y puede quedar con la versión anterior. Al ser un cambio de solo agregar columnas, el código anterior sigue funcionando contra la base de datos con el parche aplicado. Si se decide publicarlo igualmente, el parche debe estar aplicado antes.

---

## 7. Riesgos y puntos de atención

1. **Orden de despliegue.** Publicar el código sin el parche rompe las consultas de producciones (`ORA-00904`). Ver sección 4.
2. **La detracción se sobrescribe y no se puede revertir.** Después de liquidar, el valor original de San Pablo se pierde. Ver el respaldo opcional de la sección 3.3.
3. **Emisores que no envían los campos.** Si el equipo que envía las liquidaciones no incluye `mtoDetraccion` y `porcDetraccion`, la detracción de la producción queda en `0` al liquidar, aunque tuviera un valor al registrarse. **Avisarles antes del despliegue** y compartirles `docs/API_ProduccionInterface.md`.
4. **Las columnas de los Excel cambian de posición.** El Excel de la orden pasa de 14 a 17 columnas y la hoja de detalle del listado de 16 a 20. Quien use macros, fórmulas o procesos que lean estos archivos por número de columna debe actualizarlos.
5. **Órdenes anteriores.** Las producciones liquidadas antes de este cambio no recibirán los códigos SAP, porque el API no actualiza producciones ya liquidadas. Sus órdenes de pago mostrarán `-` en los códigos. Si ya tenían monto de detracción, este se muestra igual.
6. **Pendiente de verificar.** Que ninguna pantalla del Portal de Compañías permita subir o validar una factura de una producción ya liquidada. Si existiera, la validación del XML compararía contra el valor de SAP y no contra el original.
7. **Corrección en Tesorería fuera del alcance original.** El recálculo de totales al filtrar por liquidación (`Tesoreria/Detalle.cshtml`) leía los índices de columna 11 a 14. Los valores reales eran 13 a 16, porque las columnas PDF y XML se agregaron después sin actualizar el script. Es decir, los totales del pie ya salían desfasados al filtrar, antes de este requerimiento. Al agregar la columna de detracción se corrigieron todos los índices (13 a 17). Quien use esa pantalla verá que, al filtrar, los totales ahora coinciden con las filas visibles.
8. **Sin pruebas contra Oracle.** El código compila, pero no se ejecutó contra la base de datos. Las pruebas de la sección 8 son obligatorias antes de pasar a producción.

---

## 8. Pruebas posteriores al despliegue

### 8.1 Base de datos
- [ ] La consulta de la sección 3.2 devuelve las 4 filas esperadas.

### 8.2 API
Usar el Ejemplo 2 de `docs/API_ProduccionInterface.md`, con producciones de prueba en estado `FACTURA_ENVIADA_HHMM`.

- [ ] Liquidación con los 4 campos: responde `OK`.
  ```sql
  SELECT ID_PRODUCCION, ESTADO, NUMERO_LIQUIDACION, CODSAP_CPM, CODSAP_DETRAC,
         MTO_DETRACCION, PORC_DETRACCION
    FROM SHM_PRODUCCION
   WHERE NUMERO_LIQUIDACION = '<numero enviado>';
  ```
  Los 4 valores deben coincidir con lo enviado y el estado debe ser `FACTURA_LIQUIDADA`.
- [ ] Liquidación sin los campos de detracción: `OK`, con `MTO_DETRACCION = 0`, `PORC_DETRACCION = 0` y `CODSAP_DETRAC = NULL`.
- [ ] Liquidación sin ninguno de los 4 campos: `OK`, sin error.
- [ ] Un código de 19 caracteres en `codSapCpm`: `ER` con el mensaje de longitud, y el resto del lote se procesa.
- [ ] `porcDetraccion: 101` y `mtoDetraccion: -1`: `ER` con su mensaje.
- [ ] Una producción que no está en `FACTURA_ENVIADA_HHMM`: sigue respondiendo `ER`.

### 8.3 Portal Administrativo
Abrir una orden de pago que tenga producciones liquidadas con datos SAP, y otra sin ellos.

- [ ] **Detalle de orden** (`OrdenPago/Detalle`), **aprobación** (`OrdenPagoAprobacion/Detalle`), **orden aprobada** (`DetalleAprobada`) y **Tesorería** (`Tesoreria/Detalle`): muestran el código SAP bajo el comprobante y la columna "Detracción S/." con el pie sumado.
- [ ] Una factura sin detracción muestra `-` y no `0.00`.
- [ ] En **Tesorería**, filtrar por una liquidación: el total de detracción se recalcula con las filas visibles y los demás totales siguen correctos.
- [ ] **PDF** de la orden: formato A4 horizontal, sin columnas cortadas, con la columna de detracción y su total.
- [ ] **Excel de la orden:** 17 columnas, "Detracción S/." con el monto real, total en la fila final, y los rangos combinados del título abarcan todas las columnas.
- [ ] **Excel de listado (Tesorería, Exportar):** la hoja "Detalle de Liquidaciones" tiene 20 columnas con los datos correctos.
- [ ] "Total S/." no cambió respecto a antes del despliegue para la misma orden.

---

## 9. Rollback

**Código.** Restaurar las carpetas respaldadas en el paso 1 de la sección 4 (`D:\appweb\shmappapi` y `D:\appweb\shmappwebadmin`) y reciclar los Application Pools. El código anterior funciona con las columnas nuevas presentes, por lo que **no es necesario** revertir la base de datos.

**Base de datos (solo si se requiere).** Primero revertir el código y después:

```sql
ALTER TABLE SHM_PRODUCCION DROP COLUMN CODSAP_CPM;
ALTER TABLE SHM_PRODUCCION DROP COLUMN CODSAP_DETRAC;
```

Esto elimina los códigos SAP ya recibidos. No restaura `MTO_DETRACCION` ni `PORC_DETRACCION`: para eso se necesita el respaldo de la sección 3.3.

---

## 10. Contacto y referencias

- Especificación: [nuevos_campos_liquidacion.md](nuevos_campos_liquidacion.md)
- Documentación del API: `docs/API_ProduccionInterface.md`
- Guía general de despliegue: `docs/despliegue_solucion.md`
- Desarrollador: ADG Antonio
