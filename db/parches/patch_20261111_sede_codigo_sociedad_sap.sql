-- =============================================================================
-- PARCHE: Agregar CODIGO_SOCIEDAD_SAP a SHM_SEDE
-- =============================================================================
-- Descripcion : Agrega el codigo de sociedad SAP de cada sede. Se usa en la
--               liberacion de pagos (campo "Sociedad" del servicio
--               LIBERA_PAGO_CABSet). Ver docs/requerimientos/02_liberacion_pagos/
-- Autor       : ADG Antonio
-- Fecha       : 2026-11-11
-- Ambiente    : Todos
-- Base        : XEPDB1 / usuario shm_dev
-- Impacto     : Columna NULL sin DEFAULT. En Oracle 11g solo modifica el
--               diccionario de datos, no reescribe la tabla.
-- =============================================================================
-- INSTRUCCIONES DE APLICACION
--   1. Conectar como shm_dev al ambiente correspondiente
--   2. Ejecutar el PASO 1 y el PASO 2
--   3. El PASO 3 carga la sociedad de las sedes 01, 02, 03, 08, 09, 10, 11, 12, 13,
--      14, 15, 26 y 30. Las demas sedes se cargan desde la pantalla Sedes
--      (campo "Sociedad SAP")
--   4. Verificar con el PASO 4
-- =============================================================================


-- -----------------------------------------------------------------------------
-- PASO 1 - Agregar la columna
-- -----------------------------------------------------------------------------

ALTER TABLE SHM_SEDE ADD CODIGO_SOCIEDAD_SAP VARCHAR2(4)
/

-- -----------------------------------------------------------------------------
-- PASO 2 - Comentario
-- -----------------------------------------------------------------------------

COMMENT ON COLUMN SHM_SEDE.CODIGO_SOCIEDAD_SAP IS 'Codigo de sociedad SAP de la sede (ej. 1001). Se envia en la liberacion de pagos'
/

-- -----------------------------------------------------------------------------
-- PASO 3 - Carga de la sociedad SAP por sede (datos entregados por SAP)
-- -----------------------------------------------------------------------------

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0101' WHERE CODIGO = '01'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0701' WHERE CODIGO = '02'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '3501' WHERE CODIGO = '03'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0101' WHERE CODIGO = '08'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0103' WHERE CODIGO = '09'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0113' WHERE CODIGO = '10'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0102' WHERE CODIGO = '11'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0201' WHERE CODIGO = '12'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0601' WHERE CODIGO = '13'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '0901' WHERE CODIGO = '14'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '1001' WHERE CODIGO = '15'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '1301' WHERE CODIGO = '26'
/

UPDATE SHM_SEDE SET CODIGO_SOCIEDAD_SAP = '2101' WHERE CODIGO = '30'
/

COMMIT
/

-- -----------------------------------------------------------------------------
-- PASO 4 - Verificacion
-- -----------------------------------------------------------------------------

SELECT ID_SEDE, CODIGO, NOMBRE, CODIGO_SOCIEDAD_SAP
  FROM SHM_SEDE
 WHERE ACTIVO = 1
 ORDER BY ID_SEDE
/

-- Sedes activas sin sociedad SAP (sus ordenes de pago quedarian con error al liberar)
SELECT ID_SEDE, CODIGO, NOMBRE FROM SHM_SEDE WHERE ACTIVO = 1 AND CODIGO_SOCIEDAD_SAP IS NULL ORDER BY ID_SEDE
/
