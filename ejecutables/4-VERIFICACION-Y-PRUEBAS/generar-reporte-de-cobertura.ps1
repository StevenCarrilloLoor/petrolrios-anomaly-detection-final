# =============================================================================
# PetrolRíos — Script de generación de reportes de cobertura de código
# Uso: .\scripts\coverage.ps1
# Prerrequisitos: dotnet-reportgenerator-globaltool
#   dotnet tool install -g dotnet-reportgenerator-globaltool
# =============================================================================

$ErrorActionPreference = "Stop"

$SolutionRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$CoverageDir = Join-Path $SolutionRoot "coverage-results"
$ReportDir = Join-Path $SolutionRoot "coverage-report"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " PetrolRios - Generacion de cobertura" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Limpiar resultados anteriores
if (Test-Path $CoverageDir) { Remove-Item $CoverageDir -Recurse -Force }
if (Test-Path $ReportDir) { Remove-Item $ReportDir -Recurse -Force }
New-Item -ItemType Directory -Path $CoverageDir -Force | Out-Null

Write-Host ""
Write-Host ">> Ejecutando pruebas con cobertura..." -ForegroundColor Yellow
Write-Host ""

$SolutionPath = Join-Path $SolutionRoot "PetrolRios.sln"

dotnet test $SolutionPath `
    --configuration Release `
    --collect:"XPlat Code Coverage" `
    --results-directory $CoverageDir `
    --logger "console;verbosity=minimal" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura `
       DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*.Tests]*,[*]PetrolRios.Infrastructure.Migrations.*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Las pruebas fallaron." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host ">> Generando reporte HTML..." -ForegroundColor Yellow
Write-Host ""

# Buscar archivos de cobertura generados
$CoverageFiles = Get-ChildItem -Path $CoverageDir -Recurse -Filter "coverage.cobertura.xml" | ForEach-Object { $_.FullName }

if ($CoverageFiles.Count -eq 0) {
    Write-Host "ERROR: No se encontraron archivos de cobertura." -ForegroundColor Red
    exit 1
}

$CoverageFilesStr = $CoverageFiles -join ";"

reportgenerator `
    "-reports:$CoverageFilesStr" `
    "-targetdir:$ReportDir" `
    "-reporttypes:Html;TextSummary;Badges" `
    "-assemblyfilters:+PetrolRios.Domain;+PetrolRios.Application;+PetrolRios.Infrastructure;+PetrolRios.Detectors;+PetrolRios.Api" `
    "-classfilters:-PetrolRios.Infrastructure.Migrations.*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: La generacion del reporte fallo." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " Reporte generado en: $ReportDir\index.html" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Mostrar resumen
$SummaryFile = Join-Path $ReportDir "Summary.txt"
if (Test-Path $SummaryFile) {
    Get-Content $SummaryFile
}

# Abrir en el navegador
$IndexFile = Join-Path $ReportDir "index.html"
if (Test-Path $IndexFile) {
    Write-Host ""
    Write-Host "Abriendo reporte en el navegador..." -ForegroundColor Cyan
    Start-Process $IndexFile
}
