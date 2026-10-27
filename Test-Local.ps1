$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) { throw 'The .NET Framework compiler is missing.' }
New-Item -ItemType Directory -Force dist | Out-Null
& $csc /nologo /target:exe /define:CROSS /out:dist\VoiceTransportTests.exe /reference:System.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Runtime.Serialization.dll tests\VoiceTransportTests.cs app\LocalVoiceTransport.cs app\Voice.cs
if ($LASTEXITCODE -ne 0) { throw 'Transport test compilation failed.' }
& .\dist\VoiceTransportTests.exe
if ($LASTEXITCODE -ne 0) { throw 'Transport tests failed.' }
