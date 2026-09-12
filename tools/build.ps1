[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Ymm4Dir,
    [switch]$SkipWindowsTests
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$Ymm4Dir = (Resolve-Path $Ymm4Dir).Path
if (!(Test-Path "$Ymm4Dir/YukkuriMovieMaker.Plugin.dll")) { throw 'Ymm4Dir must point to YMM4 v4.47+ (.NET 10).' }
Push-Location $root
try {
    $env:YMM4DirPath = if ($Ymm4Dir.EndsWith('\') -or $Ymm4Dir.EndsWith('/')) { $Ymm4Dir } else { "$Ymm4Dir\" }
    dotnet build .\YMM4.ExtendedEffectsPack\YMM4.ExtendedEffectsPack.csproj -c Release --warnaserror -p:SkipPluginDeploy=true
    if ($LASTEXITCODE) { throw 'Build failed' }
    python -m unittest discover -s tests -p "test_*.py" -v
    if ($LASTEXITCODE) { throw 'Structural tests failed' }
    $out = Join-Path $root 'artifacts'
    New-Item $out -ItemType Directory -Force | Out-Null
    $dll = Join-Path $root 'YMM4.ExtendedEffectsPack\bin\Release\net10.0-windows10.0.19041.0\YMM4.ExtendedEffectsPack.dll'
    $zip = Join-Path $out 'YMM4.ExtendedEffectsPack.zip'
    $package = Join-Path $out 'YMM4.ExtendedEffectsPack.ymme'
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path $dll -DestinationPath $zip -Force
    if (Test-Path $package) { Remove-Item $package -Force }
    Move-Item $zip $package -Force
    Copy-Item $dll $out -Force
    Get-FileHash $package -Algorithm SHA256
    Write-Host "Package: $package"
    Write-Host 'YMM4 editor integration and visual QA are still required.'
}
finally { Pop-Location }
