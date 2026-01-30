-- =============================================
-- Script: 07_alter_table_archivo_blob.sql
-- Descripcion: Agrega soporte para almacenamiento BLOB en la tabla SHM_ARCHIVO
-- Autor: ADG Vladimir
-- Fecha: 2026-01-29
-- =============================================

-- 1. Agregar columnas para almacenamiento BLOB
ALTER TABLE SHM_ARCHIVO ADD (
    CONTENIDO_ARCHIVO BLOB,           -- Contenido binario del archivo (solo cuando TIPO_ALMACENAMIENTO = 'BLOB')
    TIPO_ALMACENAMIENTO VARCHAR2(20)  -- 'FILE' o 'BLOB' (default: 'FILE')
);

-- 2. Agregar comentarios descriptivos
COMMENT ON COLUMN SHM_ARCHIVO.CONTENIDO_ARCHIVO IS 'Contenido binario del archivo cuando se almacena en BD';
COMMENT ON COLUMN SHM_ARCHIVO.TIPO_ALMACENAMIENTO IS 'Tipo de almacenamiento: FILE (sistema de archivos) o BLOB (base de datos)';

-- 3. Actualizar registros existentes (todos son FILE actualmente)
UPDATE SHM_ARCHIVO SET TIPO_ALMACENAMIENTO = 'FILE' WHERE TIPO_ALMACENAMIENTO IS NULL;
COMMIT;

-- 4. Insertar parametro de configuracion para definir el tipo de almacenamiento por defecto
-- Valores posibles:
--   'FILE' = Almacenar en sistema de archivos (comportamiento actual)
--   'BLOB' = Almacenar en base de datos como BLOB
INSERT INTO SHM_PARAMETRO (ID_PARAMETRO, CODIGO, VALOR, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION)
VALUES (SHM_PARAMETRO_SEQ.NEXTVAL, 'SHM_TIPO_ALMACENAMIENTO_ARCHIVO', 'FILE', SYS_GUID(), 1, 1, SYSDATE);
COMMIT;

-- 5. Verificar cambios
SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH
FROM USER_TAB_COLUMNS
WHERE TABLE_NAME = 'SHM_ARCHIVO'
  AND COLUMN_NAME IN ('CONTENIDO_ARCHIVO', 'TIPO_ALMACENAMIENTO');

SELECT CODIGO, VALOR FROM SHM_PARAMETRO WHERE CODIGO = 'SHM_TIPO_ALMACENAMIENTO_ARCHIVO';
