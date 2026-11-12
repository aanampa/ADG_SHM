-- =============================================================================
-- PARCHE: Agregar CODSAP_CPM y CODSAP_DETRAC a SHM_PRODUCCION
-- =============================================================================
-- Descripcion : Agrega los codigos SAP que llegan con la liquidacion:
--                 CODSAP_CPM    : documento SAP que representa a la factura
--                 CODSAP_DETRAC : documento SAP que representa a la detraccion
--               El monto y el porcentaje de la detraccion NO se crean: se
--               reutilizan MTO_DETRACCION y PORC_DETRACCION, que se
--               sobrescriben con el valor de SAP al recibir la liquidacion.
--               Ver docs/requerimientos/01_campos_liquidacion/
-- Autor       : ADG Antonio
-- Fecha       : 2026-11-09
-- Ambiente    : Todos
-- Base        : XEPDB1 / usuario shm_dev
-- Impacto     : Columnas NULL sin DEFAULT. En Oracle 11g solo modifica el
--               diccionario de datos, no reescribe la tabla.
-- =============================================================================
-- INSTRUCCIONES DE APLICACION
--   1. Conectar como shm_dev al ambiente correspondiente
--   2. Ejecutar el script completo
--   3. Verificar el resultado de la consulta del PASO 3
--      (debe listar las 2 columnas nuevas y las 2 existentes de detraccion)
-- =============================================================================


-- -----------------------------------------------------------------------------
-- PASO 1 - Agregar las columnas
-- -----------------------------------------------------------------------------

ALTER TABLE SHM_PRODUCCION ADD CODSAP_CPM VARCHAR2(18)
/

ALTER TABLE SHM_PRODUCCION ADD CODSAP_DETRAC VARCHAR2(20)
/

-- -----------------------------------------------------------------------------
-- PASO 2 - Comentarios
-- -----------------------------------------------------------------------------

COMMENT ON COLUMN SHM_PRODUCCION.CODSAP_CPM IS 'Codigo SAP (numero de documento) que representa a la factura'
/

COMMENT ON COLUMN SHM_PRODUCCION.CODSAP_DETRAC IS 'Codigo SAP (numero de documento) que representa a la detraccion de la factura'
/

-- -----------------------------------------------------------------------------
-- PASO 3 - Verificacion
-- -----------------------------------------------------------------------------

SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE
  FROM USER_TAB_COLUMNS
 WHERE TABLE_NAME = 'SHM_PRODUCCION'
   AND (COLUMN_NAME LIKE 'CODSAP%' OR COLUMN_NAME LIKE '%DETRAC%')
 ORDER BY COLUMN_NAME
/
