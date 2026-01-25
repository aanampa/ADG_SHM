-- ============================================
-- Tabla: SHM_EMAIL_LOG
-- Descripcion: Log de correos electronicos enviados por el sistema
-- Autor: ADG Antonio
-- Fecha: 2026-01-25
-- ============================================

CREATE TABLE SHM_EMAIL_LOG (
    ID_EMAIL_LOG         NUMBER PRIMARY KEY,

    -- Informacion del destinatario
    EMAIL_DESTINO        VARCHAR2(200) NOT NULL,
    NOMBRE_DESTINO       VARCHAR2(200),

    -- Informacion del mensaje
    ASUNTO               VARCHAR2(500) NOT NULL,
    TIPO_EMAIL           VARCHAR2(50) NOT NULL,
    CONTENIDO            CLOB,
    ES_HTML              NUMBER(1) DEFAULT 0,

    -- Estado del envio
    ESTADO               VARCHAR2(20) NOT NULL,
    MENSAJE_ERROR        VARCHAR2(4000),

    -- Referencia opcional a entidades relacionadas
    ID_USUARIO           NUMBER,
    ID_ENTIDAD_MEDICA    NUMBER,
    ENTIDAD_REFERENCIA   VARCHAR2(50),
    ID_REFERENCIA        NUMBER,

    -- Informacion tecnica
    SERVIDOR_SMTP        VARCHAR2(100),
    IP_ORIGEN            VARCHAR2(50),

    -- Campos de auditoria estandar
    GUID_REGISTRO        VARCHAR2(100) NOT NULL,
    ACTIVO               NUMBER(1) DEFAULT 1,
    ID_CREADOR           NUMBER,
    FECHA_CREACION       DATE DEFAULT SYSDATE,
    ID_MODIFICADOR       NUMBER,
    FECHA_MODIFICACION   DATE
);

-- Secuencia
CREATE SEQUENCE SHM_EMAIL_LOG_SEQ START WITH 1 INCREMENT BY 1;

/*
-- Indices
CREATE INDEX IDX_EMAIL_LOG_EMAIL ON SHM_EMAIL_LOG(EMAIL_DESTINO);
CREATE INDEX IDX_EMAIL_LOG_TIPO ON SHM_EMAIL_LOG(TIPO_EMAIL);
CREATE INDEX IDX_EMAIL_LOG_ESTADO ON SHM_EMAIL_LOG(ESTADO);
CREATE INDEX IDX_EMAIL_LOG_FECHA ON SHM_EMAIL_LOG(FECHA_CREACION);
CREATE INDEX IDX_EMAIL_LOG_USUARIO ON SHM_EMAIL_LOG(ID_USUARIO);

-- Comentarios
COMMENT ON TABLE SHM_EMAIL_LOG IS 'Log de correos electronicos enviados por el sistema';
COMMENT ON COLUMN SHM_EMAIL_LOG.TIPO_EMAIL IS 'Tipo de email: RECUPERACION_CLAVE, NOTIFICACION, SOLICITUD_FACTURA, etc.';
COMMENT ON COLUMN SHM_EMAIL_LOG.ESTADO IS 'Estado del envio: ENVIADO, ERROR';
COMMENT ON COLUMN SHM_EMAIL_LOG.CONTENIDO IS 'Contenido completo del email (HTML o texto plano)';
COMMENT ON COLUMN SHM_EMAIL_LOG.ES_HTML IS '1=HTML, 0=Texto plano';
*/