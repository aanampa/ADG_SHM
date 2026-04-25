
-- Table Asegura que no se repitan los CODIGO_ENTIDAD activos en la tabla SHM_ENTIDAD_MEDICA

CREATE UNIQUE INDEX SHM_ENTIDAD_MEDICA_UK_COD ON SHM_ENTIDAD_MEDICA (
    CASE WHEN ACTIVO = 1 THEN CODIGO_ENTIDAD ELSE NULL END
);

-- Table Asegura que no se repitan los CODIGO activos en la tabla SHM_SEDE
CREATE UNIQUE INDEX SHM_SEDE_UK_COD ON SHM_SEDE (
    CASE WHEN ACTIVO = 1 THEN CODIGO ELSE NULL END
);

-- Indice para filtrado de producciones por compania medica y estado comprobante
-- Cubre las consultas de las bandejas Pendientes y Enviadas del portal Compania Medica
CREATE INDEX IDX_SHM_PRODUCCION_ENTIDAD ON SHM_PRODUCCION (ID_ENTIDAD_MEDICA);

