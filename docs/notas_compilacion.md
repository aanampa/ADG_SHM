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