--------------------------------------------------------------------------------
-- Backfill: registra en SHM_BITACORA las producciones que ya tienen datos de
-- pago (PAGO_FECHA no nulo) pero no tienen bitacora de la accion
-- ACTUALIZAR_ESTADO_PAGO, porque el registro automatico en bitacora se agrego
-- despues de que esos pagos ya se habian sincronizado desde SAP.
--
-- Se ejecuta UNA SOLA VEZ (idempotente: si se corre de nuevo, no duplica,
-- porque cada produccion solo puede tener un registro de esta accion).
--
-- AUTHOR: ADG Antonio
-- CREATED: 2026-07-21
--------------------------------------------------------------------------------

-- 1) Vista previa: cuantas producciones seran afectadas
SELECT COUNT(*) AS PRODUCCIONES_A_REGISTRAR
FROM SHM_PRODUCCION p
WHERE p.ACTIVO = 1
  AND p.PAGO_FECHA IS NOT NULL
  AND NOT EXISTS (
        SELECT 1
        FROM SHM_BITACORA b
        WHERE b.ENTIDAD = 'SHM_PRODUCCION'
          AND b.ID_ENTIDAD = p.ID_PRODUCCION
          AND b.ACCION = 'ACTUALIZAR_ESTADO_PAGO'
  );

-- 2) Backfill
DECLARE
    v_descripcion  SHM_BITACORA.DESCRIPCION%TYPE;
    v_insertados   PLS_INTEGER := 0;
BEGIN
    FOR p IN (
        SELECT p.ID_PRODUCCION,
               p.TIPO_COMPROBANTE,
               p.SERIE,
               p.NUMERO,
               p.PAGO_ESTADO,
               p.PAGO_FECHA,
               p.PAGO_NUMERO_OPERACION,
               p.PAGO_BANCO,
               p.PAGO_MONTO_PAGADO,
               NVL(p.FECHA_MODIFICACION, p.PAGO_FECHA) AS FECHA_ACCION
        FROM SHM_PRODUCCION p
        WHERE p.ACTIVO = 1
          AND p.PAGO_FECHA IS NOT NULL
          AND NOT EXISTS (
                SELECT 1
                FROM SHM_BITACORA b
                WHERE b.ENTIDAD = 'SHM_PRODUCCION'
                  AND b.ID_ENTIDAD = p.ID_PRODUCCION
                  AND b.ACCION = 'ACTUALIZAR_ESTADO_PAGO'
          )
    )
    LOOP
        v_descripcion := SUBSTR(
            'Estado de pago actualizado desde SAP. Comprobante ' || p.TIPO_COMPROBANTE || ' ' ||
            p.SERIE || '-' || p.NUMERO || ': EstadoPago=' || p.PAGO_ESTADO ||
            ', FechaPago=' || TO_CHAR(p.PAGO_FECHA, 'YYYY-MM-DD') ||
            ', Monto=' || TO_CHAR(p.PAGO_MONTO_PAGADO) ||
            ', Operacion=' || p.PAGO_NUMERO_OPERACION ||
            ', Banco=' || p.PAGO_BANCO ||
            ' [registro retroactivo]',
            1, 500);

        INSERT INTO SHM_BITACORA (
            ID_BITACORA,
            ID_ENTIDAD,
            ENTIDAD,
            ACCION,
            DESCRIPCION,
            FECHA_ACCION,
            GUID_REGISTRO,
            ACTIVO,
            ID_CREADOR,
            FECHA_CREACION
        ) VALUES (
            SHM_BITACORA_SEQ.NEXTVAL,
            p.ID_PRODUCCION,
            'SHM_PRODUCCION',
            'ACTUALIZAR_ESTADO_PAGO',
            v_descripcion,
            p.FECHA_ACCION,
            SYS_GUID(),
            1,
            1,      -- ID_CREADOR: usuario "sistema" (mismo usado por el proceso automatico)
            SYSDATE
        );

        v_insertados := v_insertados + 1;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Registros de bitacora insertados: ' || v_insertados);
    COMMIT;
END;
/

-- 3) Verificacion posterior
SELECT COUNT(*) AS TOTAL_BITACORA_ESTADO_PAGO
FROM SHM_BITACORA
WHERE ENTIDAD = 'SHM_PRODUCCION'
  AND ACCION = 'ACTUALIZAR_ESTADO_PAGO';
