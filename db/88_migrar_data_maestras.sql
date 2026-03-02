-- ============================================================================
-- SCRIPT DE MIGRACION DE DATOS DE TABLAS MAESTRAS
-- Tablas: SHM_TABLA, SHM_TABLA_DETALLE, SHM_BANCO, SHM_PARAMETRO
-- Fecha: 2026-02-25
-- Descripcion: Replica la data exacta (con IDs originales, sin secuencias)
-- ============================================================================

-- ============================================================================
-- PASO 1: EJECUTAR EN BD DESTINO (la antigua)
-- Eliminar datos existentes en orden inverso (hijos primero)
-- ============================================================================

DELETE FROM SHM_TABLA_DETALLE;
DELETE FROM SHM_TABLA;
DELETE FROM SHM_BANCO;
DELETE FROM SHM_PARAMETRO;
COMMIT;

-- ============================================================================
-- PASO 2: EJECUTAR EN BD ORIGEN (la actualizada)
-- Estas consultas generan los INSERT con IDs fijos.
-- Copiar el resultado y ejecutarlo en la BD destino.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 2.1 Generar INSERTs para SHM_TABLA
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_TABLA (ID_TABLA, CODIGO, DESCRIPCION, ACTIVO, GUID_REGISTRO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_TABLA || ', '
    || CASE WHEN CODIGO IS NULL THEN 'NULL' ELSE '''' || REPLACE(CODIGO, '''', '''''') || '''' END || ', '
    || CASE WHEN DESCRIPCION IS NULL THEN 'NULL' ELSE '''' || REPLACE(DESCRIPCION, '''', '''''') || '''' END || ', '
    || NVL(TO_CHAR(ACTIVO), 'NULL') || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || NVL(TO_CHAR(ID_CREADOR), 'NULL') || ', '
    || CASE WHEN FECHA_CREACION IS NULL THEN 'NULL' ELSE 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS'')' END || ');'
    AS SCRIPT_INSERT
FROM SHM_TABLA
ORDER BY ID_TABLA;

-- ---------------------------------------------------------------------------
-- 2.2 Generar INSERTs para SHM_TABLA_DETALLE
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_TABLA_DETALLE (ID_TABLA, ID_TABLA_DETALLE, CODIGO, DESCRIPCION, ORDEN, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_TABLA || ', '
    || ID_TABLA_DETALLE || ', '
    || CASE WHEN CODIGO IS NULL THEN 'NULL' ELSE '''' || REPLACE(CODIGO, '''', '''''') || '''' END || ', '
    || CASE WHEN DESCRIPCION IS NULL THEN 'NULL' ELSE '''' || REPLACE(DESCRIPCION, '''', '''''') || '''' END || ', '
    || NVL(TO_CHAR(ORDEN), 'NULL') || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || NVL(TO_CHAR(ACTIVO), 'NULL') || ', '
    || NVL(TO_CHAR(ID_CREADOR), 'NULL') || ', '
    || CASE WHEN FECHA_CREACION IS NULL THEN 'NULL' ELSE 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS'')' END || ');'
    AS SCRIPT_INSERT
FROM SHM_TABLA_DETALLE
ORDER BY ID_TABLA, ID_TABLA_DETALLE;

-- ---------------------------------------------------------------------------
-- 2.3 Generar INSERTs para SHM_BANCO
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_BANCO (ID_BANCO, CODIGO_BANCO, NOMBRE_BANCO, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_BANCO || ', '
    || CASE WHEN CODIGO_BANCO IS NULL THEN 'NULL' ELSE '''' || REPLACE(CODIGO_BANCO, '''', '''''') || '''' END || ', '
    || CASE WHEN NOMBRE_BANCO IS NULL THEN 'NULL' ELSE '''' || REPLACE(NOMBRE_BANCO, '''', '''''') || '''' END || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || NVL(TO_CHAR(ACTIVO), 'NULL') || ', '
    || NVL(TO_CHAR(ID_CREADOR), 'NULL') || ', '
    || CASE WHEN FECHA_CREACION IS NULL THEN 'NULL' ELSE 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS'')' END || ');'
    AS SCRIPT_INSERT
FROM SHM_BANCO
ORDER BY ID_BANCO;

-- ---------------------------------------------------------------------------
-- 2.4 Generar INSERTs para SHM_PARAMETRO
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_PARAMETRO (ID_PARAMETRO, CODIGO, VALOR, DESCRIPCION, TIPO_PARAMETRO, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_PARAMETRO || ', '
    || CASE WHEN CODIGO IS NULL THEN 'NULL' ELSE '''' || REPLACE(CODIGO, '''', '''''') || '''' END || ', '
    || CASE WHEN VALOR IS NULL THEN 'NULL' ELSE '''' || REPLACE(VALOR, '''', '''''') || '''' END || ', '
    || CASE WHEN DESCRIPCION IS NULL THEN 'NULL' ELSE '''' || REPLACE(DESCRIPCION, '''', '''''') || '''' END || ', '
    || CASE WHEN TIPO_PARAMETRO IS NULL THEN 'NULL' ELSE '''' || REPLACE(TIPO_PARAMETRO, '''', '''''') || '''' END || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || NVL(TO_CHAR(ACTIVO), 'NULL') || ', '
    || NVL(TO_CHAR(ID_CREADOR), 'NULL') || ', '
    || CASE WHEN FECHA_CREACION IS NULL THEN 'NULL' ELSE 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS'')' END || ');'
    AS SCRIPT_INSERT
FROM SHM_PARAMETRO
ORDER BY ID_PARAMETRO;

-- ============================================================================
-- PASO 3: EJECUTAR EN BD DESTINO (despues de insertar la data)
-- Actualizar secuencias para que el proximo valor sea mayor al MAX ID
-- ============================================================================

-- Actualizar SHM_TABLA_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_TABLA), 0) INTO v_max_id FROM SHM_TABLA;
    SELECT SHM_TABLA_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_TABLA_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_TABLA_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_TABLA_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_TABLA_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- Actualizar SHM_TABLA_DETALLE_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_TABLA_DETALLE), 0) INTO v_max_id FROM SHM_TABLA_DETALLE;
    SELECT SHM_TABLA_DETALLE_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_TABLA_DETALLE_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_TABLA_DETALLE_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_TABLA_DETALLE_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_TABLA_DETALLE_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- Actualizar SHM_BANCO_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_BANCO), 0) INTO v_max_id FROM SHM_BANCO;
    SELECT SHM_BANCO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_BANCO_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_BANCO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_BANCO_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_BANCO_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- Actualizar SHM_PARAMETRO_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_PARAMETRO), 0) INTO v_max_id FROM SHM_PARAMETRO;
    SELECT SHM_PARAMETRO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_PARAMETRO_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_PARAMETRO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_PARAMETRO_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_PARAMETRO_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================
