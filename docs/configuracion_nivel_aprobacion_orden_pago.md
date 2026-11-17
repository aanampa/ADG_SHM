# Cómo agregar un nivel de aprobación a la orden de pago

Agregar un nivel es **configuración**: no requiere publicar código. Al crear una orden de pago, el sistema genera una aprobación por cada perfil activo del grupo `FLUJO_APROBACION_ORDEN_PAGO`, ordenado por `ORDEN`. Los niveles se aprueban en secuencia.

| Dato | Valor actual |
|------|--------------|
| Nivel 1 | `JEFE_SEDE` (Jefe Sede), orden 1 |
| Nivel 2 | `JEFE_CORPORATIVO` (Jefe Corporativo), orden 2 |

> **Aplica solo a las órdenes que se creen después del cambio.** Las órdenes que ya están en `APROBACION_PENDIENTE` no tienen el nivel nuevo (ver el paso 6 para agregarlo a mano).

---

## 1. Antes de empezar

Anotar cómo está hoy el flujo.

**Perfiles del flujo**
```sql
SELECT ID_PERFIL_APROBACION, CODIGO, DESCRIPCION, NIVEL, ORDEN, ACTIVO
  FROM SHM_PERFIL_APROBACION
 WHERE GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'
 ORDER BY ORDEN;
```

**Usuarios asignados a cada perfil**
```sql
SELECT pa.ORDEN, pa.CODIGO AS PERFIL, u.LOGIN, u.NOMBRES, u.APELLIDO_PATERNO,
       u.EMAIL, NVL(s.NOMBRE, 'TODAS') AS SEDE, u.ACTIVO
  FROM SHM_PERFIL_APROBACION_USUARIO pau
  JOIN SHM_PERFIL_APROBACION pa ON pa.ID_PERFIL_APROBACION = pau.ID_PERFIL_APROBACION
  JOIN SHM_SEG_USUARIO u        ON u.ID_USUARIO = pau.ID_USUARIO
  LEFT JOIN SHM_SEDE s          ON s.ID_SEDE = pau.ID_SEDE
 WHERE pa.GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'
 ORDER BY pa.ORDEN, u.LOGIN;
```

---

## 2. Crear el perfil

**Por pantalla.** Portal Administrativo → mantenimiento de **Perfiles de Aprobación** (`PerfilAprobacion/Index`) → **Nuevo**.

| Campo | Qué poner |
|-------|-----------|
| Grupo Flujo Trabajo | `FLUJO_APROBACION_ORDEN_PAGO` (exactamente así, sin espacios) |
| Código | Único y en mayúsculas, por ejemplo `GERENTE_FINANZAS` |
| Descripción | Nombre que verán los usuarios, por ejemplo `Gerente de Finanzas` |
| Nivel | Texto libre (los actuales usan `SEDE` y `CORP`) |
| Orden | El número del nivel. Para agregarlo al final, uno más que el último (por ejemplo `3`) |

**Por SQL** (alternativa):
```sql
INSERT INTO SHM_PERFIL_APROBACION
  (ID_PERFIL_APROBACION, GRUPO_FLUJO_TRABAJO, CODIGO, DESCRIPCION, NIVEL, ORDEN,
   GUID_REGISTRO, ACTIVO, ID_USUARIO_CREADOR, FECHA_CREACION)
VALUES
  (SHM_PERFIL_APROBACION_SEQ.NEXTVAL, 'FLUJO_APROBACION_ORDEN_PAGO', 'GERENTE_FINANZAS',
   'Gerente de Finanzas', 'CORP', 3, SYS_GUID(), 1, 1, SYSDATE);
COMMIT;
```

**Si el nivel va en medio** (por ejemplo, entre el 1 y el 2), cambiar antes el `ORDEN` de los perfiles que quedan después, para que no haya dos con el mismo número.

