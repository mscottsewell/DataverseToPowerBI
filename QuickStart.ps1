# Quick start script for Dataverse metadata extraction

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Dataverse Metadata Extraction - Quick Start" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Python is available
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✓ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "  Please install Python 3.8 or higher from python.org" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Step 1: Install packages
Write-Host "Step 1: Installing required packages..." -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Gray
Push-Location Code
try {
    pip install -r requirements.txt
    if ($LASTEXITCODE -ne 0) {
        throw "Package installation failed"
    }
    Write-Host "✓ Packages installed successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Failed to install packages" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Step 2: Get environment URL
Write-Host "Step 2: Enter your Dataverse environment details" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Gray
$envUrl = Read-Host "Enter your Dataverse URL (e.g., https://yourorg.crm.dynamics.com)"
Write-Host ""

# Step 3: Test connection
Write-Host "Step 3: Testing connection and listing solutions..." -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Gray
try {
    python test_dataverse_connection.py "$envUrl"
    if ($LASTEXITCODE -ne 0) {
        throw "Connection test failed"
    }
} catch {
    Write-Host "✗ ERROR: Connection test failed" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Step 4: Extract metadata
Write-Host "Step 4: Extract metadata" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Gray
$solutionName = Read-Host "Enter the solution unique name from the list above"
$projectName = Read-Host "Enter project name (folder will be created in Reports/)"
Write-Host ""

Write-Host "Extracting metadata..." -ForegroundColor Yellow
$outputPath = Join-Path ".." "Reports" $projectName "Metadata"
try {
    python extract_metadata_from_dataverse.py "$envUrl" "$solutionName" "$outputPath"
    if ($LASTEXITCODE -ne 0) {
        throw "Metadata extraction failed"
    }
} catch {
    Write-Host "✗ ERROR: Metadata extraction failed" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "SUCCESS! Metadata has been extracted" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Output location: Reports\$projectName\Metadata" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review the generated JSON file" -ForegroundColor Gray
Write-Host "  2. Use it with your Power BI semantic model generation" -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to exit"
