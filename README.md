# Dataverse Metadata to Power BI Semantic Model Quickstart

## � Documentation

| Document | Purpose |
|----------|---------|
| **[SUMMARY.md](SUMMARY.md)** | Quick overview - "Can you do this?" - **Start here!** |
| **[QUICK-REFERENCE.md](QUICK-REFERENCE.md)** | Command cheat sheet & quick tips |
| **[Code/README-DirectExtraction.md](Code/README-DirectExtraction.md)** | Complete usage guide |
| **[WORKFLOW-GUIDE.md](WORKFLOW-GUIDE.md)** | End-to-end implementation workflow |
| **[VISUAL-GUIDE.md](VISUAL-GUIDE.md)** | Before/after comparison with visuals |
| **[ARCHITECTURE.md](ARCHITECTURE.md)** | Technical architecture & data flow |
| **[WHATS-NEW.md](WHATS-NEW.md)** | What changed in this update |
| **[DOCUMENTATION-INDEX.md](DOCUMENTATION-INDEX.md)** | Complete documentation map |

---

## �🚀 Quick Start: Direct Dataverse Extraction (Recommended)

**NEW:** Skip Excel exports and get metadata directly from Dataverse!

```bash
# 1. Install dependencies
cd Code
pip install -r requirements.txt

# 2. Test connection and find your solution name
python test_dataverse_connection.py https://yourorg.crm.dynamics.com

# 3. Extract metadata
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com YourSolutionName "Reports/MyProject/Metadata"
```

**Why use this method?**
- ✓ **Automated**: No manual exports needed
- ✓ **Current**: Always gets latest metadata
- ✓ **Repeatable**: Run anytime to refresh
- ✓ **CI/CD Ready**: Integrate into pipelines
- ✓ **Complete**: Extracts all forms and fields automatically

📖 **Full documentation:** [Code/README-DirectExtraction.md](Code/README-DirectExtraction.md)

---

## Alternative: Excel-Based Extraction (Legacy)

If you prefer using XrmToolBox and Excel files, follow these steps:

## Project Structure

```
DataverseMetadata-to-PowerBI-Semantic-Model/
├── Reports/                # Power BI Reports with project-specific metadata
│   └── Dynamics 365 Sales/
│       ├── Metadata/       # Excel and JSON metadata files
│       │   ├── DataverseURL.txt
│       │   ├── Dynamics 365 Sales Metadata Dictionary.xlsx
│       │   └── Dynamics 365 Sales Metadata Dictionary.json
│       └── PBIP/           # Power BI Project files
           ├── Dynamics 365 Sales.pbip
           ├── Dynamics 365 Sales.Report/
           └── Dynamics 365 Sales.SemanticModel/
├── Code/                   # Utility scripts
│   ├── extract_fields.py
│   ├── extract_tables.py
│   └── Archive/            # Legacy scripts
├── Extract-PowerBIMetadata.ps1  # Main metadata extraction tool
└── README.md
```

## Extracting Metadata

- **Create a DataverseURL.txt file** in the `Reports/[ProjectName]/Metadata/` folder containing your Dataverse organization URL (e.g., `https://yourorg.crm.dynamics.com`).
- Use the XrmToolBox "Metadata Document Generator" utility to export Dataverse table metadata to an Excel file. 
- Use the 'Load Entities from Solution' option to include only relevant tables.
- Choose the "All Attributes contained in forms", and select a form for each entity you want added to the semantic model in Power BI. (Be sure to select each entity row and select the form for it, one by one.)
- Select the two checkboxes in the lower right corner to create the two summarized tabs.
- **Hint** Use the "Generation settings" option to save a configuration file so that you can return and edit/re-create the file easily if needed.
- Generate the file and save it into the `Reports/[ProjectName]/Metadata/` folder.
- If your organization uses IRM, be sure to set the Excel file's sensitivity label to 'General' to avoid access issues. <img width="800" alt="Metadata Document Generator Screenshot" src="https://github.com/user-attachments/assets/e437ec8c-4513-4db0-bdf2-e6d32a07b802" />

## Main Tool

### Extract-PowerBIMetadata.ps1

Extracts metadata from Excel files into JSON format for Power BI semantic model generation.

**Usage:**

```powershell
# Use default project (Dynamics 365 Sales)
.\Extract-PowerBIMetadata.ps1

# Specify a different project
.\Extract-PowerBIMetadata.ps1 -ProjectName "Dynamics 365 Sales"

# Specify custom Excel file name
.\Extract-PowerBIMetadata.ps1 -ProjectName "Dynamics 365 Sales" -ExcelFileName "CustomMetadata.xlsx"

# Create a new project structure (if project doesn't exist)
.\Extract-PowerBIMetadata.ps1 -ProjectName "NewProject"
```

**Parameters:**
- `ProjectName` (optional): Name of the project folder under Reports/. Default: `Dynamics 365 Sales`
- `ExcelFileName` (optional): Name of Excel file. Default: `{ProjectName} Metadata Dictionary.xlsx`

**Output:**
- Creates a JSON file in Reports/{ProjectName}/Metadata/
- JSON contains all tables and their fields with schema information

**Auto-Create New Projects:**
- If the project folder doesn't exist, the script will create:
  - `Reports/{ProjectName}/Metadata/` folder
  - `Reports/{ProjectName}/PBIP/` folder
  - Prompts you to add Excel file and set sensitivity to 'General'

## Utility Scripts

See [Code/README.md](Code/README.md) for Python utility scripts that display metadata contents.

## Working with Projects

### Adding a New Project

1. Run the extraction script with a new project name:
   ```powershell
   .\Extract-PowerBIMetadata.ps1 -ProjectName "NewProject"
   ```

2. The script will create:
   - `Reports/NewProject/Metadata/` folder
   - `Reports/NewProject/PBIP/` folder

3. Add your Excel metadata file to `Reports/NewProject/Metadata/`
   - Filename should be: `NewProject Metadata Dictionary.xlsx`
   - Set sensitivity label to 'General'

4. Create a `DataverseURL.txt` file in `Reports/NewProject/Metadata/`
   - Add your Dataverse organization URL (e.g., `yourorg.crm.dynamics.com`)
   - This file documents the source environment for the metadata

5. Run the script again to generate JSON:
   ```powershell
   .\Extract-PowerBIMetadata.ps1 -ProjectName "NewProject"
   ```

6. Create your PBIP files in `Reports/NewProject/PBIP/`

### Existing Projects

- **Dynamics 365 Sales**: Sample Dynamics 365 Sales report with common CRM tables
  - Metadata: [Reports/Dynamics 365 Sales/Metadata/](Reports/Dynamics%20365%20Sales/Metadata/)
  - PBIP: [Reports/Dynamics 365 Sales/PBIP/](Reports/Dynamics%20365%20Sales/PBIP/)

