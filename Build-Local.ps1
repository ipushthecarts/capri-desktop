param([switch]$SmokeTest)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
& ./ci/build-windows.ps1 -OutputDirectory dist -SmokeTest:$SmokeTest -SmokeSeconds 900
Copy-Item dist/Capri.exe dist/Capri-Local.exe -Force
Copy-Item dist/Capri-Setup.exe dist/Capri-Local-Setup.exe -Force
Copy-Item Start-PocketVoice.cmd,Setup-PocketVoice.cmd,PACKAGES.md,README.md,LOCAL-VOICE.md,LICENSE.md -Destination dist -Force
Copy-Item local-voice -Destination dist -Recurse -Force
Copy-Item assets/fonts/OFL-ShareTechMono.txt -Destination dist -Force
