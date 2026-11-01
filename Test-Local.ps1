$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
& ./ci/build-windows.ps1 -OutputDirectory out/windows-test -SmokeTest -SmokeSeconds 600
