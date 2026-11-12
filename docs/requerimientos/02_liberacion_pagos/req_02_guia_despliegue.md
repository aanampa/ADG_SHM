<div align="center">

<br><br><br>

<img src="../../images/adg_logo.png" alt="ADG Systems" width="320">

<br><br><br><br>

# Guía de Despliegue

## Cambios realizados y puesta en producción

### Requerimiento 02
**Liberación de pagos en SAP al aprobar la orden de pago**

<br><br><br>

**SHM - Sistema de Honorarios Médicos**

Complejo Hospitalario San Pablo

<br><br><br>

| | |
|---|---|
| **Documento** | Guía de Despliegue |
| **Requerimiento** | 02 - Liberación de pagos |
| **Cliente** | Complejo Hospitalario San Pablo |
| **Desarrollado por** | ADG Systems |
| **Framework** | ASP.NET Core 8 |
| **Base de datos** | Oracle 11g |
| **Versión** | 1.0 |
| **Fecha** | Noviembre 2026 |

</div>

<div style="page-break-after: always;"></div>

---

# Guía de Despliegue - Requerimiento 02

## 1. Qué se instala

Al aprobar el último nivel de una orden de pago, el sistema libera en SAP todos los comprobantes de la orden. Si SAP falla, la aprobación se mantiene y Tesorería puede reintentar desde el detalle de la orden.

La pantalla **Sedes** tiene un campo nuevo, **Sociedad SAP**.

No hay cambios en `appsettings.json`: se usan las credenciales y la URL de SAP que ya existen en la sección `SapApi`.

**Requisito:** el parche del Requerimiento 01 ya debe estar aplicado (columnas `CODSAP_CPM` y `CODSAP_DETRAC` en `SHM_PRODUCCION`). No volver a ejecutarlo.

---

## 2. Qué publicar

| Componente | Ruta en el servidor 192.1.0.173 | Application Pool | ¿Publicar? |
|------------|-------------------------------|------------------|------------|
| **Portal Administrativo** | `D:\appweb\shmappwebadmin` | `shm_webadmin_pool` | **Sí** |
| **API REST** | `D:\appweb\shmappapi` | `shm_api_pool` | **Sí** |
| Portal de Compañías | `D:\appweb\shmappwebcompania` | - | No |

El servidor de aplicaciones debe tener conexión con el servidor SAP de **producción**. El servicio se probó en QAS: confirmar con el equipo SAP que está habilitado en producción con el mismo usuario y scope.

---

## 3. Pasos de instalación

1. **Respaldo:** copiar las carpetas `D:\appweb\shmappwebadmin` y `D:\appweb\shmappapi`.
2. **Aplicar los parches** (sección 4), como `shm_dev`, **antes** de publicar el código.
3. **Cargar el código de sociedad de cada sede** (sección 5).
4. **Publicar el Portal Administrativo.**
5. **Publicar el API REST.**

---

## 4. Parches de base de datos

Ejecutar en este orden. Cada parche trae su consulta de verificación al final. **No son repetibles.**

| # | Archivo | Qué hace | Verificación |
|---|---------|----------|--------------|
| 1 | `db/parches/patch_20261111_sede_codigo_sociedad_sap.sql` | Agrega la columna `CODIGO_SOCIEDAD_SAP` a `SHM_SEDE` y carga la sociedad de las sedes | Lista las sedes con la columna nueva |
| 2 | `db/parches/patch_20261111_liberacion_pagos_sap.sql` | Crea la tabla `SHM_ORDEN_PAGO_LIBERACION` y su secuencia | Lista las columnas de la tabla y la secuencia |

El parche 2 solo crea objetos nuevos. El parche 1 solo escribe la columna nueva.

---

## 5. Código de sociedad por sede

Cada sede debe tener su código de sociedad SAP. **Sin él, las órdenes de esa sede quedan con error al aprobarse.**

El parche de sedes (paso 3 del script) ya carga los códigos de las sedes 01, 02, 03, 08, 09, 10, 11, 12, 13, 14, 15, 26 y 30. Las sedes que no figuren ahí se cargan desde **Sedes → Editar → Sociedad SAP**, o con SQL:

```sql
UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '<codigo SAP>' WHERE CODIGO = '<codigo sede>';
COMMIT;
```

Sedes activas que aún no tienen sociedad (deben cargarse):

```sql
SELECT ID_SEDE, CODIGO, NOMBRE FROM SHM_SEDE WHERE ACTIVO = 1 AND CODIGO_SOCIEDAD_SAP IS NULL;
```

---

## 6. Rollback

Restaurar las carpetas respaldadas y reciclar los Application Pools. El código anterior funciona con los objetos nuevos presentes, por lo que **no es necesario** revertir la base de datos.

Solo si se requiere, después de revertir el código:

```sql
DROP TABLE SHM_ORDEN_PAGO_LIBERACION;
DROP SEQUENCE SHM_ORDEN_PAGO_LIBERACION_SEQ;
ALTER TABLE SHM_SEDE DROP COLUMN CODIGO_SOCIEDAD_SAP;
```

Lo ya liberado en SAP **no se revierte** desde SHM.
