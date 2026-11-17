# Req 04 - Consulta y seguimiento de facturas pendientes de envío: Propuesta de solución

| Campo | Valor |
|-------|-------|
| **Requerimiento** | `00_contexto.md` |
| **Autor** | ADG Antonio |
| **Fecha** | 2026-11-15 (versión 3) |
| **Estado** | Propuesta, pendiente de validar los puntos de la sección 9 |
| **Decisiones recibidas** | El sistema de Honorarios usa el borrado lógico (`DELETE`). Las eliminadas se muestran siempre, hasta que el área de Honorarios Médicos las archive mediante un flag (`FLAG_ARCHIVADO`). El alcance es solo lo pendiente de envío por parte de la compañía |

---

## 1. Requerimiento

Cuando el sistema de Honorarios elimina la producción asociada, las facturas pendientes de envío desaparecen del portal de la compañía médica. Se requiere **mantener su trazabilidad y permitir su consulta**, aunque la producción original haya sido eliminada.

## 2. Cómo funciona hoy (verificado en el código)

**El sistema de Honorarios elimina con `DELETE /api/Producciones/{id}`**, que hace un **borrado lógico**: `ProduccionRepository.DeleteAsync` solo pone `ACTIVO = 0`, y actualiza el usuario modificador (siempre `1`) y `FECHA_MODIFICACION`. **`ESTADO` y `ESTADO_COMPROBANTE` quedan como estaban.**

Después, la producción **no la ve nadie**, ni la compañía ni el Portal Administrativo, porque todas las consultas filtran `ACTIVO = 1`.

**Qué veía la compañía antes de la eliminación:** *Facturas → Pendientes* (`FacturasController.GetListPendientes`) muestra las producciones con `ESTADO_COMPROBANTE = 'POR_ENVIAR'` y estado `FACTURA_SOLICITADA` o `FACTURA_DEVUELTA`.

**Dos hallazgos que simplifican la solución**
1. `DeleteAsync` es el único lugar del sistema que pone `ACTIVO = 0` en una producción. Por lo tanto, **toda producción con `ACTIVO = 0` fue eliminada por el sistema de Honorarios**, y las que ya se eliminaron siguen en la tabla con todos sus datos.
2. Como el estado no cambia, **el estado anterior a la eliminación es el estado actual** de la fila. No hace falta guardarlo aparte.

**Lo que falta:** la eliminación no escribe en la bitácora y no deja una fecha propia. Hoy la fecha solo se deduce de `FECHA_MODIFICACION`, que en una fila inactiva es la del borrado.

## 3. Brecha

La compañía recibió la solicitud de factura por correo, la producción aparecía en su lista, y de pronto ya no. No puede saber si se eliminó, cuándo, ni qué era. Soporte tampoco, salvo consultando la base de datos. Y hoy no hay cómo cerrar ese seguimiento una vez resuelto.

## 4. Propuesta

Tres piezas: **(A)** la compañía ve sus eliminadas, **(B)** el área de Honorarios Médicos las archiva cuando ya están atendidas, y **(C)** la eliminación deja registro.

### 4.1 (A) Vista de la compañía: pestaña "Eliminadas"

Una producción aparece si cumple todo esto:

```sql
ACTIVO = 0
AND ESTADO_COMPROBANTE = 'POR_ENVIAR'
AND ESTADO IN ('FACTURA_SOLICITADA', 'FACTURA_DEVUELTA')
AND NVL(FLAG_ARCHIVADO, 0) = 0
AND ID_ENTIDAD_MEDICA = <entidad del usuario de la sesión>
```

Es el conjunto que la compañía veía en Pendientes antes de la eliminación. Las que estaban en `FACTURA_PENDIENTE` no aparecen, porque nunca las vio. **Las producciones eliminadas antes de este cambio también aparecen**, sin migrar nada.

En **Facturas → Pendientes** se agrega la pestaña **Eliminadas**, de solo lectura:

| Columna | Origen |
|---------|--------|
| Producción | `NUMERO_PRODUCCION` o `CODIGO_PRODUCCION` |
| Sede, concepto, monto | Los mismos datos que la lista de pendientes |
| Fecha límite que tenía | `FECHA_LIMITE` |
| Estado en que estaba | Solicitada o Devuelta (el `ESTADO` de la fila) |
| Fecha de eliminación | `FECHA_MODIFICACION`, o la de la bitácora si existe (ver 4.3) |

- **Sin acciones:** no se puede subir factura ni descargar el XML.
- Un **contador** en la pestaña, y búsqueda dentro de la lista.
- Se muestran **siempre**, hasta que Honorarios Médicos las archive.
- Al abrir una fila, un detalle con los eventos que existan en la bitácora de esa producción.

