-- ============================================================================
-- SCRIPT DE MIGRACION DE DATOS DE SEGURIDAD
-- Tablas: SHM_SEG_ROL, SHM_SEG_OPCION, SHM_SEG_ROL_OPCION,
--         SHM_SEG_USUARIO, SHM_SEG_USUARIO_SEDE
-- Fecha: 2026-02-25
-- Descripcion: Replica la data exacta (con IDs originales, sin secuencias)
-- ============================================================================

-- ============================================================================
-- PASO 1: EJECUTAR EN BD DESTINO (la antigua)
-- Eliminar datos existentes en orden inverso (hijos primero)
-- ============================================================================

DELETE FROM SHM_SEG_USUARIO_SEDE;
DELETE FROM SHM_SEG_ROL_OPCION;
DELETE FROM SHM_SEG_USUARIO;
DELETE FROM SHM_SEG_OPCION;
DELETE FROM SHM_SEG_ROL;
COMMIT;

-- ============================================================================
-- PASO 2: EJECUTAR EN BD ORIGEN (la actualizada)
-- Estas consultas generan los INSERT con IDs fijos.
-- Copiar el resultado y ejecutarlo en la BD destino.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 2.1 Generar INSERTs para SHM_SEG_ROL
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_SEG_ROL (ID_ROL, CODIGO, DESCRIPCION, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_ROL || ', '
    || '''' || REPLACE(CODIGO, '''', '''''') || ''', '
    || '''' || REPLACE(DESCRIPCION, '''', '''''') || ''', '
    || '''' || GUID_REGISTRO || ''', '
    || ACTIVO || ', '
    || ID_CREADOR || ', '
    || 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS''));'
    AS SCRIPT_INSERT
FROM SHM_SEG_ROL
ORDER BY ID_ROL;

-- ---------------------------------------------------------------------------
-- 2.2 Generar INSERTs para SHM_SEG_OPCION
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_SEG_OPCION (ID_OPCION, NOMBRE, URL, ICONO, ORDEN, ID_OPCION_PADRE, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_OPCION || ', '
    || '''' || REPLACE(NOMBRE, '''', '''''') || ''', '
    || CASE WHEN URL IS NULL THEN 'NULL' ELSE '''' || REPLACE(URL, '''', '''''') || '''' END || ', '
    || CASE WHEN ICONO IS NULL THEN 'NULL' ELSE '''' || REPLACE(ICONO, '''', '''''') || '''' END || ', '
    || NVL(TO_CHAR(ORDEN), 'NULL') || ', '
    || NVL(TO_CHAR(ID_OPCION_PADRE), 'NULL') || ', '
    || '''' || GUID_REGISTRO || ''', '
    || ACTIVO || ', '
    || ID_CREADOR || ', '
    || 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS''));'
    AS SCRIPT_INSERT
FROM SHM_SEG_OPCION
ORDER BY ID_OPCION;

-- ---------------------------------------------------------------------------
-- 2.3 Generar INSERTs para SHM_SEG_ROL_OPCION
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_SEG_ROL_OPCION (ID_ROL, ID_OPCION, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_ROL || ', '
    || ID_OPCION || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || ACTIVO || ', '
    || ID_CREADOR || ', '
    || 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS''));'
    AS SCRIPT_INSERT
FROM SHM_SEG_ROL_OPCION
ORDER BY ID_ROL, ID_OPCION;

