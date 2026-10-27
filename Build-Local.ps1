$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) { throw 'Install .NET Framework 4.8 (Windows 10/11 normally includes it).' }
New-Item -ItemType Directory -Force dist | Out-Null
$references = @('System.dll','System.Core.dll','System.Xml.dll','Microsoft.CSharp.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Runtime.Serialization.dll','System.Security.dll','System.Speech.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll','UIAutomationClient.dll','UIAutomationTypes.dll','WindowsBase.dll')
$argsList = @('/nologo',('/lib:' + (Join-Path (Split-Path $csc) 'WPF')),'/target:winexe','/platform:x64','/optimize+','/define:LOCAL_BUILD','/main:CapriDesktop.Program','/out:dist\Capri-Local.exe','/win32icon:assets\chip.ico','/win32manifest:app\app.manifest')
foreach ($reference in $references) { $argsList += "/reference:$reference" }
foreach ($file in (Get-ChildItem app\*.cs)) { $argsList += $file.FullName }
foreach ($file in (Get-ChildItem assets\sprites\*.png)) { $argsList += "/resource:$($file.FullName),Capri.Sprite.$($file.Name)" }
$argsList += '/resource:assets\fonts\ShareTechMono-Regular.ttf,Capri.Font.Mono.ttf'
$argsList += '/resource:assets\chip.ico,Capri.Chip.ico'
$argsList += '/resource:assets\chip.png,Capri.Chip.png'
& $csc @argsList
if ($LASTEXITCODE -ne 0) { throw "C# compiler returned $LASTEXITCODE" }
Copy-Item Start-PocketVoice.cmd,Setup-PocketVoice.cmd -Destination dist -Force
Copy-Item local-voice -Destination dist -Recurse -Force
Copy-Item LOCAL-VOICE.md,LICENSE.md,README.md -Destination dist -Force
Copy-Item assets\fonts\OFL-ShareTechMono.txt -Destination dist -Force
Write-Host 'Ready: dist\Capri-Local.exe'
