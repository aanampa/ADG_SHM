# Plan de Pruebas Funcionales — SHM Sistema de Honorarios Médicos
**Sistema:** SHM - Sistema de Honorarios Médicos
**Versión:** 1.1
**Fecha:** 2026-03-16

---

## Leyenda de Estados

| Estado | Descripción |
|--------|-------------|
| ⬜ Pendiente | No ejecutado |
| ✅ Aprobado | Ejecutado y resultado correcto |
| ❌ Fallido | Ejecutado con resultado incorrecto |
| ⏭️ Omitido | No aplica para el entorno de prueba |

---

## Alcance y Cobertura por Fase

| Fase del Proceso | Descripción | Actor | Sistema | Cubierto |
|-----------------|-------------|-------|---------|----------|
| **Fase 1** | Registro de Producciones vía API | Sistema HHMM → SHM | API SHM | ✅ |
| **Fase 2** | Emisión de Comprobantes | Compañías Médicas | Portal Compañías | ✅ |
| **Fase 3** | Recepción de Liquidaciones vía API | Sistema HHMM → SHM | API SHM | ✅ |
| **Fase 4** | Generación y Aprobación de Orden de Pago | Área Honorarios / Jefes / Gerente | Portal Administrativo | ✅ |
| **Fase 5** | Ejecución del Pago | Tesorería | Portal Administrativo | ✅ |

---

---

# FASE 1 — REGISTRO DE PRODUCCIONES

> **Actor:** Sistema HHMM (legado)
> **Sistema destino:** API SHM
> **Descripción:** El sistema HHMM envía las producciones médicas al nuevo sistema SHM a través del API. Estas producciones generan las solicitudes de comprobante para las compañías médicas.

## 1. RECEPCIÓN DE PRODUCCIONES VÍA API

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| PR-01 | Recepción de producción válida | 1. HHMM envía producción con todos los campos requeridos | Payload completo y correcto | Producción creada en SHM, aparece en bandeja de Compañía | ⬜ | |
| PR-02 | Recepción con campos con espacios en blanco | 1. HHMM envía producción con strings con espacios extra | Campos con espacios | Espacios eliminados (trim), producción creada correctamente | ⬜ | Validado en `ProduccionInterfaceService` |
| PR-03 | Producción duplicada | 1. HHMM envía dos veces la misma producción | Código producción repetido | Segunda solicitud rechazada o actualizada sin duplicar | ⬜ | |
| PR-04 | Producción con tipo comprobante Factura | 1. Enviar producción con tipo entidad médica = Empresa | TipoEntidadMedica = "1" | Producción creada con tipo comprobante Factura | ⬜ | |
| PR-05 | Producción con tipo comprobante RHE | 1. Enviar producción con tipo entidad médica = Persona Natural | TipoEntidadMedica ≠ "1" | Producción creada con tipo comprobante RHE | ⬜ | |
| PR-06 | Producción sin código de sede | 1. Enviar producción sin CodigoSede | Campo vacío | Error de validación, producción no creada | ⬜ | |
| PR-07 | Producción sin código de entidad | 1. Enviar producción sin CodigoEntidad | Campo vacío | Error de validación, producción no creada | ⬜ | |

---

---

# FASE 2 — EMISIÓN DE COMPROBANTES

> **Actor:** Compañías Médicas
> **Sistema:** Portal Compañías Médicas (`SHM.AppWebCompaniaMedica`)
> **Descripción:** La compañía médica ingresa al portal, revisa sus producciones pendientes, emite el comprobante electrónico y lo sube al sistema para enviarlo a San Pablo.

