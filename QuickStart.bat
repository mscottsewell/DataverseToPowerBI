@echo off
REM Quick start script for Dataverse metadata extraction

echo ============================================================
echo Dataverse Metadata Extraction - Quick Start
echo ============================================================
echo.

REM Check if Python is available
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher from python.org
    pause
    exit /b 1
)

echo Step 1: Installing required packages...
echo --------------------------------------------------------
cd Code
pip install -r requirements.txt
if errorlevel 1 (
    echo ERROR: Failed to install packages
    pause
    exit /b 1
)
echo.

echo Step 2: Enter your Dataverse environment details
echo --------------------------------------------------------
set /p ENV_URL="Enter your Dataverse URL (e.g., https://yourorg.crm.dynamics.com): "
echo.

echo Step 3: Testing connection and listing solutions...
echo --------------------------------------------------------
python test_dataverse_connection.py "%ENV_URL%"
if errorlevel 1 (
    echo ERROR: Connection test failed
    pause
    exit /b 1
)
echo.

echo Step 4: Extract metadata
echo --------------------------------------------------------
set /p SOLUTION_NAME="Enter the solution unique name from the list above: "
set /p PROJECT_NAME="Enter project name (folder will be created in Reports/): "
echo.

echo Extracting metadata...
python extract_metadata_from_dataverse.py "%ENV_URL%" "%SOLUTION_NAME%" "../Reports/%PROJECT_NAME%/Metadata"
if errorlevel 1 (
    echo ERROR: Metadata extraction failed
    pause
    exit /b 1
)

echo.
echo ============================================================
echo SUCCESS! Metadata has been extracted
echo ============================================================
echo Output location: Reports\%PROJECT_NAME%\Metadata
echo.
pause
