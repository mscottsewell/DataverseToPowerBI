# Code Scripts

## 🚀 NEW: Direct Dataverse Extraction

**Extract metadata directly from Dataverse without Excel!**

See [README-DirectExtraction.md](README-DirectExtraction.md) for complete documentation.

### Quick Start

```bash
# Install dependencies
pip install -r requirements.txt

# Test connection and list solutions
python test_dataverse_connection.py https://yourorg.crm.dynamics.com

# Extract metadata
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/MyProject/Metadata"
```

**Benefits:**
- ✓ Automated & repeatable
- ✓ Always current metadata
- ✓ No manual Excel export
- ✓ CI/CD ready
- ✓ Git-friendly JSON output

---

## Excel-Based Scripts (Legacy)

### extract_fields.py

Python utility to display fields from the Excel metadata.

**Usage:**
```powershell
python Code\extract_fields.py Reports\{ProjectName}\Metadata
```

**Example:**
```powershell
python Code\extract_fields.py "Reports\Dynamics 365 Sales\Metadata"
```

### extract_tables.py

Python utility to display tables from the Excel metadata.

**Usage:**
```powershell
python Code\extract_tables.py Reports\{ProjectName}\Metadata
```

**Example:**
```powershell
python Code\extract_tables.py "Reports\Dynamics 365 Sales\Metadata"
```

**Note:** A metadata folder path must be provided as an argument to specify which project to read from.

## Archive Folder

Contains 10 superseded scripts that were used during initial table setup:
- Scripts for generating SQL and Power Query
- Scripts for batch table operations via MCP
- Table update and rebuild scripts

These have been archived as the tables are now directly managed through TMDL files in the Reports folder.
