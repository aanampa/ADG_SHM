-- ============================================================
-- Script: Simular envio de factura desde portal Compania Medica
-- Descripcion: Para cada produccion en estado FACTURA_SOLICITADA,
--              actualiza el estado a FACTURA_ENVIADA y registra
--              los archivos de comprobante (PDF y XML) usando
--              los archivos de prueba ID_ARCHIVO 33 y 34.
--              Incluye ID_CUENTA_BANCO: primera cuenta activa de la
--              entidad medica (replica logica de ConfirmarEnvio).
-- Autor: ADG Vladimir D
-- Fecha: 2026-04-08
-- Modificado: ADG Vladimir D - 2026-04-09 - Agregado ID_CUENTA_BANCO
-- ============================================================

-- Verificar registros disponibles antes de ejecutar
SELECT ID_PRODUCCION, NUMERO_PRODUCCION, CODIGO_PRODUCCION,
       ESTADO, MTO_TOTAL, ID_ENTIDAD_MEDICA
FROM SHM_PRODUCCION
WHERE ESTADO = 'FACTURA_SOLICITADA'
  AND ACTIVO = 1
ORDER BY FECHA_CREACION DESC;

-- ============================================================
-- Procedimiento: SP_SIMULAR_ENVIO_FACTURA
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_SIMULAR_ENVIO_FACTURA AS

    v_count         NUMBER := 0;
    v_numero        VARCHAR2(10);
    v_id_cuenta     NUMBER := NULL;

BEGIN

    FOR rec IN (
        SELECT ID_PRODUCCION, CONCEPTO, ID_ENTIDAD_MEDICA
        FROM SHM_PRODUCCION
        WHERE ESTADO = 'FACTURA_SOLICITADA'
          AND ACTIVO = 1
        ORDER BY ID_PRODUCCION
    ) LOOP

        -- Generar numero aleatorio de 4 digitos (1000-9999)
        v_numero := TO_CHAR(TRUNC(DBMS_RANDOM.VALUE(1000, 9999)));

        -- Obtener primera cuenta bancaria activa de la entidad medica
        -- (replica logica de FacturasController.ConfirmarEnvio)
        v_id_cuenta := NULL;
        IF rec.ID_ENTIDAD_MEDICA IS NOT NULL THEN
            BEGIN
                SELECT ID_CUENTA_BANCO
                INTO   v_id_cuenta
                FROM   (
                    SELECT ID_CUENTA_BANCO
                    FROM   SHM_ENTIDAD_CUENTA_BANCO
                    WHERE  ID_ENTIDAD_MEDICA = rec.ID_ENTIDAD_MEDICA
                      AND  ACTIVO = 1
                    ORDER BY ID_CUENTA_BANCO
                )
                WHERE ROWNUM = 1;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    v_id_cuenta := NULL;
            END;
        END IF;

        -- Actualizar produccion a FACTURA_ENVIADA (incluyendo ID_CUENTA_BANCO)
        UPDATE SHM_PRODUCCION
        SET
            ESTADO              = 'FACTURA_ENVIADA',
            ESTADO_COMPROBANTE  = 'ENVIADO',
            SERIE               = 'E001',
            NUMERO              = v_numero,
            FECHA_EMISION       = TRUNC(SYSDATE),
            GLOSA               = rec.CONCEPTO,
            FACTURA_FECHA_ENVIO = SYSDATE,
            ID_CUENTA_BANCO     = v_id_cuenta,
            ID_MODIFICADOR      = 1,
            FECHA_MODIFICACION  = SYSDATE
        WHERE ID_PRODUCCION = rec.ID_PRODUCCION;

        -- Registrar archivo PDF (ID_ARCHIVO = 33)
        INSERT INTO SHM_ARCHIVO_COMPROBANTE (
            ID_ARCHIVO_COMPROBANTE,
            ID_PRODUCCION,
            ID_ARCHIVO,
            TIPO_ARCHIVO,
            DESCRIPCION,
            GUID_REGISTRO,
            ACTIVO,
            ID_CREADOR,
            FECHA_CREACION
        ) VALUES (
            SHM_ARCHIVO_COMPROBANTE_SEQ.NEXTVAL,
            rec.ID_PRODUCCION,
            33,
            'PDF',
            'Factura PDF',
            RAWTOHEX(SYS_GUID()),
            1,
            1,
            SYSDATE
        );

        -- Registrar archivo XML (ID_ARCHIVO = 34)
        INSERT INTO SHM_ARCHIVO_COMPROBANTE (
            ID_ARCHIVO_COMPROBANTE,
            ID_PRODUCCION,
            ID_ARCHIVO,
            TIPO_ARCHIVO,
            DESCRIPCION,
            GUID_REGISTRO,
            ACTIVO,
            ID_CREADOR,
            FECHA_CREACION
        ) VALUES (
            SHM_ARCHIVO_COMPROBANTE_SEQ.NEXTVAL,
            rec.ID_PRODUCCION,
            34,
            'XML',
            'Factura XML',
            RAWTOHEX(SYS_GUID()),
            1,
            1,
            SYSDATE
        );

        v_count := v_count + 1;
        DBMS_OUTPUT.PUT_LINE('Procesado: ID_PRODUCCION=' || rec.ID_PRODUCCION
            || ' | Serie-Numero=E001-' || v_numero
            || ' | ID_CUENTA_BANCO=' || NVL(TO_CHAR(v_id_cuenta), 'NULL'));

    END LOOP;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Total producciones procesadas: ' || v_count);

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('ERROR: ' || SQLERRM);
        RAISE;

END SP_SIMULAR_ENVIO_FACTURA;
/

-- ============================================================
-- Ejecutar el procedimiento
-- ============================================================
BEGIN
    DBMS_OUTPUT.ENABLE(1000000);
    SP_SIMULAR_ENVIO_FACTURA;
END;
/

-- Verificar resultado
SELECT p.ID_PRODUCCION, p.NUMERO_PRODUCCION, p.ESTADO,
       p.SERIE, p.NUMERO, p.FECHA_EMISION, p.FACTURA_FECHA_ENVIO,
       p.ID_CUENTA_BANCO,
       ecb.CUENTA_CORRIENTE,
       ac.ID_ARCHIVO_COMPROBANTE, ac.ID_ARCHIVO, ac.TIPO_ARCHIVO, ac.DESCRIPCION
FROM SHM_PRODUCCION p
JOIN SHM_ARCHIVO_COMPROBANTE ac ON ac.ID_PRODUCCION = p.ID_PRODUCCION
LEFT JOIN SHM_ENTIDAD_CUENTA_BANCO ecb ON ecb.ID_CUENTA_BANCARIA = p.ID_CUENTA_BANCO
WHERE p.ESTADO = 'FACTURA_ENVIADA'
  AND p.ACTIVO = 1
ORDER BY p.ID_PRODUCCION, ac.ID_ARCHIVO;