**Verificar** (debe aparecer el perfil nuevo, y ningún `ORDEN` repetido):
```sql
SELECT ORDEN, CODIGO, DESCRIPCION, ACTIVO
  FROM SHM_PERFIL_APROBACION
 WHERE GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'
 ORDER BY ORDEN;

SELECT ORDEN, COUNT(*) FROM SHM_PERFIL_APROBACION
 WHERE GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO' AND ACTIVO = 1
 GROUP BY ORDEN HAVING COUNT(*) > 1;   -- no debe devolver filas
```

---

## 3. Asignar los usuarios

**Por pantalla.** Portal Administrativo → **Usuarios → Internos** → en la fila del usuario, el botón **Perfil de Aprobación** (ícono de escudo) → elegir el **Perfil** → opcionalmente la **Sede** → **Agregar**.

- **Sede vacía ("Todas"):** el usuario puede aprobar órdenes de cualquier sede.
- **Con sede:** solo aprueba las órdenes de esa sede.
- Un usuario solo puede tener **una asignación por perfil**. No se pueden asignar dos sedes distintas del mismo perfil al mismo usuario: en ese caso, dejar la sede en blanco o usar otro usuario.

**Por SQL** (alternativa):
```sql
INSERT INTO SHM_PERFIL_APROBACION_USUARIO (ID_PERFIL_APROBACION, ID_USUARIO, ID_SEDE)
VALUES (
  (SELECT ID_PERFIL_APROBACION FROM SHM_PERFIL_APROBACION WHERE CODIGO = 'GERENTE_FINANZAS'),
  (SELECT ID_USUARIO FROM SHM_SEG_USUARIO WHERE LOGIN = '<login>'),
  NULL);   -- o el ID_SEDE
COMMIT;
```

**Verificar.** La consulta de usuarios del paso 1 debe listar al usuario en el perfil nuevo. Además:

```sql
-- El perfil nuevo debe tener al menos un usuario activo
SELECT pa.CODIGO, COUNT(u.ID_USUARIO) AS USUARIOS_ACTIVOS
  FROM SHM_PERFIL_APROBACION pa
  LEFT JOIN SHM_PERFIL_APROBACION_USUARIO pau ON pau.ID_PERFIL_APROBACION = pa.ID_PERFIL_APROBACION
  LEFT JOIN SHM_SEG_USUARIO u ON u.ID_USUARIO = pau.ID_USUARIO AND u.ACTIVO = 1
 WHERE pa.GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO' AND pa.ACTIVO = 1
 GROUP BY pa.CODIGO;                   -- ningún perfil debe tener 0

-- Aprobadores sin correo: no reciben la notificación de que les toca aprobar
SELECT u.LOGIN, pa.CODIGO
  FROM SHM_PERFIL_APROBACION_USUARIO pau
  JOIN SHM_PERFIL_APROBACION pa ON pa.ID_PERFIL_APROBACION = pau.ID_PERFIL_APROBACION
  JOIN SHM_SEG_USUARIO u ON u.ID_USUARIO = pau.ID_USUARIO
 WHERE pa.GRUPO_FLUJO_TRABAJO = 'FLUJO_APROBACION_ORDEN_PAGO'
   AND (u.EMAIL IS NULL OR TRIM(u.EMAIL) IS NULL);   -- idealmente sin filas
```

> **Un perfil sin usuarios bloquea las órdenes nuevas:** la orden se queda esperando ese nivel y nadie puede aprobarla.

---

## 4. Probar con una orden nueva

1. Generar una orden de pago nueva (de una sede con aprobadores en todos los niveles).
2. Verificar que se creó una aprobación por nivel, incluido el nuevo:

```sql
SELECT opa.ORDEN, pa.CODIGO AS PERFIL, opa.ESTADO, opa.ID_USUARIO_APROBADOR, opa.FECHA_APROBACION
  FROM SHM_ORDEN_PAGO_APROBACION opa
  JOIN SHM_ORDEN_PAGO op        ON op.ID_ORDEN_PAGO = opa.ID_ORDEN_PAGO
  JOIN SHM_PERFIL_APROBACION pa ON pa.ID_PERFIL_APROBACION = opa.ID_PERFIL_APROBACION
 WHERE op.NUMERO_ORDEN_PAGO = '<numero de orden>' AND opa.ACTIVO = 1
 ORDER BY opa.ORDEN;
```

