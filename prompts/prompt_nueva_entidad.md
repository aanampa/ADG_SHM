# Prompt para Agregar Nueva Entidad

## Contexto Previo
Lee el archivo `NOTAS_SESION_CLAUDE.md` para entender el contexto del proyecto.

## Solicitud

Agrega la entidad **Tabla** al proyecto con arquitectura Clean Architecture en `src/`.

### Tabla en Base de Datos
- Nombre tabla: `SHM_TABLA`
- (Describir las columnas de la tabla aqui)

### Secuencia de base de datos para la clave primaria
- SHM_SEG_TABLA_SEQ

### Estructura a Crear

1. **SHM.AppDomain**
   - `Entities/Tabla.cs` - Entidad con las propiedades mapeadas a la tabla
   - `DTOs/Tabla/CreateTablaDto.cs` - DTO para creacion
   - `DTOs/Tabla/UpdateTablaDto.cs` - DTO para actualizacion
   - `DTOs/Tabla/TablaResponseDto.cs` - DTO de respuesta
   - `Interfaces/Repositories/ITablaRepository.cs` - Interface del repositorio
   - `Interfaces/Services/ITablaService.cs` - Interface del servicio

2. **SHM.AppApplication**
   - `Services/TablaService.cs` - Implementacion del servicio

3. **SHM.AppInfrastructure**
   - `Repositories/TablaRepository.cs` - Implementacion del repositorio con Dapper

4. **SHM.AppApiHonorarioMedico**
   - `Controllers/TablaController.cs` - Controller REST con endpoints CRUD
   - Registrar en `Program.cs` la inyeccion de dependencias

### Endpoints a Crear
| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | /api/tabla | Obtener todas las sedes |
| GET | /api/tabla/{id} | Obtener sede por ID |
| POST | /api/tabla | Crear nueva sede |
| PUT | /api/tabla/{id} | Actualizar sede |
| DELETE | /api/tabla/{id} | Desactivar sede (soft delete) |

### Consideraciones
- Usar el mismo patron que la entidad Usuario
- La clase debe llamarse `Tabla` (sin prefijos Shm o ShmSeg)
- Incluir soft delete (campo ACTIVO)
- Mapear columnas de BD con alias en las queries SQL
- Actualizar `NOTAS_SESION_CLAUDE.md` al finalizar

---

## Ejemplo de Uso

```
Lee el contexto de NOTAS_SESION_CLAUDE.md y luego agrega la entidad Tabla
siguiendo el patron de Usuario. La tabla es SHM_TABLA con las columnas:

CREATE TABLE SHM_TABLA (
	ID_TABLA NUMBER NOT NULL ENABLE, 
	CODIGO VARCHAR2(50), 
	DESCRIPCION VARCHAR2(250), 
	ACTIVO NUMBER(1,0), 
	GUID_REGISTRO VARCHAR2(100), 
	ID_CREADOR NUMBER, 
	FECHA_CREACION DATE, 
	ID_MODIFICADOR NUMBER, 
	FECHA_MODIFICACION DATE, 
	CONSTRAINT PK_SHM_TABLA PRIMARY KEY (ID_TABLA)
) 

```
