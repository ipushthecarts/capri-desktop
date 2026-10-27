@echo off
setlocal
cd /d "%~dp0"
py -3.12 --version >nul 2>&1
if errorlevel 1 (echo Install Python 3.12 for Windows, including the Python launcher, then run this again. & pause & exit /b 1)
if not exist ".voice-venv\Scripts\python.exe" py -3.12 -m venv .voice-venv
if errorlevel 1 goto failed
".voice-venv\Scripts\python.exe" -m pip install --upgrade pip
if errorlevel 1 goto failed
".voice-venv\Scripts\python.exe" -m pip install torch --index-url https://download.pytorch.org/whl/cpu
if errorlevel 1 goto failed
".voice-venv\Scripts\python.exe" -m pip install -r local-voice\requirements.txt
if errorlevel 1 goto failed
echo Ready. Run Start-PocketVoice.cmd, then choose Pocket TTS in Capri's local voice settings.
pause
exit /b 0
:failed
echo Setup failed. Read the error above. Windows voices still work without Python.
pause
exit /b 1
