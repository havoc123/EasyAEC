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

$proj = Join-Path $root "AEC_Core\AEC_Core.vcxproj"
if (-not (Test-Path -LiteralPath $proj)) {
    Write-Error "[AEC_Core] Project not found: $proj"
    exit 1
}

& $msbExe $proj "/p:Configuration=$Configuration" "/p:Platform=x64" "/v:minimal" "/nologo"
exit $LASTEXITCODE
