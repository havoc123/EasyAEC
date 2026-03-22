param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere)) {
    Write-Warning "[AEC_Core] vswhere.exe not found. Install Visual Studio (Desktop development with C++) or build AEC_Core from EasyAEC.sln in VS."
    exit 0
}

$raw = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null
$msbExe = $null
if ($raw -is [System.Array]) {
    foreach ($line in $raw) {
        $t = [string]$line
        if (-not [string]::IsNullOrWhiteSpace($t) -and (Test-Path -LiteralPath $t.Trim())) {
            $msbExe = $t.Trim()
            break
        }
    }
}
else {
    $t = [string]$raw
    if (-not [string]::IsNullOrWhiteSpace($t) -and (Test-Path -LiteralPath $t.Trim())) {
        $msbExe = $t.Trim()
    }
}

if ($null -eq $msbExe) {
    Write-Warning "[AEC_Core] MSBuild.exe not found via vswhere. Install Visual Studio Build Tools / full VS with MSBuild."
    exit 0
}

$msbExe = [string]$msbExe
if ([string]::IsNullOrWhiteSpace($msbExe)) {
    Write-Warning "[AEC_Core] MSBuild.exe path empty. Install Visual Studio Build Tools / full VS with MSBuild."
    exit 0
}

$aec3Sln = Join-Path $root "third_party\AEC3\AEC3.sln"
if (Test-Path -LiteralPath $aec3Sln) {
    Write-Host "[AEC_Core] Building third_party/AEC3 (AEC3.lib, api.lib, base.lib) ..."
    & $msbExe $aec3Sln `
        "/p:Configuration=$Configuration" `
        "/p:Platform=x64" `
        "/p:WindowsTargetPlatformVersion=10.0" `
        "/p:PlatformToolset=v143" `
        "/v:minimal" `
        "/nologo" `
        "/m"
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
else {
    Write-Warning "[AEC_Core] Missing $aec3Sln — run: git clone https://github.com/ewan-xu/AEC3.git third_party/AEC3"
}

$proj = Join-Path $root "AEC_Core\AEC_Core.vcxproj"
if (-not (Test-Path -LiteralPath $proj)) {
    Write-Error "[AEC_Core] Project not found: $proj"
    exit 1
}

Write-Host "[AEC_Core] Building AEC_Core.dll ..."
& $msbExe $proj "/p:Configuration=$Configuration" "/p:Platform=x64" "/v:minimal" "/nologo"
exit $LASTEXITCODE
