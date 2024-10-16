#!/bin/bash

# This script runs code coverage on the current solution and generates a report.

# Clean previous coverage reports
echo "[INFO] Cleaning previous coverage reports..."
rm -rf ./coverage

# Run tests with coverage
echo "[INFO] Running tests with code coverage..."
dotnet test --collect:"XPlat Code Coverage" \
    --results-directory ./coverage \
    -p:CollectCoverage=true \
    -p:CoverletOutputFormat=cobertura \
    -p:CoverletOutput=./coverage/coverage.xml \
    -p:Threshold=70

# Check if coverage file exists
if [ ! -f "./coverage/coverage.xml" ]; then
    echo "[ERROR] Code coverage report was not generated."
    exit 1
fi

# Generate an HTML report using reportgenerator (if available)
if command -v reportgenerator &> /dev/null; then
    echo "[INFO] Generating HTML report..."
    reportgenerator -reports:./coverage/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
    echo "[INFO] HTML report generated at ./coverage/report/index.html"
else
    echo "[INFO] reportgenerator tool is not installed. Skipping HTML report generation."
fi

echo "[INFO] Code coverage process completed. Reports are in the ./coverage/ directory."
