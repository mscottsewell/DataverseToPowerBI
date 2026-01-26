# Launch the Dataverse Metadata Extractor UI
Set-Location $PSScriptRoot

# Activate virtual environment if it exists
if (Test-Path ".venv\Scripts\Activate.ps1") {
    & ".venv\Scripts\Activate.ps1"
}

# Run the UI application
python Code\dataverse_metadata_ui.py