### 4.2 (B) Archivado por Honorarios Médicos

**Flag de archivo, no un estado nuevo.** Si se cambiara el `ESTADO`, se perdería el dato de en qué estado estaba la producción al eliminarse, que es justo lo que se necesita mostrar. Por eso se agrega una marca aparte.

**Base de datos:** tres columnas nuevas, opcionales y sin `DEFAULT` (en Oracle 11g solo modifican el diccionario de datos):

| Columna | Descripción |
|---------|-------------|
| `FLAG_ARCHIVADO NUMBER(1)` | `1` = archivada. `NULL` o `0` = vigente |
| `FECHA_ARCHIVADO DATE` | Cuándo se archivó |
| `ID_USUARIO_ARCHIVADO NUMBER` | Quién la archivó |

**Pantalla nueva en el Portal Administrativo** (para el área de Honorarios Médicos): **Producciones eliminadas**, con su opción de menú y asignación a roles como las demás.

| Elemento | Detalle |
|----------|---------|
| Lista | Compañía, sede, producción, concepto, monto, estado en que estaba, fecha de eliminación, fecha y usuario de archivado |
| Filtros | Compañía, sede, estado, y **Archivadas: No / Sí / Todas** (por defecto "No"), búsqueda |
| Acción | **Archivar** una producción o varias seleccionadas, con una confirmación simple y **sin comentario**. **No se puede desarchivar** (ver 9.8): la confirmación avisa que el archivado es definitivo |
| Alcance | Las mismas producciones de la vista de la compañía (4.1), más las ya archivadas según el filtro |
| Seguridad | **Archiva el Jefe de Sede.** "Jefe de Sede" es el perfil de aprobación `JEFE_SEDE` (el mismo del flujo de aprobación de órdenes de pago, asignado en *Usuarios → Internos → Perfil de Aprobación*). Un usuario puede ver y archivar una producción solo si tiene ese perfil y su asignación es para la sede de la producción, o no tiene sede asignada (aplica a todas). El acceso a la pantalla también se controla por la opción de menú y el rol. Se identifican las filas por GUID |
| Registro | Se guardan solo **quién** archivó y **cuándo** (columnas `ID_USUARIO_ARCHIVADO` y `FECHA_ARCHIVADO`). Además, cada archivado escribe en la bitácora (`PRODUCCION_ARCHIVADA`) con el usuario y la fecha, sin comentario |

Al archivar, la producción sale de la pestaña "Eliminadas" de la compañía. **No se borra nada:** sigue en la base de datos y se puede consultar desde la pantalla interna.

### 4.3 (C) Registro de la eliminación

`ProduccionService.DeleteProduccionAsync` escribe una entrada en la bitácora (`SHM_BITACORA`, entidad `SHM_PRODUCCION`, acción `PRODUCCION_ELIMINADA`) con número de producción, estado en que estaba, monto y fecha límite. Así la fecha queda exacta y el detalle tiene un evento propio. Para las eliminadas antes del cambio se usa `FECHA_MODIFICACION`.

### 4.4 Lo que no cambia

- La eliminación sigue siendo un borrado lógico (`ACTIVO = 0`), y la producción sigue oculta para el resto del sistema.
- El sistema de Honorarios sigue usando el mismo `DELETE`.

## 5. Alternativa descartada

Cambiar `DELETE` para que marque la producción como anulada (`ESTADO = 'PRODUCCION_ANULADA'`) y la deje con `ACTIVO = 1`. La producción volvería a aparecer en todas las listas y flujos que hoy filtran `ACTIVO = 1` (listados del Portal Administrativo, solicitud masiva, liquidaciones, tableros). Para este requerimiento no hace falta.

## 6. Opcionales (fase 3)

| Opción | Detalle |
|--------|---------|
| **Aviso por correo a la compañía** al eliminarse una producción con solicitud de factura | **Descartado por ahora** (decisión 9.3). Si se retoma: en `DeleteProduccionAsync`, antes del borrado, si estaba solicitada o devuelta, reutilizando `BuildRecipientsAsync` (usuarios en TO y contactos en CC) y `EmailService` |
| **Vista "Archivadas" para la compañía** | **Descartada por ahora** (decisión 9.5): al archivar, la compañía deja de ver la producción. Si se retoma: un interruptor de solo lectura en la pestaña "Eliminadas". El dato no se pierde: sigue en la base de datos y en la pantalla interna |

## 7. Impacto

