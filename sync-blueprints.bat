@echo off
setlocal

set "LOCAL_BLUEPRINTS=%~dp0Blueprints"
set "APPDATA_BLUEPRINTS=%AppData%\SpaceEngineers\Blueprints\local"

:A
cls

echo Space Engineers Blueprint Sync
echo.
echo Choose an option:
echo   1. Copy from .\Blueprints\ (repo) to AppData (local game files)
echo   2. Copy from AppData (local game files) to .\Blueprints\ (repo)
echo   3. Exit
echo.

set /p "CHOICE="

if "%CHOICE%"=="1" (
    set "SOURCE=%LOCAL_BLUEPRINTS%"
    set "DEST=%APPDATA_BLUEPRINTS%"
    goto :copy
)

if "%CHOICE%"=="2" (
    set "SOURCE=%APPDATA_BLUEPRINTS%"
    set "DEST=%LOCAL_BLUEPRINTS%"
    goto :copy
)

if "%CHOICE%"=="3" (
    exit \b 0
)

echo Invalid choice.
pause
GOTO A
exit /b 1

:copy
if not exist "%SOURCE%\" (
    echo Source folder not found:
    echo   "%SOURCE%"
    exit /b 1
)

if not exist "%DEST%\" (
    mkdir "%DEST%"
)

echo.
echo Copying blueprint folders from:
echo   "%SOURCE%"
echo to:
echo   "%DEST%"
echo.

for /d %%D in ("%SOURCE%\*") do (
    echo Copying "%%~nxD"...
    robocopy "%%D" "%DEST%\%%~nxD" /E
)

echo.
echo Done.
pause
exit /b 0