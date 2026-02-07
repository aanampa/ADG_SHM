## Compilacion de solucion

```
cd .\src\
dotnet build
```

## Compilacion de proyecto

```
cd .\src\SHM.AppApiHonorarioMedico\

dotnet publish -c Release -o "C:\appweb\shmappapi" --runtime win-x64 --self-contained false

dotnet publish -c Release -o "D:\Fuentes SGD\ADG_HHMM v1\Deploy\shmappapi" --runtime win-x64 --self-contained false

```


ALTER TABLE SHM_PRODUCCION MODIFY (CODIGO_LIQUIDACION VARCHAR2(20));
ALTER TABLE SHM_PRODUCCION MODIFY (CODIGO_LIQUIDACION VARCHAR2(20));


ALTER TABLE SHM_ENTIDAD_MEDICA
RENAME COLUMN CODIGO_CORRIENTISTA TO CODIGO_CORRENTISTA;

ALTER TABLE SHM_ORDEN_PAGO_PRODUCCION
DROP CONSTRAINT PK_SHM_ORDEN_PAGO_LIQUIDACION;


ALTER TABLE SHM_ORDEN_PAGO_LIQUIDACION ADD CONSTRAINT PK_SHM_ORDEN_PAGO_LIQUIDACION PRIMARY KEY (ID_ORDEN_PAGO_LIQUIDACION)