## 2. AUTENTICACIÓN — Portal Compañías

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| AU-01 | Login exitoso | 1. Ingresar usuario y clave válidos<br>2. Completar captcha<br>3. Clic en Ingresar | Usuario/clave válidos | Redirige al Dashboard | ⬜ | |
| AU-02 | Login fallido - clave incorrecta | 1. Ingresar usuario válido<br>2. Ingresar clave incorrecta<br>3. Clic en Ingresar | Clave errónea | Mensaje de error, no ingresa al sistema | ⬜ | |
| AU-03 | Login fallido - usuario inexistente | 1. Ingresar usuario no registrado<br>2. Clic en Ingresar | Usuario no registrado | Mensaje de error, no ingresa | ⬜ | |
| AU-04 | Login fallido - captcha incorrecto | 1. Ingresar usuario y clave válidos<br>2. Ingresar captcha errado<br>3. Clic en Ingresar | Captcha incorrecto | Mensaje de error de captcha | ⬜ | |
| AU-05 | Logout | 1. Con sesión activa<br>2. Clic en Cerrar Sesión | Sesión activa | Redirige al Login, cookie de sesión eliminada | ⬜ | |
| AU-06 | Sesión expirada | 1. Iniciar sesión<br>2. Dejar inactivo más de 30 minutos<br>3. Intentar navegar | Inactividad > 30 min | Redirige automáticamente al Login | ⬜ | |
| AU-07 | Recuperar clave - email existente | 1. Ir a Recuperar Clave<br>2. Ingresar email registrado<br>3. Clic en Enviar | Email registrado | Mensaje de confirmación de envío | ⬜ | |
| AU-08 | Recuperar clave - email inexistente | 1. Ir a Recuperar Clave<br>2. Ingresar email no registrado<br>3. Clic en Enviar | Email no registrado | Mensaje informativo (no revelar si existe) | ⬜ | |
| AU-09 | Restablecer clave - token válido | 1. Usar enlace de email con token vigente<br>2. Ingresar nueva clave | Token vigente | Permite cambiar clave y redirige al Login | ⬜ | |
| AU-10 | Restablecer clave - token expirado | 1. Usar enlace de email con token vencido | Token vencido | Mensaje de error indicando token inválido | ⬜ | |
| AU-11 | Cambiar clave - clave actual incorrecta | 1. Ir a Cambiar Clave<br>2. Ingresar clave actual incorrecta | Clave actual errónea | Mensaje de error de validación | ⬜ | |
| AU-12 | Cambiar clave - nueva clave no coincide | 1. Ir a Cambiar Clave<br>2. Ingresar nueva clave y confirmación distintas | Confirmación diferente | Mensaje de error de validación | ⬜ | |

---

## 3. COMPROBANTES PENDIENTES

> La compañía ve la bandeja de producciones que esperan un comprobante.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| PE-01 | Listado de pendientes | 1. Iniciar sesión<br>2. Ir a Comprobantes > Pendientes | Sin filtro | Lista producciones pendientes enviadas desde HHMM | ⬜ | Prerequisito: Fase 1 ejecutada |
| PE-02 | Búsqueda por texto | 1. En Pendientes<br>2. Ingresar texto en buscador<br>3. Presionar buscar | Código o nombre parcial | Lista filtrada con coincidencias | ⬜ | |
| PE-03 | Sin registros pendientes | 1. Con usuario sin producciones pendientes<br>2. Ir a Pendientes | — | Mensaje indicando que no hay registros | ⬜ | |
| PE-04 | Solo ve producciones de su entidad | 1. Iniciar sesión con usuario de Compañía A<br>2. Ir a Pendientes | Usuario Compañía A | Solo aparecen producciones de Compañía A, no de otras | ⬜ | Aislamiento por entidad |
| PE-05 | Botón Subir comprobante | 1. En Pendientes<br>2. Clic en botón Subir de una producción | Producción válida | Redirige al formulario de Subir | ⬜ | |

---

## 4. SUBIR COMPROBANTE