Resultado esperado: una fila por nivel, todas en `APROBACION_PENDIENTE`.

3. Aprobar en orden. Después de cada aprobación, repetir la consulta:
   - El nivel aprobado pasa a `APROBADO`, con usuario y fecha.
   - El usuario del siguiente nivel recibe el correo.
   - Un usuario de un nivel posterior **no puede** aprobar mientras haya niveles anteriores sin aprobar.
4. Al aprobar el último nivel (el nuevo), la orden pasa a `APROBADO`:

```sql
SELECT NUMERO_ORDEN_PAGO, ESTADO FROM SHM_ORDEN_PAGO WHERE NUMERO_ORDEN_PAGO = '<numero de orden>';
```

---

## 5. Órdenes pendientes que ya existen

Para ver cuáles quedaron sin el nivel nuevo:

```sql
SELECT op.NUMERO_ORDEN_PAGO, op.ESTADO, op.FECHA_GENERACION
  FROM SHM_ORDEN_PAGO op
 WHERE op.ESTADO = 'APROBACION_PENDIENTE' AND op.ACTIVO = 1
   AND NOT EXISTS (
       SELECT 1 FROM SHM_ORDEN_PAGO_APROBACION x
        WHERE x.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO AND x.ACTIVO = 1
          AND x.ID_PERFIL_APROBACION =
              (SELECT ID_PERFIL_APROBACION FROM SHM_PERFIL_APROBACION WHERE CODIGO = 'GERENTE_FINANZAS'));
```

Tienen dos opciones: **terminarlas con el flujo anterior** (lo más simple), o **agregarles el nivel a mano**:

```sql
INSERT INTO SHM_ORDEN_PAGO_APROBACION
  (ID_ORDEN_PAGO_APROBACION, ID_ORDEN_PAGO, ID_PERFIL_APROBACION, ESTADO, ORDEN,
   GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION)
SELECT SHM_ORDEN_PAGO_APROBACION_SEQ.NEXTVAL, op.ID_ORDEN_PAGO,
       (SELECT ID_PERFIL_APROBACION FROM SHM_PERFIL_APROBACION WHERE CODIGO = 'GERENTE_FINANZAS'),
       'APROBACION_PENDIENTE',
       (SELECT ORDEN FROM SHM_PERFIL_APROBACION WHERE CODIGO = 'GERENTE_FINANZAS'),
       SYS_GUID(), 1, 1, SYSDATE
  FROM SHM_ORDEN_PAGO op
 WHERE op.NUMERO_ORDEN_PAGO = '<numero de orden>';
COMMIT;
```

Hacerlo solo en órdenes que **todavía no aprobaron su último nivel**. Si una orden ya fue aprobada por completo (`APROBADO`), no se le agrega el nivel.

---

## 6. Desactivar un nivel

En la pantalla de Perfiles de Aprobación, eliminar el perfil (es una baja lógica, `ACTIVO = 0`), o por SQL:

```sql
UPDATE SHM_PERFIL_APROBACION SET ACTIVO = 0 WHERE CODIGO = 'GERENTE_FINANZAS';
COMMIT;
```

Esto afecta solo a las órdenes **nuevas**. Las órdenes pendientes que ya tienen ese nivel lo siguen esperando: hay que aprobarlas o anularlas, o desactivar a mano su aprobación (`ACTIVO = 0` en `SHM_ORDEN_PAGO_APROBACION`).

---

## 7. Limitaciones

- **La bandeja de órdenes muestra solo dos columnas de estado**, fijas para `JEFE_SEDE` y `JEFE_CORPORATIVO`. El nivel nuevo no tiene columna propia. Su estado se ve en el detalle de la orden y en la bitácora. Mostrarlo en la bandeja requiere un cambio de código.
- **Liberación en SAP:** se dispara al aprobar el **último** nivel, sea cual sea. El campo `UserLiberador` toma el login de quien lo aprobó.
- **Correo:** cada nivel notifica a los usuarios del perfil que correspondan a la sede de la orden. Si el usuario no tiene correo, no se entera.
