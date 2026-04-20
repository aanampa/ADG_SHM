Crear un End Point para obtener el estado de pago de un comprobante en SapInterfaceController

GET:
https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/DatosFacturaSet(CodigoAcreedor='3000000305',TipoComprobante='H1',Serie='0E001',NumeroComprobante='0008150',Anio='2026')

Parametros:
- CodigoAcreedor
- TipoComprobante : 
	Si TIPO_COMPROBANTE = 1 Entonces H1
	Si TIPO_COMPROBANTE = 22 Entonces H2

- Serie
	0E001		Concatenar un CERO a la izquierda
- NumeroComprobante
	0008150		Completar CEROS a la izquierda, 7 caracteres
	
- Anio
	2026	, el año se obtiene de la FECHA_EMISION
	
Header:
Accept : application/json
Authorization: Bearer 7SM5WiXAH-GOonRuJ7LohKQaIN51iZ8WkB-3x2I1n45R2KQs

Response 200
{
  "d": {
    "__metadata": {
      "id": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/DatosFacturaSet(CodigoAcreedor='3000000305',TipoComprobante='H1',Serie='0E001',NumeroComprobante='0008150',Anio='2026')",
      "uri": "https://190.12.87.190:8124/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/DatosFacturaSet(CodigoAcreedor='3000000305',TipoComprobante='H1',Serie='0E001',NumeroComprobante='0008150',Anio='2026')",
      "type": "ZODGS_PORTAL_HHMM_SRV.DatosFactura"
    },
    "CodigoAcreedor": "3000000305",
    "TipoComprobante": "H1",
    "Serie": "0E001",
    "NumeroComprobante": "0008150",
    "Anio": "2026",
    "EstadoPago": "EN PROCESO",
    "FechaPago": "2026-04-15",
    "NumeroOperacion": "2000000719",
    "Banco": "BBVA BANCO CONTINENTAL SOLES",
    "CtaBanDeposito": "0190660200364095",
    "MontoPagado": "202.420"
  }
}

--
Response 400 
{
  "error": {
    "code": "ZFI/016",
    "message": {
      "lang": "es",
      "value": "No se encontró datos con los parámetros ingresados"
    },
    "innererror": {
      "application": {
        "component_id": "",
        "service_namespace": "/SAP/",
        "service_id": "ZODGS_PORTAL_HHMM_SRV",
        "service_version": "0001"
      },
      "transactionid": "589CBAEA8AF40450E0069DEE393C49AB",
      "timestamp": "",
      "Error_Resolution": {
        "SAP_Transaction": "",
        "SAP_Note": "See SAP Note 1797736 for error analysis (https://service.sap.com/sap/support/notes/1797736)"
      },
      "errordetails": [
        {
          "ContentID": "",
          "code": "ZFI/016",
          "message": "No se encontró datos con los parámetros ingresados",
          "propertyref": "",
          "severity": "warning",
          "transition": false,
          "target": ""
        }
      ]
    }
  }
}


Adicionalmente, en la tabla SHM_PRODUCCION se agregaran los siguientes campos:	
PAGO_ESTADO					VARCHAR2(20)
PAGO_FECHA					DATE
PAGO_NUMERO_OPERACION		VARCHAR2(50)
PAGO_BANCO					VARCHAR2(120)
PAGO_CUENTA_DEPOSITO		VARCHAR2(120)
PAGO_MONTO_PAGADO  			NUMBER	
Actualizar los campos con los datos del estado de la factura