> La compañía adjunta los archivos del comprobante emitido en su sistema de facturación electrónica.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| SU-01 | Sin archivos adjuntos | 1. Ir a Subir<br>2. No adjuntar archivos<br>3. Clic en Continuar | Sin archivos | Error: "Los archivos PDF y XML son requeridos" | ⬜ | |
| SU-02 | Falta CDR (si CDR es requerido) | 1. Adjuntar PDF y XML<br>2. No adjuntar CDR<br>3. Clic en Continuar | PDF + XML, sin CDR | Error: "Todos los archivos son requeridos (PDF, XML, CDR)" | ⬜ | Depende del parámetro `SHM_REQUIERE_ARCHIVO_CDR` |
| SU-03 | Fecha de emisión futura | 1. Seleccionar fecha mayor a hoy<br>2. Clic en Continuar | Fecha futura | Error: "La fecha de emisión no puede ser una fecha futura" | ⬜ | |
| SU-04 | XML de Factura en producción RHE | 1. Producción tipo RHE<br>2. Adjuntar XML de Factura<br>3. Continuar | XML Factura + tipo RHE | Error de validación del XML | ⬜ | |
| SU-05 | XML de RHE en producción Factura | 1. Producción tipo Factura<br>2. Adjuntar XML de RHE<br>3. Continuar | XML RHE + tipo Factura | Error de validación del XML | ⬜ | |
| SU-06 | Archivo no es XML válido | 1. Adjuntar archivo .txt renombrado como .xml<br>2. Continuar | Archivo no XML | Error: "El archivo no es un XML válido" | ⬜ | |
| SU-07 | XML sin RUC del emisor | 1. Adjuntar XML sin nodo de emisor<br>2. Continuar | XML incompleto | Error: "El XML no contiene el RUC del emisor" | ⬜ | |
| SU-08 | XML sin documento del cliente | 1. Adjuntar XML sin nodo de cliente<br>2. Continuar | XML incompleto | Error: "El XML no contiene el documento del cliente" | ⬜ | |
| SU-09 | Subida válida - Factura | 1. Adjuntar PDF + XML Factura válido + CDR<br>2. Ingresar fecha correcta<br>3. Continuar | Archivos válidos tipo Factura | Redirige a Vista Previa | ⬜ | |
| SU-10 | Subida válida - RHE | 1. Adjuntar PDF + XML RHE válido + CDR<br>2. Ingresar fecha correcta<br>3. Continuar | Archivos válidos tipo RHE | Redirige a Vista Previa | ⬜ | |

---

## 5. VISTA PREVIA — Validaciones

> El sistema compara los datos del formulario con los datos extraídos del XML antes de confirmar el envío.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| VP-01 | Tipo comprobante coincide - Factura | 1. Subir XML Factura con tipo Factura<br>2. Ver Vista Previa | Formulario "1" / XML "01" | Fila Tipo Comprobante en estado **Correcto** | ⬜ | |
| VP-02 | Tipo comprobante coincide - RHE | 1. Subir XML RHE con tipo RHE<br>2. Ver Vista Previa | Formulario "22" / XML "02" | Fila Tipo Comprobante en estado **Correcto** | ⬜ | |
| VP-03 | Tipo comprobante no coincide | 1. Subir XML RHE pero seleccionar tipo Factura | Tipos distintos | Fila Tipo Comprobante en estado **Observado** | ⬜ | |
| VP-04 | Serie coincide | 1. Ingresar serie igual a la del XML | Misma serie | Fila Serie en estado **Correcto** | ⬜ | |
| VP-05 | Serie no coincide | 1. Ingresar serie distinta a la del XML | Series distintas | Fila Serie en estado **Observado** | ⬜ | |
| VP-06 | Número coincide | 1. Ingresar número igual al del XML | Mismo número | Fila Número en estado **Correcto** | ⬜ | |
| VP-07 | Número no coincide | 1. Ingresar número distinto al del XML | Números distintos | Fila Número en estado **Observado** | ⬜ | |
| VP-08 | Fecha emisión coincide | 1. Ingresar fecha igual a la del XML | Misma fecha | Fila Fecha en estado **Correcto** | ⬜ | |
| VP-09 | Fecha del XML se muestra en dd/MM/yyyy | 1. Subir XML con fecha en formato yyyy-MM-dd<br>2. Ver Vista Previa | Fecha XML: 2026-03-05 | Se muestra: 05/03/2026 | ⬜ | |
| VP-10 | RUC emisor coincide | 1. Subir XML cuyo RUC emisor coincide con el sistema | RUC sistema = RUC XML | Fila RUC Emisor en estado **Correcto** | ⬜ | |
| VP-11 | RUC emisor no coincide | 1. Subir XML con RUC diferente al registrado | RUC distintos | Fila RUC Emisor en estado **Observado** | ⬜ | |
| VP-12 | RUC receptor coincide | 1. Subir XML cuyo RUC receptor coincide con la sede/entidad | RUC cliente = RUC XML | Fila RUC Receptor en estado **Correcto** | ⬜ | |
| VP-13 | Importe coincide | 1. Subir XML con importe igual al de la producción (±0.01) | Montos iguales | Fila Importe en estado **Correcto** | ⬜ | |
| VP-14 | Importe no coincide | 1. Subir XML con importe diferente | Montos distintos | Fila Importe en estado **Observado** | ⬜ | |
| VP-15 | Cancelar vista previa | 1. En Vista Previa<br>2. Clic en Cancelar | — | Archivos temporales eliminados, vuelve a Subir | ⬜ | |
| VP-16 | Ver PDF en vista previa | 1. En Vista Previa<br>2. Visualizar panel de PDF | PDF válido | PDF se muestra correctamente en el panel | ⬜ | |

