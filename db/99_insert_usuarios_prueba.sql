-- ============================================================
-- Script: Registro de usuarios de prueba para companias medicas
-- Descripcion: Inserta un usuario externo de prueba por cada
--              compania medica activa que no tenga usuario externo.
-- Autor: ADG Vladimir D
-- Fecha: 2026-04-08
-- ============================================================

-- Verificar companias sin usuario externo (consulta previa)
-- SELECT em.ID_ENTIDAD_MEDICA, em.RAZON_SOCIAL, em.RUC
-- FROM SHM_ENTIDAD_MEDICA em
-- WHERE em.ACTIVO = 1
--   AND NOT EXISTS (
--       SELECT 1 FROM SHM_SEG_USUARIO u
--       WHERE u.TIPO_USUARIO = 'E'
--         AND u.ACTIVO = 1
--         AND u.ID_ENTIDAD_MEDICA = em.ID_ENTIDAD_MEDICA
--   );

-- ============================================================
-- Procedimiento: SP_INSERTAR_USUARIOS_PRUEBA
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_INSERTAR_USUARIOS_PRUEBA AS

    v_count        NUMBER := 0;
    v_login        SHM_SEG_USUARIO.LOGIN%TYPE;

BEGIN

    FOR rec IN (
        SELECT em.ID_ENTIDAD_MEDICA, em.RUC
        FROM SHM_ENTIDAD_MEDICA em
        WHERE em.ACTIVO = 1
          AND NOT EXISTS (
              SELECT 1 FROM SHM_SEG_USUARIO u
              WHERE u.TIPO_USUARIO = 'E'
                AND u.ACTIVO = 1
                AND u.ID_ENTIDAD_MEDICA = em.ID_ENTIDAD_MEDICA
          )
    ) LOOP

        -- Usar RUC como login; si ya existe como login agregar sufijo _P
        v_login := rec.RUC;

        -- Verificar si el login ya existe en otro registro
        DECLARE
            v_existe NUMBER;
        BEGIN
            SELECT COUNT(*) INTO v_existe
            FROM SHM_SEG_USUARIO
            WHERE LOGIN = v_login;

            IF v_existe > 0 THEN
                v_login := rec.RUC || '_P';
            END IF;
        END;

        INSERT INTO SHM_SEG_USUARIO (
            ID_USUARIO,
            TIPO_USUARIO,
            LOGIN,
            PASSWORD,
            EMAIL,
            NUMERO_DOCUMENTO,
            NOMBRES,
            APELLIDO_PATERNO,
            APELLIDO_MATERNO,
            TELEFONO,
            CELULAR,
            CARGO,
            ID_ENTIDAD_MEDICA,
            ID_ROL,
            TOKEN_RECUPERACION,
            FECHA_EXPIRACION_TOKEN,
            FLAG_PASSWORD_TEMPORAL,
            GUID_REGISTRO,
            ACTIVO,
            ID_CREADOR,
            FECHA_CREACION,
            ID_MODIFICADOR,
            FECHA_MODIFICACION
        ) VALUES (
            SHM_SEG_USUARIO_SEQ.NEXTVAL,
            'E',
            v_login,
            '$2a$11$Kk4KeMcd4cGi.ddiipQAWeGO8dnejUGdAfJQ15IfUrZI9Wqc0eXAy',
            'vdiazolivet@gmail.com',
            '99999999',
            'JUAN',
            'PEREZ',
            'FLORES',
            NULL,
            '99999999',
            NULL,
            rec.ID_ENTIDAD_MEDICA,
            2,
            NULL,
            NULL,
            0,
            RAWTOHEX(SYS_GUID()),
            1,
            1,
            SYSDATE,
            NULL,
            NULL
        );

        v_count := v_count + 1;
        DBMS_OUTPUT.PUT_LINE('Insertado: LOGIN=' || v_login || ' | ID_ENTIDAD=' || rec.ID_ENTIDAD_MEDICA);

    END LOOP;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Total usuarios insertados: ' || v_count);

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('ERROR: ' || SQLERRM);
        RAISE;

END SP_INSERTAR_USUARIOS_PRUEBA;
/

-- ============================================================
-- Ejecutar el procedimiento
-- ============================================================
BEGIN
    DBMS_OUTPUT.ENABLE(1000000);
    SP_INSERTAR_USUARIOS_PRUEBA;
END;

-- Verificar resultado
SELECT u.ID_USUARIO, u.LOGIN, u.TIPO_USUARIO, u.ACTIVO,
       em.RAZON_SOCIAL, em.RUC
FROM SHM_SEG_USUARIO u
JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = u.ID_ENTIDAD_MEDICA
WHERE u.TIPO_USUARIO = 'E'
  AND u.ACTIVO = 1
ORDER BY u.ID_USUARIO DESC;

-- Verificar resultado 2
SELECT em.ID_ENTIDAD_MEDICA, em.RUC
 FROM SHM_ENTIDAD_MEDICA em
WHERE em.ACTIVO = 1
  AND NOT EXISTS (
      SELECT 1 FROM SHM_SEG_USUARIO u
      WHERE u.TIPO_USUARIO = 'E'
        AND u.ACTIVO = 1
        AND u.ID_ENTIDAD_MEDICA = em.ID_ENTIDAD_MEDICA
        )