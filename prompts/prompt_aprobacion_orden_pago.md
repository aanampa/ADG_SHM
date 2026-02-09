# PROMPT – Desarrollo de Pantalla de Aprobación de Órdenes de Pago

## Objetivo general
Desarrollar una pantalla de bandeja de aprobación de Órdenes de Pago y una pantalla de detalle, que permita a los usuarios con perfil Sede o Corporativo visualizar, aprobar o rechazar las órdenes de pago pendientes, utilizando el modelo de datos existente.

## Contexto funcional
- Una Orden de Pago pasa por un flujo de aprobación secuencial:
  1. Aprobación Sede
  2. Aprobación Corporativa
- Puede haber múltiples usuarios por nivel, pero solo uno debe aprobar.
- El usuario no está preasignado; se registra al aprobar.
- El control de aprobación se basa en:
  - SHM_PERFIL_APROBACION
  - SHM_PERFIL_APROBACION_USUARIO

## Modelo de datos
Tablas a utilizar:
- SHM_ORDEN_PAGO
- SHM_ORDEN_PAGO_APROBACION
- SHM_PERFIL_APROBACION
- SHM_PERFIL_APROBACION_USUARIO
- SHM_USUARIO
- SHM_SEDE

## Pantalla 1: Bandeja de Órdenes Pendientes
Reglas:
- Mostrar órdenes con nivel en estado PENDIENTE.
- El usuario debe tener el perfil correspondiente.
- Para Sede, la sede debe coincidir.
- Para Corporativo, puede ver todas.

La pantalla es similar a OrdenPago/Index
Campos:
- Botón Detalle
- Número Orden de Pago	
- Fecha Generación	
- Banco
- Cant. Liquid.	
- Cant. Comprob.	
- Estado	
- Sub Total S/.	
- IGV S/.	
- Renta S/.	
- Total S/.


## Pantalla 2: Detalle y Aprobación
Mostrar:
- Detalle de Orden de Pago (similar a OrdenPago/Detalle)
- Flujo de aprobación

Acciones:
- Aprobar: actualiza nivel, registra usuario y fecha.
- Rechazar: requiere comentario y bloquea flujo.

## Consideraciones
- No permitir doble aprobación.
- Validar concurrencia.
- Acciones transaccionales.

## Controlador
Implementarlo en el Controladro OrdenPagoAprobacionController