---

## 6. CONFIRMAR ENVÍO — Integración San Pablo (HHMM)

> Al confirmar, el sistema registra el comprobante localmente y lo notifica al sistema San Pablo HHMM. Si el envío falla, la producción se revierte automáticamente.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| CE-01 | Envío exitoso a San Pablo | 1. En Vista Previa<br>2. Clic en Confirmar Envío | San Pablo: `IsSuccess: true` | Mensaje "Factura enviada exitosamente", producción pasa a estado FACTURA_ENVIADA_HHMM | ⬜ | |
| CE-02 | San Pablo rechaza con mensaje | 1. En Vista Previa<br>2. Confirmar Envío<br>3. San Pablo responde con error con mensaje | `IsSuccess: false, Message: "texto de error"` | Mensaje "Error al enviar el comprobante: texto de error", producción revertida | ⬜ | |
| CE-03 | San Pablo rechaza sin mensaje | 1. En Vista Previa<br>2. Confirmar Envío<br>3. San Pablo responde sin mensaje | `IsSuccess: false, Message: null` | Mensaje "Error al enviar el comprobante: No se pudo enviar el comprobante", producción revertida | ⬜ | |
| CE-04 | San Pablo no disponible | 1. Con servicio San Pablo caído<br>2. Confirmar Envío | Timeout / conexión rechazada | Mensaje de error genérico, producción revertida | ⬜ | |
| CE-05 | Estado producción revertido tras error | 1. Provocar error en envío<br>2. Verificar en bandeja de Pendientes | Error en envío | Producción vuelve a aparecer en Pendientes | ⬜ | |
| CE-06 | Producción desaparece de Pendientes tras éxito | 1. Envío exitoso<br>2. Verificar bandeja de Pendientes | — | Producción ya no aparece en Pendientes | ⬜ | |
| CE-07 | Bitácora tras envío exitoso | 1. Envío exitoso<br>2. Ver bitácora de la producción | — | Registro con acción `FACTURA_ENVIADA_HHMM` | ⬜ | |
| CE-08 | Bitácora tras error de envío | 1. Envío fallido<br>2. Ver bitácora de la producción | — | Registro con acción `ERROR_ENVIO_HHMM` | ⬜ | |

---

## 7. COMPROBANTES ENVIADOS

