@echo off
setlocal enabledelayedexpansion
:: ============================================================
:: git-vladimir.bat - Sube cambios al repositorio GIT
:: Uso: git-vladimir.bat "descripcion del cambio"
::      git-vladimir.bat  (sin parametro usa "Cambios varios yyyy-mm-dd hh:mm AM/PM")
:: ============================================================

:: Si no se pasa descripcion, generar una con fecha y hora
if "%~1"=="" (
    set _DATE=%date%
    set _TIME=%time%
    set _TIME=!_TIME: =0!

    set DIA=!_DATE:~0,2!
    set MES=!_DATE:~3,2!
    set ANO=!_DATE:~6,4!
    set HH=!_TIME:~0,2!
    set MM=!_TIME:~3,2!

    set /a HORA12=!HH!
    if !HORA12! LSS 12 (set AMPM=AM) else (set AMPM=PM)
    if !HORA12! GTR 12 set /a HORA12=!HORA12!-12
    if !HORA12!==0 set HORA12=12

    set DESCRIPCION=Cambios varios !ANO!-!MES!-!DIA! !HORA12!:!MM! !AMPM!
) else (
    set DESCRIPCION=%~1
)

set BRANCH=vladimir_v1

echo.
echo ============================================================
echo  GIT PUSH - Branch: %BRANCH%
echo  Descripcion: !DESCRIPCION!
echo ============================================================
echo.

echo [1/4] git status...
git status
echo.

echo [2/4] git add ...
git add .
echo.

echo [3/4] git commit...
git commit -m "!DESCRIPCION!"
echo.

echo [4/4] git push...
git push -u origin %BRANCH%
echo.

if %ERRORLEVEL%==0 (
    echo ============================================================
    echo  OK - Cambios subidos exitosamente.
    echo ============================================================
) else (
    echo ============================================================
    echo  ERROR - Revise los mensajes anteriores.
    echo ============================================================
)

echo.
pause