| Área | Cambio |
|------|--------|
| Base de datos | **Un parche** con 3 columnas nuevas en `SHM_PRODUCCION` |
| `ProduccionRepository` / `ProduccionService` | Consulta de eliminadas por entidad y para el área interna. Archivar. `DeleteProduccionAsync` escribe la bitácora |
| Portal de Compañías | Pestaña "Eliminadas", detalle y contador |
| Portal Administrativo | Pantalla "Producciones eliminadas" con su opción de menú y permisos por rol |
| API | Publicar, para que la eliminación registre la bitácora |
| Compatibilidad | El código anterior sigue funcionando con las columnas nuevas presentes. Las consultas existentes no cambian |
| Publicación | **API, Portal de Compañías y Portal Administrativo**, previo parche de base de datos |

## 8. Plan de trabajo y pruebas

| Fase | Contenido |
|------|-----------|
| 1 | Parche de base de datos. Consulta de eliminadas y pestaña en el Portal de Compañías |
| 2 | Bitácora al eliminar en el API. Pantalla "Producciones eliminadas" en el Portal Administrativo con archivado (individual y masivo) |
| 3 | (Opcional, hoy sin definir) Evolutivos que se decidan más adelante: aviso por correo y vista "Archivadas" para la compañía |
| 4 | Guía de despliegue |

**Pruebas**
- Una compañía con producciones solicitadas y devueltas eliminadas las ve en la pestaña.
- **Aislamiento:** otra compañía no ve las eliminadas de la primera.
- Una producción en `FACTURA_PENDIENTE` eliminada **no** aparece.
- Una producción cuya factura **ya se envió** (`ESTADO_COMPROBANTE = 'ENVIADO'`) y luego se elimina **no** aparece: el alcance es solo lo pendiente de envío.
- Una eliminada **antes** del cambio aparece con la fecha de `FECHA_MODIFICACION`.
- Eliminar una producción nueva por el API deja `PRODUCCION_ELIMINADA` en la bitácora, y su fecha en la pestaña coincide.
- La pestaña no permite subir factura.
- **Archivar:** al archivar desde el Portal Administrativo, la producción sale de la pestaña de la compañía. Queda con fecha y usuario, y la entrada en la bitácora.
- **Archivado definitivo:** no existe la acción de desarchivar. Una producción archivada solo se consulta desde la pantalla interna con el filtro "Archivadas: Sí".
- Archivado masivo de varias producciones.
- **Permisos:** un usuario sin el perfil `JEFE_SEDE` no puede archivar. Un Jefe de Sede con asignación a una sede solo ve y archiva las eliminadas de esa sede. Uno sin sede asignada ve las de todas. Quien no tenga la opción de menú no accede a la pantalla.

## 9. Preguntas abiertas

| # | Pregunta | Por qué importa |
|---|----------|-----------------|
| 9.1 | ~~¿Anulación por API o `DELETE`?~~ | **Resuelto:** el sistema de Honorarios usa `DELETE` (borrado lógico) |
| 9.2 | ~~¿Hasta cuándo se muestran las eliminadas?~~ | **Resuelto:** siempre, hasta que el área de Honorarios Médicos las archive mediante un flag de archivo |
| 9.3 | ~~¿Se avisa por correo a la compañía al eliminarse una producción?~~ | **Resuelto:** por ahora no se avisa. La compañía se entera consultando la pestaña "Eliminadas". Queda como opcional para una fase posterior |
| 9.4 | ~~¿Se incluyen también las facturas ya enviadas que luego se eliminan?~~ | **Resuelto:** solo las pendientes de envío por parte de la compañía médica. Es lo que ya cubre la consulta de 4.1 (`ESTADO_COMPROBANTE = 'POR_ENVIAR'`) |
| 9.5 | ~~Una vez archivada, ¿la compañía la sigue viendo?~~ | **Resuelto:** por ahora no. Al archivar, sale de la pestaña "Eliminadas" de la compañía. Queda consultable solo desde la pantalla interna de Honorarios Médicos |
| 9.6 | ~~¿Quiénes pueden archivar?~~ | **Resuelto:** el Jefe de Sede, solo para las producciones de su sede. Se asume que "Jefe de Sede" es el perfil de aprobación `JEFE_SEDE`. Falta confirmar si el administrador del sistema también puede archivar (ver 9.12) |
| 9.7 | ~~¿El archivado requiere un comentario?~~ | **Resuelto:** no hay comentario. Solo se guarda quién archivó y cuándo |
| 9.8 | ~~¿Debe poder desarchivarse una producción?~~ | **Resuelto:** no. El archivado es definitivo. Como no hay vuelta atrás, la pantalla pide confirmación y muestra cuántas producciones se van a archivar |
| 9.9 | ~~¿Debe poder reactivarse o recrearse una producción eliminada por error?~~ | **Resuelto:** no. Se mantiene el comportamiento actual: una producción eliminada no se puede recrear por el API, porque `ExistsByKeyAsync` no filtra por `ACTIVO` y la omite como "ya existe". Queda registrado como limitación conocida |
| 9.10 | Observación: `DELETE` no valida el estado de la producción. Puede eliminar una ya enviada, liquidada, en una orden de pago o pagada. ¿Debe restringirse? | Fuera de este requerimiento, pero una eliminación en un estado avanzado afectaría a órdenes de pago y pagos |
| 9.11 | La eliminación se audita siempre con el usuario `1`. ¿Se necesita saber quién la hizo? | El API no tiene autenticación, por lo que hoy no se puede saber |
| 9.12 | ¿El administrador del sistema (y el área de Honorarios Médicos en general) puede ver y archivar las eliminadas de todas las sedes, o solo el Jefe de Sede? | Define el permiso de la pantalla interna |