> La compañía puede consultar el historial de comprobantes ya enviados.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| EN-01 | Listado de enviados | 1. Ir a Comprobantes > Enviados | Sin filtro | Lista comprobantes enviados del usuario/entidad | ⬜ | |
| EN-02 | Búsqueda por texto | 1. En Enviados<br>2. Ingresar texto en buscador | Código o nombre parcial | Lista filtrada con coincidencias | ⬜ | |
| EN-03 | Paginación | 1. Con más de una página de resultados<br>2. Navegar entre páginas | Múltiples registros | Paginación correcta | ⬜ | |
| EN-04 | Ver detalle de comprobante | 1. En Enviados<br>2. Clic en Ver Detalle | Comprobante enviado | Muestra detalle completo del comprobante | ⬜ | |
| EN-05 | Descarga de PDF | 1. En Detalle<br>2. Clic en Descargar PDF | Archivo PDF existente | Descarga el archivo PDF | ⬜ | |
| EN-06 | Descarga de XML | 1. En Detalle<br>2. Clic en Descargar XML | Archivo XML existente | Descarga el archivo XML | ⬜ | |
| EN-07 | Descarga de CDR | 1. En Detalle<br>2. Clic en Descargar CDR | Archivo CDR existente | Descarga el archivo CDR | ⬜ | |

---

## 8. EVALUADOR XML (Herramienta de Diagnóstico)

> Herramienta de apoyo para que la compañía verifique su XML antes de subirlo.

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| EX-01 | Evaluar XML Factura válido | 1. Ir a Herramientas > Evaluador XML<br>2. Seleccionar tipo Factura<br>3. Cargar XML de factura<br>4. Clic en Evaluar | XML Factura válido | Muestra estado válido y todos los datos extraídos (emisor, cliente, importes, ítems) | ⬜ | |
| EX-02 | Evaluar XML RHE válido | 1. Seleccionar tipo RHE<br>2. Cargar XML de RHE<br>3. Clic en Evaluar | XML RHE válido | Muestra estado válido y todos los datos extraídos | ⬜ | |
| EX-03 | Tipo incorrecto para el XML | 1. Seleccionar tipo Factura<br>2. Cargar XML de RHE | Tipo no coincide con XML | Mensaje de error de validación | ⬜ | |
| EX-04 | Archivo no es XML | 1. Cargar archivo .pdf o .txt renombrado<br>2. Clic en Evaluar | Archivo no XML | Mensaje "El archivo no es un XML válido" | ⬜ | |
| EX-05 | Sin archivo seleccionado | 1. No seleccionar archivo<br>2. Clic en Evaluar | Sin archivo | Validación en frontend, no envía | ⬜ | |
| EX-06 | XML RHE - verificar fecha en dd/MM/yyyy | 1. Evaluar XML RHE con fecha en yyyy-MM-dd | Fecha XML: 2026-03-05 | Fecha mostrada como 05/03/2026 | ⬜ | |

---

---

# FASE 3 — RECEPCIÓN DE LIQUIDACIONES

> **Actor:** Área de Honorarios Médicos / Sistema HHMM
> **Sistema destino:** API SHM
> **Descripción:** El área de honorarios recepciona las facturas en el sistema HHMM, genera las liquidaciones y las envía al nuevo sistema SHM.

## 9. RECEPCIÓN DE LIQUIDACIONES VÍA API

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| LQ-01 | Recepción de liquidación válida | 1. HHMM envía liquidación con todos los datos requeridos | Payload completo | Liquidación creada en SHM y asociada a la producción correspondiente | ⬜ | Prerequisito: Fase 2 ejecutada |
| LQ-02 | Liquidación asociada a producción existente | 1. Enviar liquidación con código de producción válido | Código producción existente | Liquidación vinculada correctamente a la producción | ⬜ | |
| LQ-03 | Liquidación con producción inexistente | 1. Enviar liquidación con código de producción no existente | Código producción inválido | Error de validación, liquidación no creada | ⬜ | |
| LQ-04 | Múltiples liquidaciones para una producción | 1. Enviar más de una liquidación para la misma producción | Código producción repetido | Sistema maneja correctamente la asociación | ⬜ | |

---

---

# FASE 4 — ORDEN DE PAGO Y APROBACIÓN

