@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -File "%~dp0Build-Local.ps1"
if errorlevel 1 (echo Build failed. See the error above. & pause & exit /b 1)
echo Built dist\Capri-Local.exe. Start it to use local Windows speech.
pause
