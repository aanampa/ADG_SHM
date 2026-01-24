# Manual de Usuario
## Portal de Honorarios Medicos - Companias Medicas
### SHM.AppWebCompaniaMedica

**Version:** 1.0
**Fecha:** Enero 2026
**Cliente:** Grupo San Pablo

---

## Tabla de Contenidos

1. [Introduccion](#1-introduccion)
2. [Acceso al Sistema](#2-acceso-al-sistema)
3. [Dashboard](#3-dashboard)
4. [Facturas Pendientes](#4-facturas-pendientes)
5. [Subir Factura](#5-subir-factura)
6. [Facturas Enviadas](#6-facturas-enviadas)
7. [Detalle de Factura](#7-detalle-de-factura)
8. [Mi Perfil](#8-mi-perfil)
9. [Configuracion](#9-configuracion)
10. [Cerrar Sesion](#10-cerrar-sesion)

---

## 1. Introduccion

El **Portal de Honorarios Medicos** es una plataforma web que permite a las companias medicas gestionar sus facturas de forma rapida y segura. A traves de este portal podra:

- Visualizar las facturas pendientes de envio
- Subir comprobantes electronicos (PDF y XML)
- Consultar el estado de las facturas enviadas
- Gestionar su informacion personal y bancaria
- Cambiar su contrasena de acceso

### Requisitos del Sistema

- Navegador web actualizado (Chrome, Firefox, Edge o Safari)
- Conexion a internet estable
- Archivos PDF y XML de sus comprobantes electronicos

---

## 2. Acceso al Sistema

### 2.1 Pantalla de Inicio de Sesion

Para acceder al portal, ingrese a la URL proporcionada por el administrador del sistema.

![Pantalla de Login](images/manual/01_login.png)
*Figura 2.1: Pantalla de inicio de sesion*

### 2.2 Credenciales de Acceso

Complete los siguientes campos:

| Campo | Descripcion |
|-------|-------------|
| **Usuario** | Nombre de usuario asignado por el administrador |
| **Contrasena** | Su contrasena personal |
| **Verificacion de Seguridad** | Codigo CAPTCHA de 6 caracteres |

![Verificacion CAPTCHA](images/manual/02_login_captcha.png)
*Figura 2.2: Campo de verificacion de seguridad (CAPTCHA)*

### 2.3 Pasos para Iniciar Sesion

1. Ingrese su **nombre de usuario**
2. Ingrese su **contrasena**
3. Escriba los **6 caracteres** que aparecen en la imagen de verificacion
   - Si no puede leer los caracteres, haga clic en el boton de refrescar (icono de flecha circular)
4. Presione el boton **"Iniciar Sesion"**

### 2.4 Recuperar Contrasena

Si olvido su contrasena:

![Recuperar Contrasena](images/manual/03_recuperar_clave.png)
*Figura 2.3: Formulario de recuperacion de contrasena*

1. Haga clic en **"¿Olvidaste tu contrasena?"**
2. Ingrese su correo electronico registrado
3. Recibira un enlace para restablecer su contrasena
4. Siga las instrucciones del correo electronico

---

## 3. Dashboard

El Dashboard es la pantalla principal del sistema y muestra un resumen de su actividad.

![Dashboard Completo](images/manual/04_dashboard_completo.png)
*Figura 3.1: Vista general del Dashboard*

### 3.1 Tarjetas de Estadisticas

En la parte superior encontrara tres tarjetas con informacion resumida:

![Estadisticas](images/manual/05_dashboard_estadisticas.png)
*Figura 3.2: Tarjetas de estadisticas*

| Tarjeta | Descripcion |
|---------|-------------|
| **Total por Facturar** | Monto total pendiente de facturacion |
| **Facturas Pendientes** | Cantidad de facturas que debe enviar |
| **Facturas Enviadas (Mes)** | Facturas enviadas en el mes actual |

### 3.2 Graficos

![Graficos](images/manual/06_dashboard_graficos.png)
*Figura 3.3: Graficos de facturas por mes y estados*

- **Facturas por Mes:** Grafico de barras que muestra la evolucion mensual de facturas enviadas y pendientes
- **Estados:** Grafico circular que muestra la distribucion de facturas por estado (Aprobadas, En Revision, Pendientes, Pagadas)

### 3.3 Resumen del Mes

Informacion detallada del mes actual:
- Total facturado
- Facturas procesadas
- Tiempo promedio de procesamiento

### 3.4 Notificaciones Recientes

Lista de notificaciones importantes sobre el estado de sus facturas.

---

## 4. Facturas Pendientes

Esta seccion muestra las facturas que tiene pendientes de enviar.

### 4.1 Acceso

Desde el menu lateral, seleccione **"Facturas Pendientes"**.

![Menu Lateral](images/manual/26_menu_lateral.png)
*Figura 4.1: Menu lateral de navegacion*

### 4.2 Lista de Facturas

![Facturas Pendientes](images/manual/07_facturas_pendientes.png)
*Figura 4.2: Lista de facturas pendientes de envio*

La tabla muestra la siguiente informacion:

| Columna | Descripcion |
|---------|-------------|
| **Codigo** | Codigo unico de la produccion |
| **Sede** | Sede o clinica asociada |
| **Concepto** | Descripcion del servicio |
| **Importe** | Monto total a facturar |
| **Fecha Limite** | Fecha maxima para enviar la factura |
| **Estado** | Estado actual de la factura |
| **Acciones** | Boton para subir la factura |

### 4.3 Buscar Facturas

![Busqueda](images/manual/08_facturas_pendientes_busqueda.png)
*Figura 4.3: Campo de busqueda de facturas*

Utilice el campo de busqueda para filtrar facturas por:
- Codigo de produccion
- Sede
- Concepto

### 4.4 Actualizar Lista

Presione el boton **"Actualizar"** para refrescar la lista de facturas pendientes.

---

## 5. Subir Factura

Esta seccion permite enviar los comprobantes electronicos de una factura pendiente.

### 5.1 Acceso

1. Vaya a **"Facturas Pendientes"**
2. Ubique la factura que desea enviar
3. Haga clic en el boton **"Subir Factura"**

### 5.2 Informacion de la Produccion

En la parte superior se muestra la informacion de la produccion (solo lectura):

![Informacion de Produccion](images/manual/09_subir_factura_info.png)
*Figura 5.1: Informacion de la produccion*

- Sede
- Codigo de Produccion
- Concepto
- Importe
- Fecha Limite

### 5.3 Datos Bancarios

Se muestran los datos de su cuenta bancaria registrada:

![Datos Bancarios](images/manual/10_subir_factura_bancarios.png)
*Figura 5.2: Datos bancarios para pago*

- Banco
- Cuenta Corriente
- Cuenta CCI
- Moneda

> **Importante:** Si no tiene cuenta bancaria registrada y el sistema lo requiere, no podra enviar facturas. Contacte al administrador del sistema.

### 5.4 Datos de la Factura

![Formulario de Factura](images/manual/11_subir_factura_formulario.png)
*Figura 5.3: Formulario de datos de la factura*

Complete los siguientes campos obligatorios:

| Campo | Descripcion | Ejemplo |
|-------|-------------|---------|
| **Tipo Comprobante** | Seleccione el tipo de documento | Factura, Boleta, Recibo por Honorarios |
| **Fecha de Emision** | Fecha en que se emitio el comprobante | 15/01/2026 |
| **Serie** | Serie del comprobante (4 caracteres) | F001 |
| **Numero** | Numero del comprobante (8 digitos) | 00001234 |

### 5.5 Documentos Electronicos

![Areas de Carga](images/manual/12_subir_factura_archivos.png)
*Figura 5.4: Areas de carga de documentos electronicos*

Debe adjuntar los siguientes archivos:

| Archivo | Formato | Obligatorio | Descripcion |
|---------|---------|-------------|-------------|
| **PDF** | .pdf | Si | Representacion impresa del comprobante |
| **XML** | .xml | Si | Archivo XML UBL 2.1 de SUNAT |
| **CDR** | .xml | No | Constancia de Recepcion de SUNAT |

### 5.6 Como Adjuntar Archivos

![Archivo Cargado](images/manual/13_subir_factura_archivo_cargado.png)
*Figura 5.5: Archivo cargado exitosamente*

1. Haga clic en el area de carga o arrastre el archivo
2. Seleccione el archivo desde su computadora
3. El sistema validara automaticamente el formato
4. Verifique que aparezca el nombre del archivo cargado

### 5.7 Validaciones Automaticas

El sistema valida automaticamente:
- Que los datos del formulario coincidan con el XML
- Que el RUC emisor corresponda a su entidad medica
- Que la fecha de emision sea valida
- Que el concepto coincida con la descripcion del XML

### 5.8 Enviar Factura

![Confirmacion](images/manual/31_alerta_confirmacion.png)
*Figura 5.6: Dialogo de confirmacion*

1. Verifique que todos los campos esten completos
2. Verifique que los archivos esten cargados correctamente
3. Presione el boton **"Enviar Factura"**
4. Confirme la operacion en el mensaje de confirmacion

![Exito](images/manual/29_alerta_exito.png)
*Figura 5.7: Mensaje de operacion exitosa*

---

## 6. Facturas Enviadas

Esta seccion muestra el historial de facturas que ha enviado.

### 6.1 Acceso

Desde el menu lateral, seleccione **"Facturas Enviadas"**.

### 6.2 Lista de Facturas

![Facturas Enviadas](images/manual/14_facturas_enviadas.png)
*Figura 6.1: Lista de facturas enviadas*

La tabla muestra la siguiente informacion:

| Columna | Descripcion |
|---------|-------------|
| **Codigo** | Codigo de produccion |
| **Sede** | Sede o clinica asociada |
| **Concepto** | Descripcion del servicio |
| **Importe** | Monto total facturado |
| **Fecha Emision** | Fecha del comprobante |
| **Serie-Numero** | Identificador del comprobante |
| **Estado** | Estado del comprobante |
| **Acciones** | Boton para ver detalles |

### 6.3 Estados de Factura

| Estado | Color | Descripcion |
|--------|-------|-------------|
| **Enviado** | Azul | Factura recibida, pendiente de revision |
| **En Revision** | Amarillo | Factura en proceso de validacion |
| **Aprobado** | Verde | Factura validada correctamente |
| **Observado** | Rojo | Factura con observaciones, requiere correccion |
| **Pagado** | Verde oscuro | Factura procesada y pagada |

### 6.4 Buscar y Paginar

![Paginacion](images/manual/15_facturas_enviadas_paginacion.png)
*Figura 6.2: Controles de paginacion*

- Use el campo de busqueda para filtrar resultados
- Navegue entre paginas usando los controles de paginacion
- Cambie la cantidad de registros por pagina (10, 25, 50)

---

## 7. Detalle de Factura

Muestra la informacion completa de una factura enviada.

### 7.1 Acceso

1. Vaya a **"Facturas Enviadas"**
2. Haga clic en el boton **"Ver Detalle"** de la factura

### 7.2 Informacion de la Produccion

![Detalle - Informacion](images/manual/16_detalle_factura_info.png)
*Figura 7.1: Informacion general de la factura*

- Sede, codigo, concepto, periodo
- Tipo de comprobante, serie, numero
- Fecha de emision

### 7.3 Detalle de Importes

![Detalle - Importes](images/manual/17_detalle_factura_importes.png)
*Figura 7.2: Detalle de importes*

- Consumo
- Descuento
- Subtotal
- IGV
- Total

### 7.4 Datos Bancarios

![Detalle - Bancarios](images/manual/18_detalle_factura_bancarios.png)
*Figura 7.3: Datos bancarios para pago*

- Banco
- Cuenta Corriente
- Cuenta CCI
- Moneda

### 7.5 Visor de PDF

![Detalle - PDF](images/manual/19_detalle_factura_pdf.png)
*Figura 7.4: Visor integrado del PDF*

El visor permite visualizar el comprobante sin necesidad de descargarlo.

### 7.6 Archivos Adjuntos

![Detalle - Archivos](images/manual/20_detalle_factura_archivos.png)
*Figura 7.5: Lista de archivos adjuntos*

Haga clic en el boton de descarga junto a cada archivo para obtener una copia.

---

## 8. Mi Perfil

Permite ver y editar su informacion personal.

### 8.1 Acceso

Desde el menu lateral, seleccione **"Mi Perfil"**.

### 8.2 Informacion Personal

![Perfil - Personal](images/manual/21_perfil_personal.png)
*Figura 8.1: Seccion de informacion personal*

Datos que puede editar:
- Nombres
- Apellido Paterno
- Apellido Materno
- Correo Electronico
- Numero de Documento
- Telefono
- Celular
- Cargo / Puesto

**Para editar:**

![Perfil - Edicion](images/manual/22_perfil_personal_edicion.png)
*Figura 8.2: Modo de edicion activado*

1. Haga clic en el boton **"Editar"**
2. Modifique los campos deseados
3. Presione **"Guardar Cambios"**

### 8.3 Entidad Medica

![Perfil - Entidad](images/manual/23_perfil_entidad.png)
*Figura 8.3: Informacion de la entidad medica*

Informacion de su compania medica (solo lectura):
- Codigo de Entidad
- RUC
- Razon Social
- Telefono
- Celular
- Direccion

### 8.4 Informacion Bancaria

![Perfil - Bancaria](images/manual/24_perfil_bancaria.png)
*Figura 8.4: Informacion bancaria*

Datos de sus cuentas bancarias registradas (solo lectura):
- Banco
- Moneda
- Cuenta Corriente
- Cuenta CCI

> **Nota:** Para modificar la informacion bancaria o de la entidad medica, contacte al administrador del sistema.

### 8.5 Seguridad - Cambiar Contrasena

![Perfil - Seguridad](images/manual/25_perfil_seguridad.png)
*Figura 8.5: Seccion de cambio de contrasena*

Para cambiar su contrasena:

1. Ingrese su **contrasena actual**
2. Ingrese la **nueva contrasena** (minimo 8 caracteres)
3. **Confirme** la nueva contrasena
4. Presione **"Cambiar Contrasena"**

**Requisitos de la contrasena:**
- Minimo 8 caracteres
- Incluir mayusculas y minusculas
- Incluir numeros

---

## 9. Configuracion

Seccion para ajustar preferencias del sistema.

### 9.1 Acceso

Desde el menu lateral, seleccione **"Configuracion"**.

### 9.2 Opciones Disponibles

- Preferencias de notificaciones
- Configuracion de idioma
- Otras opciones del sistema

---

## 10. Cerrar Sesion

### 10.1 Como Cerrar Sesion

Existen dos formas:

**Opcion 1 - Menu Lateral:**
1. Haga clic en **"Cerrar Sesion"** en el menu lateral

**Opcion 2 - Barra Superior:**

![Barra Superior](images/manual/27_barra_superior.png)
*Figura 10.1: Barra de navegacion superior*

1. Haga clic en el icono de salida en la esquina superior derecha

### 10.2 Confirmacion

![Modal Logout](images/manual/28_modal_logout.png)
*Figura 10.2: Modal de confirmacion de cierre de sesion*

1. Aparecera un mensaje de confirmacion
2. Presione **"Cerrar Sesion"** para confirmar
3. Sera redirigido a la pantalla de inicio de sesion

> **Importante:** Siempre cierre sesion al terminar de usar el sistema, especialmente en computadoras compartidas.

---

## Preguntas Frecuentes

### ¿Que hago si olvide mi contrasena?
Utilice la opcion "¿Olvidaste tu contrasena?" en la pantalla de login.

### ¿Por que no puedo subir una factura?
Verifique que:
- Tenga cuenta bancaria registrada (si el sistema lo requiere)
- Los archivos PDF y XML sean validos
- Los datos coincidan con el XML

### ¿Como contacto al administrador?
Utilice el enlace "Contacta soporte" en la pantalla de login o comuniquese con el area de sistemas.

### ¿Puedo modificar una factura enviada?
No. Una vez enviada, la factura no puede ser modificada. Contacte al administrador si requiere hacer correcciones.

---

## Mensajes del Sistema

### Mensajes de Exito

![Alerta Exito](images/manual/29_alerta_exito.png)
*Mensaje de operacion exitosa*

Indica que la accion se realizo correctamente.

### Mensajes de Error

![Alerta Error](images/manual/30_alerta_error.png)
*Mensaje de error*

Indica que ocurrio un problema. Verifique los datos e intente nuevamente.

---

## Soporte Tecnico

Para asistencia tecnica, contacte al equipo de soporte:

- **Email:** soporte@gruposanpablo.com.pe
- **Telefono:** (01) 123-4567
- **Horario:** Lunes a Viernes de 8:00 a.m. a 6:00 p.m.

---

*Documento elaborado por el equipo de desarrollo SHM*
*Ultima actualizacion: Enero 2026*
