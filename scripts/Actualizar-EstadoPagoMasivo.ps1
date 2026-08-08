<#
.SYNOPSIS
    Invoca el endpoint del API SHM que actualiza el estado de pago de los
    comprobantes de ordenes de pago aprobadas (POST /api/sapinterface/actualizar-estado-pago-masivo).

.DESCRIPTION
    Pensado para ser ejecutado por el Task Scheduler de Windows de forma diaria.
    Registra el resultado (o cualquier error) en un archivo de log con rotacion diaria.

.PARAMETER ApiUrl
    URL completa del endpoint. Por defecto apunta al API REST en el servidor de
    aplicaciones (ver docs/despliegue_solucion.md - SHM API REST, puerto 92).

.PARAMETER LogFolder
    Carpeta donde se escriben los logs. Se crea un archivo por dia.

.AUTHOR
    ADG Antonio

.CREATED
    2026-07-18
#>

param(
    [string]$ApiUrl    = "https://192.1.0.173:92/api/sapinterface/actualizar-estado-pago-masivo",
    [string]$LogFolder = "D:\appweb\shmappapi\logs"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $LogFolder)) {
    New-Item -ItemType Directory -Path $LogFolder -Force | Out-Null
}

$logFile = Join-Path $LogFolder ("actualizar-estado-pago-masivo_{0}.log" -f (Get-Date -Format "yyyyMMdd"))

function Write-Log {
    param([string]$Mensaje)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $Mensaje" | Out-File -FilePath $logFile -Append -Encoding utf8
}

Write-Log "Inicio de actualizacion masiva de estado de pago. URL: $ApiUrl"

try {
    $response = Invoke-RestMethod -Uri $ApiUrl -Method Post -TimeoutSec 900

    if ($response.isSuccess) {
        $d = $response.data
        Write-Log ("OK - Ordenes: {0} | Comprobantes: {1} | ConsultadosEnSap: {2} | Actualizados: {3} | SinComprobante: {4} | Errores: {5}" -f `
            $d.totalOrdenes, $d.totalComprobantes, $d.consultadosEnSap, $d.actualizados, $d.sinComprobante, $d.errores)
    }
    else {
        Write-Log ("RESPUESTA_NO_EXITOSA - {0}" -f $response.message)
        exit 1
    }
}
catch {
    Write-Log ("EXCEPCION - {0}" -f $_.Exception.Message)
    exit 1
}

Write-Log "Fin del proceso"
exit 0
