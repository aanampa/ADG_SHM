-- =============================================================================
-- Consulta: comprobantes sin Codigo SAP (CODSAP_CPM) que impedirian la liberacion
-- Solo consulta, no modifica datos.
-- =============================================================================

-- 1. Comprobantes de ordenes de pago activas (aprobadas o pendientes) sin codigo SAP.
--    Son los que la liberacion marcaria como ERROR "no tiene Codigo SAP de factura".
SELECT op.NUMERO_ORDEN_PAGO,
       op.ESTADO                AS ESTADO_OP,
       em.RUC,
       em.RAZON_SOCIAL,
       em.CODIGO_ACREEDOR,
       p.ID_PRODUCCION,
       p.TIPO_COMPROBANTE,
       p.SERIE,
       p.NUMERO,
       p.FECHA_EMISION,
       p.MTO_TOTAL,
       p.CODSAP_CPM,
       l.ESTADO_LIBERACION,
       l.MENSAJE_SAP
  FROM SHM_ORDEN_PAGO op
  JOIN SHM_ORDEN_PAGO_PRODUCCION opp ON opp.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
  JOIN SHM_PRODUCCION p              ON p.ID_PRODUCCION = opp.ID_PRODUCCION
  JOIN SHM_ENTIDAD_MEDICA em         ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
  LEFT JOIN SHM_ORDEN_PAGO_LIBERACION l
         ON l.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO AND l.ID_PRODUCCION = p.ID_PRODUCCION
 WHERE op.ACTIVO = 1
   AND op.ESTADO = 'APROBADO'
   AND (p.CODSAP_CPM IS NULL OR TRIM(p.CODSAP_CPM) IS NULL)
 ORDER BY op.NUMERO_ORDEN_PAGO, p.SERIE, p.NUMERO;

-- 2. Comprobantes con error de liberacion, con el motivo (para ver que corregir y reintentar).
SELECT op.NUMERO_ORDEN_PAGO,
       p.SERIE || '-' || p.NUMERO AS COMPROBANTE,
       l.ESTADO_LIBERACION,
       l.INTENTOS,
       l.MENSAJE_SAP
  FROM SHM_ORDEN_PAGO_LIBERACION l
  JOIN SHM_ORDEN_PAGO op ON op.ID_ORDEN_PAGO = l.ID_ORDEN_PAGO
  JOIN SHM_PRODUCCION p  ON p.ID_PRODUCCION = l.ID_PRODUCCION
 WHERE l.ACTIVO = 1
   AND l.ESTADO_LIBERACION = 'ERROR'
 ORDER BY op.NUMERO_ORDEN_PAGO, COMPROBANTE;

-- 3. Resumen por compañia: cuantos comprobantes aprobados sin codigo SAP tiene cada una.
SELECT em.RUC,
       em.RAZON_SOCIAL,
       COUNT(*) AS COMPROBANTES_SIN_CODIGO
  FROM SHM_PRODUCCION p
  JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
 WHERE p.ACTIVO = 1
   AND p.SERIE IS NOT NULL
   AND p.NUMERO IS NOT NULL
   AND (p.CODSAP_CPM IS NULL OR TRIM(p.CODSAP_CPM) IS NULL)
   AND p.ESTADO IN ('FACTURA_ACEPTADA', 'FACTURA_ENVIADA_HHMM', 'FACTURA_LIQUIDADA', 'FACTURA_ORDEN_PAGO')
 GROUP BY em.RUC, em.RAZON_SOCIAL
 ORDER BY COMPROBANTES_SIN_CODIGO DESC;
