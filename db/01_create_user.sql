-- ============================================
-- Script: Creación de Usuario y Permisos
-- ============================================

ALTER SESSION SET CONTAINER = XEPDB1;

-- Crear tablespace para el usuario
CREATE TABLESPACE ts_shm
    DATAFILE 'ts_shm.dbf'
    SIZE 100M
    AUTOEXTEND ON
    NEXT 50M
    MAXSIZE 500M;

-- Crear usuario
CREATE USER shm_dev IDENTIFIED BY DevPass123
    DEFAULT TABLESPACE ts_shm
    TEMPORARY TABLESPACE temp
    QUOTA UNLIMITED ON ts_shm;

-- Permisos de sesión y objetos
GRANT CREATE SESSION TO shm_dev;
GRANT CREATE TABLE TO shm_dev;
GRANT CREATE VIEW TO shm_dev;
GRANT CREATE SEQUENCE TO shm_dev;
GRANT CREATE PROCEDURE TO shm_dev;
GRANT CREATE TRIGGER TO shm_dev;
GRANT CREATE TYPE TO shm_dev;
GRANT CREATE SYNONYM TO shm_dev;

-- Permisos adicionales
GRANT SELECT ANY DICTIONARY TO shm_dev;
GRANT DEBUG CONNECT SESSION TO shm_dev;

COMMIT;

SELECT 'Usuario shm_dev creado exitosamente' AS mensaje FROM DUAL;
