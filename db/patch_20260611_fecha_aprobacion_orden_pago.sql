-- =============================================================================
-- PARCHE: Agregar FECHA_APROBACION a SHM_ORDEN_PAGO
-- =============================================================================
-- Descripcion : Agrega el campo FECHA_APROBACION DATE a la tabla SHM_ORDEN_PAGO.
--               El campo se graba cuando la orden alcanza el estado APROBADO
--               y se limpia (NULL) si la orden es devuelta.
--               Se realiza backfill para registros historicos ya aprobados o pagados.
-- Autor       : ADG Antonio
-- Fecha       : 2026-06-11
-- Ambiente    : Produccion
-- Base        : XEPDB1 / usuario shm_dev
-- =============================================================================
-- INSTRUCCIONES DE APLICACION
--   1. Conectar como shm_dev al ambiente de produccion
--   2. Ejecutar el script completo
--   3. Verificar la salida de los mensajes DBMS_OUTPUT
--   4. Confirmar con COMMIT (incluido al final del script)
-- =============================================================================


-- -----------------------------------------------------------------------------
-- PASO 1 - Agregar la columna
-- -----------------------------------------------------------------------------

ALTER TABLE SHM_ORDEN_PAGO ADD FECHA_APROBACION DATE
/

-- -----------------------------------------------------------------------------
-- PASO 2 - Backfill: poblar registros ya APROBADOS o PAGADOS
--          Toma la fecha de la ultima aprobacion registrada en
--          SHM_ORDEN_PAGO_APROBACION (MAX FECHA_APROBACION con ESTADO=APROBADO)
-- -----------------------------------------------------------------------------

UPDATE SHM_ORDEN_PAGO op
SET op.FECHA_APROBACION = (
    SELECT MAX(opa.FECHA_APROBACION)
    FROM SHM_ORDEN_PAGO_APROBACION opa
    WHERE opa.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
      AND opa.ESTADO        = 'APROBADO'
      AND opa.ACTIVO        = 1
)
WHERE op.ESTADO IN ('APROBADO', 'PAGADO')
  AND op.ACTIVO = 1
/

-- -----------------------------------------------------------------------------
-- PASO 3 - Confirmar cambios
-- -----------------------------------------------------------------------------

COMMIT
/

-- -----------------------------------------------------------------------------
-- PASO 4 - Verificacion
-- -----------------------------------------------------------------------------

-- Confirmar que la columna existe y tiene el tipo correcto
SELECT COLUMN_NAME,
       DATA_TYPE,
       NULLABLE
FROM   USER_TAB_COLUMNS
WHERE  TABLE_NAME   = 'SHM_ORDEN_PAGO'
  AND  COLUMN_NAME  = 'FECHA_APROBACION'
/

-- Resumen de registros actualizados por estado
SELECT ESTADO,
       COUNT(*)                                         AS TOTAL,
       COUNT(FECHA_APROBACION)                          AS CON_FECHA,
       SUM(CASE WHEN FECHA_APROBACION IS NULL THEN 1
                ELSE 0 END)                             AS SIN_FECHA
FROM   SHM_ORDEN_PAGO
WHERE  ACTIVO = 1
GROUP BY ESTADO
ORDER BY ESTADO
/
