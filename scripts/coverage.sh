#!/usr/bin/env bash
# =============================================================================
# PetrolRíos — Script de generación de reportes de cobertura de código
# Uso: ./scripts/coverage.sh
# Prerrequisitos: dotnet-reportgenerator-globaltool
#   dotnet tool install -g dotnet-reportgenerator-globaltool
# =============================================================================

set -euo pipefail

SOLUTION_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COVERAGE_DIR="$SOLUTION_ROOT/coverage-results"
REPORT_DIR="$SOLUTION_ROOT/coverage-report"

echo "============================================="
echo " PetrolRíos — Generación de cobertura"
echo "============================================="

# Limpiar resultados anteriores
rm -rf "$COVERAGE_DIR" "$REPORT_DIR"
mkdir -p "$COVERAGE_DIR"

echo ""
echo "▶ Ejecutando pruebas con cobertura..."
echo ""

dotnet test "$SOLUTION_ROOT/PetrolRios.sln" \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory "$COVERAGE_DIR" \
    --logger "console;verbosity=minimal" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
       DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*.Tests]*,[*]PetrolRios.Infrastructure.Migrations.*"

echo ""
echo "▶ Generando reporte HTML..."
echo ""

# Buscar archivos coverage.cobertura.xml generados
COVERAGE_FILES=$(find "$COVERAGE_DIR" -name "coverage.cobertura.xml" -type f | tr '\n' ';')

if [ -z "$COVERAGE_FILES" ]; then
    echo "❌ No se encontraron archivos de cobertura."
    exit 1
fi

reportgenerator \
    -reports:"$COVERAGE_FILES" \
    -targetdir:"$REPORT_DIR" \
    -reporttypes:"Html;TextSummary;Badges" \
    -assemblyfilters:"+PetrolRios.Domain;+PetrolRios.Application;+PetrolRios.Infrastructure;+PetrolRios.Detectors;+PetrolRios.Api" \
    -classfilters:"-PetrolRios.Infrastructure.Migrations.*"

echo ""
echo "============================================="
echo " ✅ Reporte generado en: $REPORT_DIR/index.html"
echo "============================================="
echo ""

# Mostrar resumen de cobertura
if [ -f "$REPORT_DIR/Summary.txt" ]; then
    cat "$REPORT_DIR/Summary.txt"
fi
