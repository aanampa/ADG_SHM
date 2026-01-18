# Oracle Database - Entorno de Práctica

Base de datos Oracle XE 21 en Docker.

## Requisitos

- Docker Desktop instalado

## Iniciar la base de datos

```bash
cd c:\PROYECTOS\ORACLE
docker-compose up -d
```

Esperar ~2 minutos la primera vez. Verificar con:

```bash
docker-compose logs -f
```

Cuando aparezca `DATABASE IS READY TO USE!` está lista.

## Detener

```bash
docker-compose down
```

Los datos se mantienen.

## Reiniciar con datos limpios

```bash
docker-compose down -v
docker-compose up -d
```

## Conexión

| Parámetro | Valor |
|-----------|-------|
| Host | localhost |
| Puerto | 11521 |
| Service Name | XEPDB1 |
| Usuario | dev_user |
| Password | DevPass123 |

### SQL*Plus (terminal)

```bash
docker exec -it oracle-practice-db sqlplus shm_dev/DevPass123@//localhost:1521/XEPDB1
```

### .NET Core

```
User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1
```

Paquete NuGet:
```bash
dotnet add package Oracle.ManagedDataAccess.Core
```

### JDBC

```
jdbc:oracle:thin:@localhost:11521/XEPDB1
```

### Python

```python
import oracledb

connection = oracledb.connect(
    user="shm_dev",
    password="DevPass123",
    dsn="localhost:11521/XEPDB1"
)
```

## Usuario administrador (SYS)

```bash
docker exec -it oracle-practice-db sqlplus sys/Admin123@//localhost:1521/XEPDB1 as sysdba
```

## Comandos útiles

```bash
# Ver estado del contenedor
docker ps

# Ver logs
docker-compose logs -f

# Reiniciar
docker-compose restart
```

## Notas

- El contenedor se inicia automáticamente con Docker Desktop
- Los datos persisten en el volumen `oracle_practice_data`
- Puerto 11521 (no estándar) para evitar conflictos
