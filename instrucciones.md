## Instrucciones
En la carpeta src/ crea una solucion segun la estructura señalada mas abajo.

Procesa la tarea completa sin hacer preguntas. Escribe y ejecuta el código directamente basándote en mis especificaciones. Si algo no está claro, haz suposiciones razonables y continúa.

## Tecnologías

- .NET 8.0
- Dapper (ORM ligero)
- Oracle Database 21c XE
- Oracle.ManagedDataAccess.Core
- BCrypt.Net-Next (hash de contraseñas)
- NLog (logging)
- Swagger/OpenAPI

## Estructura de la solucion:

```
src/
	SHM.HonorarioMedico			--> Solucion
		SHM.AppDomian			--> Proyecto Library
			DTOs
				Usuario
			Interfaces
				Repositories
				Services
			Utils
		SHM.AppApplication		--> Proyecto Library
			Services
		SHM.AppInfraestructure	--> Proyecto Library
			Configuratios
			Repositories
		-- SHM.AppApiHonorarioMedico   --> Proyecto API-REST
		SHM.AppWebHonorarioMedico   --> Proyecto ASP NET WEB 
		SHM.AppWebCompaniaMedica    --> Proyecto ASP NET WEB 

```

## Configuración

### Cadena de Conexión

La cadena de conexión está configurada en [appsettings.json](ShmUsuarioApi/appsettings.json):

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=shm_dev;Password=DevPass123;Data Source=localhost:11521/XEPDB1"
}
```
