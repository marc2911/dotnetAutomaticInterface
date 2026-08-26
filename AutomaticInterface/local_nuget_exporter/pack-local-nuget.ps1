param(
    [string]$Project = "../DotnetAutomaticInterface\DotnetAutomaticInterface.csproj",
    [string]$Configuration = "Release",
    [string]$Output = "./local_nuget_feed"
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

# Read csproj to determine Version
[xml]$xml = Get-Content -Path $Project
$versionNode = $xml.SelectSingleNode('//Version') # read from csproj
$version = $versionNode.InnerText.Trim()
Write-Host "Detected Package Version=$version"

$version = "$version-local"

Write-Host "Building project in Release first..."
dotnet build $Project -c $Configuration -p:GeneratePackageOnBuild=false -p:Version=$version

Write-Host "Packing project (output: $tempPack) with local version $version..."
dotnet pack $Project -c $Configuration -o $tempPack --no-build -p:Version=$version
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet pack failed"; Remove-Item $tempPack -Recurse -Force; Exit $LASTEXITCODE }

# Create hierarchical feed folder (feed root)
$nugetCmd = Get-Command nuget -ErrorAction SilentlyContinue 
if (-not $nugetCmd) {   
    Write-Host 'nuget not found � installing via winget (may prompt for elevation)...'
    winget install Microsoft.NuGet

    if ($LASTEXITCODE -ne 0) {
        throw "Nuget installation failed..."
    }
    # Reload PATH for this PowerShell process
    $env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
    [Environment]::GetEnvironmentVariable("Path", "User")
}

# Find produced nupkg
$nupkg = Get-ChildItem -Path $tempPack -Filter "*.nupkg" -File | Select-Object -First 1
if (-not $nupkg) { Write-Error "No .nupkg produced in $tempPack"; Remove-Item $tempPack -Recurse -Force; Exit 1 }

$exportCmd = "nuget add $nupkg -Source $Output"
Write-Host "Running $exportCmd"

Invoke-Expression $exportCmd

# Clean up temp
Remove-Item $tempPack -Recurse -Force -ErrorAction SilentlyContinue

Exit 0
