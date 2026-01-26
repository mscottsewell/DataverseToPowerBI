@echo off
echo Building Dataverse Metadata Extractor...
cd /d "%~dp0DataverseMetadataExtractor"
dotnet build --configuration Release
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Starting application...
start "" "bin\Release\net8.0-windows\DataverseMetadataExtractor.exe"