-- ---------------------------------------------------------------------------
-- 2.4 Generar INSERTs para SHM_SEG_USUARIO
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_SEG_USUARIO (ID_USUARIO, TIPO_USUARIO, LOGIN, PASSWORD, EMAIL, NUMERO_DOCUMENTO, NOMBRES, APELLIDO_PATERNO, APELLIDO_MATERNO, TELEFONO, CELULAR, CARGO, ID_ENTIDAD_MEDICA, ID_ROL, FLAG_PASSWORD_TEMPORAL, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_USUARIO || ', '
    || '''' || TIPO_USUARIO || ''', '
    || '''' || REPLACE(LOGIN, '''', '''''') || ''', '
    || '''' || REPLACE(PASSWORD, '''', '''''') || ''', '
    || CASE WHEN EMAIL IS NULL THEN 'NULL' ELSE '''' || REPLACE(EMAIL, '''', '''''') || '''' END || ', '
    || CASE WHEN NUMERO_DOCUMENTO IS NULL THEN 'NULL' ELSE '''' || NUMERO_DOCUMENTO || '''' END || ', '
    || CASE WHEN NOMBRES IS NULL THEN 'NULL' ELSE '''' || REPLACE(NOMBRES, '''', '''''') || '''' END || ', '
    || CASE WHEN APELLIDO_PATERNO IS NULL THEN 'NULL' ELSE '''' || REPLACE(APELLIDO_PATERNO, '''', '''''') || '''' END || ', '
    || CASE WHEN APELLIDO_MATERNO IS NULL THEN 'NULL' ELSE '''' || REPLACE(APELLIDO_MATERNO, '''', '''''') || '''' END || ', '
    || CASE WHEN TELEFONO IS NULL THEN 'NULL' ELSE '''' || TELEFONO || '''' END || ', '
    || CASE WHEN CELULAR IS NULL THEN 'NULL' ELSE '''' || CELULAR || '''' END || ', '
    || CASE WHEN CARGO IS NULL THEN 'NULL' ELSE '''' || REPLACE(CARGO, '''', '''''') || '''' END || ', '
    || NVL(TO_CHAR(ID_ENTIDAD_MEDICA), 'NULL') || ', '
    || NVL(TO_CHAR(ID_ROL), 'NULL') || ', '
    || NVL(TO_CHAR(FLAG_PASSWORD_TEMPORAL), 'NULL') || ', '
    || '''' || GUID_REGISTRO || ''', '
    || ACTIVO || ', '
    || ID_CREADOR || ', '
    || 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS''));'
    AS SCRIPT_INSERT
FROM SHM_SEG_USUARIO
ORDER BY ID_USUARIO;

-- ---------------------------------------------------------------------------
-- 2.5 Generar INSERTs para SHM_SEG_USUARIO_SEDE
-- ---------------------------------------------------------------------------
SELECT 'INSERT INTO SHM_SEG_USUARIO_SEDE (ID_USUARIO, ID_SEDE, GUID_REGISTRO, ES_ULTIMA_SEDE, ACTIVO, ID_CREADOR, FECHA_CREACION) VALUES ('
    || ID_USUARIO || ', '
    || ID_SEDE || ', '
    || CASE WHEN GUID_REGISTRO IS NULL THEN 'NULL' ELSE '''' || GUID_REGISTRO || '''' END || ', '
    || NVL(TO_CHAR(ES_ULTIMA_SEDE), 'NULL') || ', '
    || NVL(TO_CHAR(ACTIVO), 'NULL') || ', '
    || NVL(TO_CHAR(ID_CREADOR), 'NULL') || ', '
    || 'TO_DATE(''' || TO_CHAR(FECHA_CREACION, 'YYYY-MM-DD HH24:MI:SS') || ''', ''YYYY-MM-DD HH24:MI:SS''));'
    AS SCRIPT_INSERT
FROM SHM_SEG_USUARIO_SEDE
ORDER BY ID_USUARIO, ID_SEDE;

-- ============================================================================
-- PASO 3: EJECUTAR EN BD DESTINO (despues de insertar la data)
-- Actualizar secuencias para que el proximo valor sea mayor al MAX ID
-- ============================================================================

-- Actualizar SHM_SEG_ROL_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_ROL), 0) INTO v_max_id FROM SHM_SEG_ROL;
    SELECT SHM_SEG_ROL_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_ROL_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_SEG_ROL_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_ROL_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_SEG_ROL_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- Actualizar SHM_SEG_OPCION_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_OPCION), 0) INTO v_max_id FROM SHM_SEG_OPCION;
    SELECT SHM_SEG_OPCION_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_OPCION_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_SEG_OPCION_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_OPCION_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_SEG_OPCION_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- Actualizar SHM_SEG_USUARIO_SEQ
DECLARE
    v_max_id NUMBER;
    v_curr_val NUMBER;
    v_diff NUMBER;
BEGIN
    SELECT NVL(MAX(ID_USUARIO), 0) INTO v_max_id FROM SHM_SEG_USUARIO;
    SELECT SHM_SEG_USUARIO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
    v_diff := v_max_id - v_curr_val;
    IF v_diff > 0 THEN
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_USUARIO_SEQ INCREMENT BY ' || v_diff;
        SELECT SHM_SEG_USUARIO_SEQ.NEXTVAL INTO v_curr_val FROM DUAL;
        EXECUTE IMMEDIATE 'ALTER SEQUENCE SHM_SEG_USUARIO_SEQ INCREMENT BY 1';
    END IF;
    DBMS_OUTPUT.PUT_LINE('SHM_SEG_USUARIO_SEQ actualizada. Proximo valor: ' || (v_max_id + 1));
END;
/

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================
