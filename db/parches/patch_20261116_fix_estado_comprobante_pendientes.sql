-- =============================================================================
-- PARCHE: Corregir producciones solicitadas que no aparecen en "Facturas Pendientes"
-- =============================================================================
-- Descripcion : La lista "Facturas Pendientes" del Portal de Companias solo muestra
--               las producciones con ESTADO_COMPROBANTE = 'POR_ENVIAR'. Al anular un
--               comprobante (estadoProduccion = "9") o al fallar el envio a San Pablo,
--               el sistema dejaba ESTADO_COMPROBANTE en NULL, y al volver a solicitar
--               la factura no se restauraba. Resultado: el panel de inicio cuenta la
--               factura como pendiente pero la lista no la muestra.
--               Este parche corrige los registros que ya quedaron asi. El codigo nuevo
--               evita que vuelva a ocurrir.
-- Autor       : ADG Antonio
-- Fecha       : 2026-11-16
-- Ambiente    : Todos
-- Base        : XEPDB1 / usuario shm_dev
-- Impacto     : Solo actualiza ESTADO_COMPROBANTE de las filas afectadas.
-- =============================================================================
-- INSTRUCCIONES DE APLICACION
--   1. Conectar como shm_dev al ambiente correspondiente
--   2. Ejecutar el PASO 1 y revisar las filas: son las que se van a corregir
--   3. Ejecutar el PASO 2 (UPDATE y COMMIT)
--   4. Verificar con el PASO 3 (no debe devolver filas)
-- =============================================================================


-- -----------------------------------------------------------------------------
-- PASO 1 - Producciones afectadas (solo consulta)
-- Solicitadas o devueltas, activas, con el comprobante en NULL.
-- -----------------------------------------------------------------------------

SELECT p.ID_PRODUCCION,
       em.RUC,
       em.RAZON_SOCIAL,
       p.CODIGO_PRODUCCION,
       p.NUMERO_PRODUCCION,
       p.ESTADO,
       p.ESTADO_COMPROBANTE,
       p.FECHA_LIMITE
  FROM SHM_PRODUCCION p
  JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
 WHERE p.ACTIVO = 1
   AND p.ESTADO IN ('FACTURA_SOLICITADA', 'FACTURA_DEVUELTA')
   AND p.ESTADO_COMPROBANTE IS NULL
 ORDER BY em.RAZON_SOCIAL, p.CODIGO_PRODUCCION
/

-- -----------------------------------------------------------------------------
-- PASO 2 - Correccion
-- -----------------------------------------------------------------------------

UPDATE SHM_PRODUCCION
   SET ESTADO_COMPROBANTE = 'POR_ENVIAR',
       FECHA_MODIFICACION = SYSDATE
 WHERE ACTIVO = 1
   AND ESTADO IN ('FACTURA_SOLICITADA', 'FACTURA_DEVUELTA')
   AND ESTADO_COMPROBANTE IS NULL
/

COMMIT
/

-- -----------------------------------------------------------------------------
-- PASO 3 - Verificacion (no debe devolver filas)
-- -----------------------------------------------------------------------------

SELECT p.ID_PRODUCCION, p.NUMERO_PRODUCCION, p.ESTADO, p.ESTADO_COMPROBANTE
  FROM SHM_PRODUCCION p
 WHERE p.ACTIVO = 1
   AND p.ESTADO IN ('FACTURA_SOLICITADA', 'FACTURA_DEVUELTA')
   AND p.ESTADO_COMPROBANTE IS NULL
/