> **Actor:** Área de Honorarios Médicos → Jefe de Sede → Gerente Corporativo
> **Sistema:** Portal Administrativo (`SHM.AppWebHonorarioMedico`)
> **Descripción:** El sistema agrupa las facturas recibidas para generar órdenes de pago que pasan por un flujo de aprobación de dos niveles.

## 10. GENERACIÓN DE ORDEN DE PAGO

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| OP-01 | Generar orden de pago | 1. Seleccionar facturas a agrupar<br>2. Generar orden de pago | Facturas con liquidaciones | Orden de pago creada con estado "Pendiente de Aprobación" | ⬜ | Prerequisito: Fase 3 ejecutada |
| OP-02 | Orden incluye todas las facturas seleccionadas | 1. Generar orden con 3 facturas<br>2. Ver detalle de orden | 3 facturas seleccionadas | Orden contiene exactamente las 3 facturas | ⬜ | |
| OP-03 | Orden sin facturas seleccionadas | 1. Intentar generar orden sin seleccionar facturas | Sin selección | Error de validación | ⬜ | |
| OP-04 | Monto total de la orden | 1. Generar orden<br>2. Verificar monto total | Facturas con montos conocidos | Monto total = suma de todas las facturas incluidas | ⬜ | |

---

## 11. APROBACIÓN — Jefe de Sede (1er nivel)

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| AP-01 | Jefe de Sede ve órdenes pendientes | 1. Iniciar sesión como Jefe de Sede<br>2. Ir a Órdenes de Pago | Sesión Jefe de Sede | Lista de órdenes pendientes de su 1° aprobación | ⬜ | |
| AP-02 | Jefe aprueba orden de pago | 1. Seleccionar orden pendiente<br>2. Clic en Aprobar<br>3. Confirmar | Orden en estado "Pendiente 1° Aprobación" | Orden pasa a estado "Pendiente 2° Aprobación" (Gerente) | ⬜ | |
| AP-03 | Jefe rechaza orden de pago | 1. Seleccionar orden pendiente<br>2. Clic en Rechazar<br>3. Ingresar motivo | Motivo de rechazo | Orden devuelta, estado "Rechazada Jefe Sede" | ⬜ | |
| AP-04 | Jefe no puede aprobar orden de otra sede | 1. Iniciar sesión como Jefe de Sede A<br>2. Intentar aprobar orden de Sede B | Orden de otra sede | Error o no visible en su bandeja | ⬜ | Aislamiento por sede |
| AP-05 | Usuario sin rol Jefe no puede aprobar | 1. Iniciar sesión como usuario sin rol de Jefe<br>2. Intentar aprobar orden | Rol incorrecto | Acceso denegado | ⬜ | |

---

## 12. APROBACIÓN — Gerente Corporativo (2do nivel)

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| AP-06 | Gerente ve órdenes pendientes de 2° aprobación | 1. Iniciar sesión como Gerente Corporativo<br>2. Ir a Órdenes de Pago | Sesión Gerente | Lista de órdenes aprobadas por Jefe de Sede pendientes de su aprobación | ⬜ | |
| AP-07 | Gerente aprueba orden de pago | 1. Seleccionar orden<br>2. Clic en Aprobar<br>3. Confirmar | Orden en estado "Pendiente 2° Aprobación" | Orden pasa a estado "Aprobada - Pendiente de Pago" | ⬜ | |
| AP-08 | Gerente rechaza orden de pago | 1. Seleccionar orden<br>2. Clic en Rechazar<br>3. Ingresar motivo | Motivo de rechazo | Orden devuelta, estado "Rechazada Gerente" | ⬜ | |
| AP-09 | Gerente no puede aprobar orden no aprobada por Jefe | 1. Intentar aprobar orden que aún no pasó el 1° nivel | Orden en estado "Pendiente 1° Aprobación" | No aparece en la bandeja del Gerente | ⬜ | |
| AP-10 | Usuario sin rol Gerente no puede aprobar 2° nivel | 1. Iniciar sesión como Jefe de Sede<br>2. Intentar hacer 2° aprobación | Rol incorrecto | Acceso denegado | ⬜ | |