## 10. Recomendación

Hacer las fases 1 y 2: pestaña "Eliminadas" para la compañía, archivado por Honorarios Médicos y bitácora al eliminar. Las producciones ya eliminadas aparecen de inmediato. Lo que falta definir para construir la pantalla interna es solo 9.12 (si el administrador también archiva). La pregunta 9.10 (el `DELETE` no valida el estado) conviene hablarla con el sistema de Honorarios aunque quede fuera de este alcance.

## 11. Prototipo

Dos pantallas navegables con datos de ejemplo, en la carpeta [prototipo/](prototipo/). Reutilizan los estilos reales de cada portal (se referencian desde `src/`), así que hay que abrirlos **desde la carpeta del repositorio** con un navegador; no necesitan servidor.

| Archivo | Pantalla | Qué permite probar |
|---------|----------|--------------------|
| `compania_facturas_eliminadas.html` | **Portal de Compañías**: opción de menú nueva "Facturas Eliminadas" | Lista, búsqueda, aviso informativo, detalle con seguimiento y estado vacío (botón "Sin datos" abajo a la derecha) |
| `honorarios_seguimiento_facturas.html` | **Portal Administrativo**: opción nueva "Seguimiento Facturas" | Un solo seguimiento con las facturas pendientes **vigentes** (solicitadas y sin enviar) y las **eliminadas**: resumen, filtros por situación y estado, fecha límite con vencidas resaltadas, **notificaciones enviadas a la compañía** (detalle con destinatarios y estado), archivado individual y masivo de las eliminadas con confirmación, y simulación de tres usuarios (Jefe de Sede de una sede, de todas, y sin perfil) |

**Qué conviene revisar con el negocio**
- Los textos: el aviso de la compañía, el nombre de las opciones de menú y los títulos de columna ("Estaba en", "Eliminada el").
- Las columnas de cada lista: si falta algo o sobra algo.
- La confirmación del archivado y su advertencia de que es definitivo.
- En el seguimiento, el prototipo muestra la solicitud de factura (fecha de `FACTURA_FECHA_SOLICITUD`) y la eliminación (`FECHA_MODIFICACION`).

**Lo que el prototipo no hace:** no consulta datos reales, no guarda nada, y los controles "PROTOTIPO" de la esquina no forman parte de la pantalla final.

**Cambio de alcance que muestra el prototipo del Portal Administrativo.** Ya no es una pantalla solo de eliminadas: es un **seguimiento de facturas pendientes de envío** con dos situaciones, **Vigente** (solicitada o devuelta, la compañía aún no la envió y sigue activa en su portal) y **Eliminada**. Esto responde a "dónde veo las facturas solicitadas a las compañías que aún no enviaron su factura".

- Las **vigentes** salen de la misma consulta que alimenta *Facturas Pendientes* de la compañía (`ACTIVO = 1`, `POR_ENVIAR`, solicitada o devuelta). Las **eliminadas**, de la consulta de 4.1.
- Las **notificaciones** salen de `SHM_EMAIL_LOG` (`ENTIDAD_REFERENCIA = 'SHM_PRODUCCION'`, `ID_REFERENCIA` = producción), igual que el historial de notificaciones que ya existe en la pantalla Producción. No hacen falta cambios de base de datos para esto.
- Solo las **eliminadas** se pueden archivar. Las vigentes no tienen acción en esta pantalla.
- Se renombró la opción de menú a "Seguimiento Facturas" y el título a "Seguimiento de Facturas Pendientes".

**Pregunta nueva**

| # | Pregunta | Por qué importa |
|---|----------|-----------------|
| 9.13 | ¿El Jefe de Sede necesita este seguimiento unificado (vigentes y eliminadas) o basta con ver solo las eliminadas y seguir usando la pantalla *Producción* (filtrando por "Factura Solicitada") para las vigentes? | La pantalla Producción ya permite filtrar por estado y ver el historial de notificaciones. El seguimiento unificado agrega el resumen, las vencidas resaltadas y las eliminadas en el mismo lugar |
