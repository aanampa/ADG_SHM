# Integracion SAP

## 1 Autenticación 

Servicio de autenticación generación de Token acceso SAP (Post)
https://190.12.87.190:8124/sap/bc/sec/oauth2/token

- Autorización
  Tipo: Basic Auth.
	Username: OZUNIGA_OAUT
	Password: 987654321
- Header
	Content-Type: application/x-www-form-urlencoded
- Body
	grant_type: client_credentials
	scope: ZODGS_PORTAL_HHMM_SRV_0001

Respose:

200 OK
{
  "access_token": "rdjD3ijLH9GF_-EIIuM0AZWdSCflDWX5PYB2DCISvddcZL90",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "ZODGS_PORTAL_HHMM_SRV_0001"
}

400 Bad Request (Error en el parámetro de Scope requerido para la generación de Token)
{

}

401 Unauthorized
{
	"error": "invalid_client",
	"error_description": "OAuth 2.0 Client authentication failed. The supplied OAuth 2.0 client credentials are invalid"
}

## 2 Listar Códigos de Banco 

Servicios para obtener Códigos de Banco (GET)
https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet

- Autorización
	Tipo: No Auth.
- Header
	Accept: application/json
	Authorization: Bearer rdjD3ijLH-Co9R2dEKs6bYziiEopgV6WkO2qnsKhJUCg3L8

Response:

200 OK
{
  "d": {
    "results": [
      {
        "__metadata": {
          "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet('BA010')",
          "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet('BA010')",
          "type": "ZODGS_PORTAL_HHMM_SRV.COD_BANCO"
        },
        "CodigoBanco": "BA010",
        "DescripcionBanco": "BANCO CENTRAL DE RESERVA DEL PERÚ SOLES"
      },
      {
        "__metadata": {
          "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet('BA011')",
          "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet('BA011')",
          "type": "ZODGS_PORTAL_HHMM_SRV.COD_BANCO"
        },
        "CodigoBanco": "BA011",
        "DescripcionBanco": "BANCO CENTRAL DE RESERVA DEL PERÚ DÓLARES"
      },
	  
	  .
	  ..
	  ....
    ]
  }
}

401 Unauthorized



