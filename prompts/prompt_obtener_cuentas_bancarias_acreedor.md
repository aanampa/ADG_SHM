# Api para consultar las cuentas bancarias de un Acreedor

## API:

GET:
https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet?$filter=CodAcreedor eq '1002210420'

Header
	Accept:application/json
	Authorization: Bearer <token>

Param:
	$filter = CodAcreedor eq '{cod_acreedor}'
	ejemplo: CodAcreedor eq '1002210420'

Response:

200 OK

{
  "d": {
    "results": [
      {
        "__metadata": {
          "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "type": "ZODGS_PORTAL_HHMM_SRV.CTA_ACREEDOR"
        },
        "CodAcreedor": "1002210420",
        "NroCuenta": "1927093505063",
        "NroCtaInterbancaria": "",
        "Moneda": "PEN",
        "CodigoBanco": "BA030",
        "DescripcionBanco": "BANCO DE CRÉDITO DEL PERÚ SOLES"
      },
      {
        "__metadata": {
          "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "type": "ZODGS_PORTAL_HHMM_SRV.CTA_ACREEDOR"
        },
        "CodAcreedor": "1002210420",
        "NroCuenta": "00031180996",
        "NroCtaInterbancaria": "",
        "Moneda": "PEN",
        "CodigoBanco": "BA020",
        "DescripcionBanco": "BANCO DE LA NACIÓN SOLES"
      },
      {
        "__metadata": {
          "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet('1002210420')",
          "type": "ZODGS_PORTAL_HHMM_SRV.CTA_ACREEDOR"
        },
        "CodAcreedor": "1002210420",
        "NroCuenta": "2003004783750",
        "NroCtaInterbancaria": "",
        "Moneda": "PEN",
        "CodigoBanco": "BA040",
        "DescripcionBanco": "INTERBANK SOLES"
      }
    ]
  }
}

400 Bad Request
{
  "error": {
    "code": "ZFI/016",
    "message": {
      "lang": "es",
      "value": "Codigo Acreedor no existe en SAP"
    },
    "innererror": {
      "application": {
        "component_id": "",
        "service_namespace": "/SAP/",
        "service_id": "ZODGS_PORTAL_HHMM_SRV",
        "service_version": "0001"
      },
      "transactionid": "589CBAEA8AF40430E0069B4C9BDF3676",
      "timestamp": "",
      "Error_Resolution": {
        "SAP_Transaction": "",
        "SAP_Note": "See SAP Note 1797736 for error analysis (https://service.sap.com/sap/support/notes/1797736)"
      },
      "errordetails": [
        {
          "ContentID": "",
          "code": "ZFI/016",
          "message": "Codigo Acreedor no existe en SAP",
          "propertyref": "",
          "severity": "warning",
          "transition": false,
          "target": ""
        }
      ]
    }
  }
}


Retornar:

List<SapAcreedorCuentaBancariaDto>

SapAcreedorCuentaBancariaDto {
	public string CodAcreedor { get; set; }
	public string NroCuenta { get; set; }
	public string? NroCtaInterbancaria { get; set; }
	public string Moneda { get; set; }
	public string CodigoBanco { get; set; }
	public string DescripcionBanco { get; set; }

}

