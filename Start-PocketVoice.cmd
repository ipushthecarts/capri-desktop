@echo off
setlocal
cd /d "%~dp0"
if not exist ".voice-venv\Scripts\python.exe" (echo Run Setup-PocketVoice.cmd first. & pause & exit /b 1)
".voice-venv\Scripts\python.exe" local-voice\server.py %*
pause
