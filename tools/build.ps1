[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)][string]$Ymm4Dir = "",
    [switch]$SkipWindowsTests
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# Auto-detect fxc.exe if not in PATH
if (!(Get-Command fxc.exe -ErrorAction SilentlyContinue)) {
    $fxcCandidate = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\fxc.exe", "C:\Program Files\Windows Kits\10\bin\*\x64\fxc.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($fxcCandidate) {
        $env:PATH = "$($fxcCandidate.DirectoryName);$env:PATH"
    }
}

if ([string]::IsNullOrWhiteSpace($Ymm4Dir)) {
    if ($env:YMM4DirPath -and (Test-Path "$env:YMM4DirPath\YukkuriMovieMaker.Plugin.dll")) {
        $Ymm4Dir = $env:YMM4DirPath
    } else {
        foreach ($p in @('Directory.Build.props.user', 'Directory.Build.props')) {
            $propsPath = Join-Path $root $p
            if (Test-Path $propsPath) {
                $xml = [xml](Get-Content $propsPath)
                $node = $xml.Project.PropertyGroup.YMM4DirPath
                $propVal = if ($node -is [string]) { $node.Trim() } elseif ($node.InnerText) { $node.InnerText.Trim() } elseif ($node.'#text') { $node.'#text'.Trim() } else { "$node".Trim() }
                if ($propVal -and (Test-Path "$propVal\YukkuriMovieMaker.Plugin.dll")) {
                    $Ymm4Dir = $propVal
                    break
                }
            }
        }
    }
}
if ([string]::IsNullOrWhiteSpace($Ymm4Dir) -or !(Test-Path "$Ymm4Dir/YukkuriMovieMaker.Plugin.dll")) {
    throw 'Ymm4Dir must point to YMM4 v4.47+ (.NET 10) containing YukkuriMovieMaker.Plugin.dll.'
}

$Ymm4Dir = (Resolve-Path $Ymm4Dir).Path
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
    
    # Create staging folder for package
    $stageDir = Join-Path $out 'package_stage'
    if (Test-Path $stageDir) { Remove-Item $stageDir -Recurse -Force }
    New-Item $stageDir -ItemType Directory -Force | Out-Null

    Copy-Item $dll $stageDir -Force
    $pluginJson = Join-Path $root 'plugin.json'
    if (Test-Path $pluginJson) { Copy-Item $pluginJson $stageDir -Force }
    $readme = Join-Path $root 'README.md'
    if (Test-Path $readme) { Copy-Item $readme $stageDir -Force }
    $license = Join-Path $root 'LICENSE'
    if (Test-Path $license) { Copy-Item $license $stageDir -Force }

    $zip = Join-Path $out 'YMM4.ExtendedEffectsPack.zip'
    $package = Join-Path $out 'YMM4.ExtendedEffectsPack.ymme'
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path "$stageDir\*" -DestinationPath $zip -Force
    if (Test-Path $package) { Remove-Item $package -Force }
    Move-Item $zip $package -Force
    Copy-Item $dll $out -Force
    Remove-Item $stageDir -Recurse -Force

    Get-FileHash $package -Algorithm SHA256
    Write-Host "Package: $package"
    Write-Host 'YMM4 editor integration and visual QA are still required.'
}
finally { Pop-Location }
