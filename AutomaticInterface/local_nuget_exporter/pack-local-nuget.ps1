param(
    [string]$Project = "..\DotnetAutomaticInterface\DotnetAutomaticInterface.csproj",
    [string]$Configuration = "Release",
    [string]$Output = "local_nuget_feed"
)

Set-StrictMode -Version Latest

Write-Host "Project: $Project"
Write-Host "Configuration: $Configuration"
Write-Host "Output feed root: $Output"

if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
New-Item -ItemType Directory -Path $Output | Out-Null

# Create a temporary pack output folder
$tempPack = Join-Path $env:TEMP "pack-temp-$(Get-Random)"
Write-Host "Temp pack folder: $tempPack"

if (Test-Path $tempPack) { Remove-Item $tempPack -Recurse -Force }
New-Item -ItemType Directory -Path $tempPack | Out-Null

Write-Host "Packing project (output: $tempPack)..."
$packCmd = "dotnet pack `"$Project`" -c $Configuration -o `"$tempPack`""
Invoke-Expression $packCmd
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet pack failed"; Remove-Item $tempPack -Recurse -Force; Exit $LASTEXITCODE }

# Find produced nupkg
$nupkg = Get-ChildItem -Path $tempPack -Filter "*.nupkg" -File | Select-Object -First 1
if (-not $nupkg) { Write-Error "No .nupkg produced in $tempPack"; Remove-Item $tempPack -Recurse -Force; Exit 1 }

# Read csproj to determine Version; use project filename as PackageId
[xml]$xml = Get-Content -Path $Project

$versionNode = $xml.SelectSingleNode('//Version') # read from csproj
if ($versionNode -and $versionNode.InnerText.Trim()) {
    $version = $versionNode.InnerText.Trim()
}
else {
    $version = '0.0.0'
}
$packageId = [System.IO.Path]::GetFileNameWithoutExtension($Project)

Write-Host "Detected PackageId=$packageId  Version=$version"

# Create hierarchical feed folder (feed root)
$nugetCmd = Get-Command nuget -ErrorAction SilentlyContinue 
if (-not $nugetCmd) {   
    Write-Host 'nuget not found � installing via winget (may prompt for elevation)...'
    Invoke-Expression "winget install Microsoft.NuGet"
}

$exportCmd = "nuget add $nupkg -Source $Output"
Write-Host "Running $exportCmd"

Invoke-Expression $exportCmd

# Clean up temp
Remove-Item $tempPack -Recurse -Force -ErrorAction SilentlyContinue

Exit 0
