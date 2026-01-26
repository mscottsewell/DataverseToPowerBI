@echo off
REM Launch the Dataverse Metadata Extractor UI
cd /d "%~dp0"

REM Activate virtual environment if it exists
if exist ".venv\Scripts\activate.bat" (
    call .venv\Scripts\activate.bat
)

REM Run the UI application
python Code\dataverse_metadata_ui.py

pause