---

---

# FASE 5 — PAGO

> **Actor:** Tesorería
> **Sistema:** Portal Administrativo (`SHM.AppWebHonorarioMedico`)
> **Descripción:** Tesorería recibe las órdenes de pago aprobadas y registra la ejecución del pago.

## 13. EJECUCIÓN DEL PAGO

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| PA-01 | Tesorería ve órdenes aprobadas pendientes de pago | 1. Iniciar sesión como Tesorería<br>2. Ir a Órdenes de Pago | Sesión Tesorería | Lista de órdenes con estado "Aprobada - Pendiente de Pago" | ⬜ | Prerequisito: Fase 4 ejecutada |
| PA-02 | Registrar pago de orden | 1. Seleccionar orden aprobada<br>2. Registrar datos del pago<br>3. Confirmar | Datos de pago válidos | Orden pasa a estado "Pagada", producción actualizada | ⬜ | |
| PA-03 | Tesorería no ve órdenes no aprobadas | 1. Iniciar sesión como Tesorería<br>2. Verificar que no aparecen órdenes pendientes de aprobación | — | Solo órdenes completamente aprobadas son visibles | ⬜ | |
| PA-04 | Usuario sin rol Tesorería no puede registrar pago | 1. Iniciar sesión como usuario sin rol Tesorería<br>2. Intentar registrar pago | Rol incorrecto | Acceso denegado | ⬜ | |

---

---

# TRANSVERSAL — SEGURIDAD Y ACCESO

## 14. CONTROL DE ACCESO

| ID | Caso de prueba | Pasos | Datos de entrada | Resultado esperado | Estado | Observaciones |
|----|----------------|-------|------------------|--------------------|--------|---------------|
| SE-01 | Acceso sin sesión redirige a Login | 1. Intentar acceder a URL protegida sin sesión activa | Sin cookie de sesión | Redirige al Login | ⬜ | |
| SE-02 | URLs con GUID no exponen IDs internos | 1. Revisar URLs en el navegador al navegar el sistema | Cualquier página de detalle | Las URLs usan GUID, no IDs numéricos | ⬜ | |
| SE-03 | Compañía no accede a producciones de otra compañía | 1. Obtener GUID de producción de otra compañía<br>2. Intentar acceder directamente via URL | GUID de otra compañía | Error 403 o redirección, sin datos expuestos | ⬜ | |

---

---

## Resumen de Ejecución

| Fase | Sección | Total | ✅ Aprobados | ❌ Fallidos | ⏭️ Omitidos | ⬜ Pendientes |
|------|---------|-------|-------------|------------|------------|--------------|
| **Fase 1** | 1. Recepción de Producciones (API) | 7 | | | | 7 |
| **Fase 2** | 2. Autenticación | 12 | | | | 12 |
| **Fase 2** | 3. Comprobantes Pendientes | 5 | | | | 5 |
| **Fase 2** | 4. Subir Comprobante | 10 | | | | 10 |
| **Fase 2** | 5. Vista Previa | 16 | | | | 16 |
| **Fase 2** | 6. Confirmar Envío | 8 | | | | 8 |
| **Fase 2** | 7. Comprobantes Enviados | 7 | | | | 7 |
| **Fase 2** | 8. Evaluador XML | 6 | | | | 6 |
| **Fase 3** | 9. Recepción de Liquidaciones (API) | 4 | | | | 4 |
| **Fase 4** | 10. Generación de Orden de Pago | 4 | | | | 4 |
| **Fase 4** | 11. Aprobación Jefe de Sede | 5 | | | | 5 |
| **Fase 4** | 12. Aprobación Gerente Corporativo | 5 | | | | 5 |
| **Fase 5** | 13. Ejecución del Pago | 4 | | | | 4 |
| **Transversal** | 14. Control de Acceso | 3 | | | | 3 |
| | **TOTAL** | **96** | | | | **96** |

---

*Documento generado: 2026-03-31*
