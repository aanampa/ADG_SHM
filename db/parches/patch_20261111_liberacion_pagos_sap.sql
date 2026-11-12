-- =============================================================================
-- PARCHE: Tabla SHM_ORDEN_PAGO_LIBERACION (liberacion de pagos en SAP)
-- =============================================================================
-- Descripcion : Guarda el resultado de la liberacion en SAP de cada comprobante
--               de una orden de pago (servicio LIBERA_PAGO_CABSet). Una fila por
--               orden de pago y comprobante. Los reintentos actualizan la fila.
--               Ver docs/requerimientos/02_liberacion_pagos/
-- Autor       : ADG Antonio
-- Fecha       : 2026-11-11
-- Ambiente    : Todos
-- Base        : XEPDB1 / usuario shm_dev
-- Requiere    : patch_20261111_sede_codigo_sociedad_sap.sql (sociedad por sede)
--               patch_20261105_campos_liquidacion_sap.sql  (CODSAP_CPM)
-- Impacto     : Solo crea objetos nuevos. No modifica tablas existentes.
-- =============================================================================
-- INSTRUCCIONES DE APLICACION
--   1. Conectar como shm_dev al ambiente correspondiente
--   2. Ejecutar el script completo
--   3. Verificar el resultado de la consulta del PASO 4
-- =============================================================================


-- -----------------------------------------------------------------------------
-- PASO 1 - Secuencia
-- -----------------------------------------------------------------------------

CREATE SEQUENCE SHM_ORDEN_PAGO_LIBERACION_SEQ START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE
/

-- -----------------------------------------------------------------------------
-- PASO 2 - Tabla
-- -----------------------------------------------------------------------------

CREATE TABLE SHM_ORDEN_PAGO_LIBERACION(
  ID_ORDEN_PAGO_LIBERACION Number NOT NULL,
  ID_ORDEN_PAGO Number NOT NULL,
  ID_PRODUCCION Number NOT NULL,
  SOCIEDAD Varchar2(4),
  DOCUMENTO Varchar2(10),
  EJERCICIO Varchar2(4),
  DOCUMENTO_DT Varchar2(10),
  ESTADO_LIBERACION Varchar2(20) NOT NULL,
  INTENTOS Number(3) DEFAULT 0 NOT NULL,
  FECHA_LIBERACION Date,
  MENSAJE_SAP Varchar2(2000),
  RESPUESTA_SAP Varchar2(4000),
  GUID_REGISTRO Varchar2(100) NOT NULL,
  ACTIVO Number(1) NOT NULL,
  ID_CREADOR Number NOT NULL,
  FECHA_CREACION Date NOT NULL,
  ID_MODIFICADOR Number,
  FECHA_MODIFICACION Date
)
/

ALTER TABLE SHM_ORDEN_PAGO_LIBERACION ADD CONSTRAINT PK_SHM_ORDEN_PAGO_LIBERACION PRIMARY KEY (ID_ORDEN_PAGO_LIBERACION)
/

ALTER TABLE SHM_ORDEN_PAGO_LIBERACION ADD CONSTRAINT UK_SHM_OP_LIBERACION UNIQUE (ID_ORDEN_PAGO, ID_PRODUCCION)
/

ALTER TABLE SHM_ORDEN_PAGO_LIBERACION ADD CONSTRAINT FK_SHM_OP_LIBERACION_OP FOREIGN KEY (ID_ORDEN_PAGO) REFERENCES SHM_ORDEN_PAGO (ID_ORDEN_PAGO)
/

ALTER TABLE SHM_ORDEN_PAGO_LIBERACION ADD CONSTRAINT FK_SHM_OP_LIBERACION_PROD FOREIGN KEY (ID_PRODUCCION) REFERENCES SHM_PRODUCCION (ID_PRODUCCION)
/

ALTER TABLE SHM_ORDEN_PAGO_LIBERACION ADD CONSTRAINT CK_SHM_OP_LIBERACION_ESTADO CHECK (ESTADO_LIBERACION IN ('PENDIENTE', 'LIBERADO', 'ERROR'))
/

-- -----------------------------------------------------------------------------
-- PASO 3 - Comentarios
-- -----------------------------------------------------------------------------

COMMENT ON TABLE SHM_ORDEN_PAGO_LIBERACION IS 'Resultado de la liberacion en SAP de cada comprobante de una orden de pago'
/
COMMENT ON COLUMN SHM_ORDEN_PAGO_LIBERACION.ESTADO_LIBERACION IS 'PENDIENTE, LIBERADO o ERROR'
/
COMMENT ON COLUMN SHM_ORDEN_PAGO_LIBERACION.INTENTOS IS 'Cantidad de veces que se envio a SAP'
/
COMMENT ON COLUMN SHM_ORDEN_PAGO_LIBERACION.MENSAJE_SAP IS 'Mensaje de SAP o detalle del error de validacion'
/
COMMENT ON COLUMN SHM_ORDEN_PAGO_LIBERACION.RESPUESTA_SAP IS 'Respuesta de SAP (truncada a 4000 caracteres)'
/

-- -----------------------------------------------------------------------------
-- PASO 4 - Verificacion
-- -----------------------------------------------------------------------------

SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE
  FROM USER_TAB_COLUMNS
 WHERE TABLE_NAME = 'SHM_ORDEN_PAGO_LIBERACION'
 ORDER BY COLUMN_ID
/

SELECT SEQUENCE_NAME FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'SHM_ORDEN_PAGO_LIBERACION_SEQ'
/
