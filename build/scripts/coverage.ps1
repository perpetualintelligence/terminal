#
#   Copyright (c) 2019-2015 Perpetual Intelligence L.L.C. All Rights Reserved.
#
#   For license, terms, and data policies, go to:
#   https://terms.perpetualintelligence.com/articles/intro.html
#

# Define colors for the output
$ONEIMLX_COLOR_MAGENTA = "Magenta"
$ONEIMLX_COLOR_GREEN = "Green"
$ONEIMLX_COLOR_DEFAULT = "White"

# Define the path to the OneImlx.Terminal solution file
$ONEIMLX_SOLUTION_PATH = "./OneImlx.Terminal.Solution.sln"  # Specify the solution or test project path

# Define paths for coverage reports and outputs
$ONEIMLX_COVERAGE_DIRECTORY = "./coverage"
$ONEIMLX_HTML_REPORT_DIRECTORY = "$ONEIMLX_COVERAGE_DIRECTORY/report"
$ONEIMLX_HTML_REPORT_INDEX = "$ONEIMLX_HTML_REPORT_DIRECTORY/index.html"
$ONEIMLX_COBERTURA_PATTERN = "$ONEIMLX_COVERAGE_DIRECTORY/**/*.cobertura.xml"

# Clean up any existing coverage reports
Write-Host "[INFO] Running OneImlx.Terminal code coverage reports..." -ForegroundColor $ONEIMLX_COLOR_MAGENTA
Remove-Item -Recurse -Force $ONEIMLX_COVERAGE_DIRECTORY -ErrorAction SilentlyContinue

# Execute tests and collect coverage data
Write-Host "[INFO] Build and test $ONEIMLX_SOLUTION_PATH..." -ForegroundColor $ONEIMLX_COLOR_MAGENTA
dotnet test $ONEIMLX_SOLUTION_PATH `
    --collect:"XPlat Code Coverage" `
    --results-directory $ONEIMLX_COVERAGE_DIRECTORY `
    -p:CollectCoverage=true `
    -p:CoverletOutputFormat=cobertura `
    -p:CoverletOutput=$ONEIMLX_COVERAGE_DIRECTORY/coverage.`

# Validate the generation of Cobertura files
$ONEIMLX_COBERTURA_FILES = Get-ChildItem -Path $ONEIMLX_COVERAGE_DIRECTORY -Recurse -Filter "*.cobertura.xml"
if (-Not $ONEIMLX_COBERTURA_FILES) {
    Write-Host "[ERROR] No Cobertura code coverage reports were generated for the OneImlx.Terminal framework." -ForegroundColor Red
    exit 1
}

# Generate a merged HTML report using reportgenerator
if (Get-Command reportgenerator -ErrorAction SilentlyContinue) {
    Write-Host "[INFO] Merging coverage data and creating an interactive HTML report..." -ForegroundColor $ONEIMLX_COLOR_MAGENTA
    reportgenerator `
        -reports:$ONEIMLX_COBERTURA_PATTERN `
        -targetdir:$ONEIMLX_HTML_REPORT_DIRECTORY `
        -reporttypes:Html `
        -title:"OneImlx.Terminal Coverage Report"

    # Verify and open the HTML report
    if (Test-Path $ONEIMLX_HTML_REPORT_INDEX) {
        Write-Host "[INFO] Opening the HTML report in your default web browser..." -ForegroundColor $ONEIMLX_COLOR_MAGENTA
        Start-Process $ONEIMLX_HTML_REPORT_INDEX
    } else {
        Write-Host "[ERROR] HTML report generation encountered an issue." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[ERROR] The required 'reportgenerator' tool is missing. Please install it using:" -ForegroundColor Red
    Write-Host "       dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Reports are available in the $ONEIMLX_COVERAGE_DIRECTORY directory." -ForegroundColor $ONEIMLX_COLOR_MAGENTA
Write-Host "[INFO] OneImlx.Terminal framework coverage successfully completed." -ForegroundColor $ONEIMLX_COLOR_GREEN
