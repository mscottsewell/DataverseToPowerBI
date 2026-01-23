# What's New - Direct Dataverse Extraction

## 🎉 Major Update: Bypass Excel Completely

You can now extract metadata **directly from Dataverse** without needing Excel files or XrmToolBox!

## New Files Added

### Core Extraction
- **`Code/extract_metadata_from_dataverse.py`** - Main extraction script
  - Authenticates to Dataverse using OAuth
  - Queries solution components
  - Extracts fields from all main forms
  - Generates JSON output

### Helper Tools
- **`Code/test_dataverse_connection.py`** - Test connection & list solutions
- **`Code/preview_metadata_extraction.py`** - Preview extraction before running
- **`QuickStart.ps1`** - One-click PowerShell setup
- **`QuickStart.bat`** - One-click batch file setup

### Dependencies
- **`Code/requirements.txt`** - Python package requirements
  - requests (HTTP client)
  - msal (Microsoft authentication)
  - pandas (data handling)
  - openpyxl (Excel compatibility)

### Documentation
- **`Code/README-DirectExtraction.md`** - Complete usage guide
- **`WORKFLOW-GUIDE.md`** - End-to-end workflow
- **`SUMMARY.md`** - Quick overview & summary

### Updated Documentation
- **`README.md`** - Added Quick Start section
- **`Code/README.md`** - Promoted new method

## How To Use

### Quick Start (3 commands)

```bash
# 1. Install
pip install -r Code/requirements.txt

# 2. Test & find your solution
python Code/test_dataverse_connection.py https://yourorg.crm.dynamics.com

# 3. Extract
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com YourSolution "Reports/MyProject/Metadata"
```

### Or Use Interactive Scripts

**Windows:**
```powershell
.\QuickStart.ps1
```

**Or:**
```cmd
QuickStart.bat
```

## What It Does

Given just an **environment URL** and **solution name**, the script:

1. ✓ Authenticates you via browser (OAuth)
2. ✓ Finds all tables in the solution
3. ✓ Discovers all main forms per table
4. ✓ Parses form XML to extract field lists
5. ✓ Retrieves full metadata for each field
6. ✓ Generates structured JSON output

## Benefits

| Old Method (Excel) | New Method (Direct) |
|-------------------|---------------------|
| ❌ Manual export | ✅ Fully automated |
| ❌ Can be outdated | ✅ Always current |
| ❌ Requires XrmToolBox | ✅ Just Python |
| ❌ Select forms manually | ✅ All forms automatically |
| ❌ Human error prone | ✅ Consistent |
| ❌ Hard to version | ✅ Git-friendly JSON |
| ❌ Can't automate | ✅ CI/CD ready |

## Output Format

Same JSON structure as before, ensuring compatibility with existing tools:

```json
{
  "Environment": "https://yourorg.crm.dynamics.com",
  "Solution": "MySolution",
  "Tables": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "SchemaName": "Account",
      "Forms": [...],
      "Attributes": [
        {
          "LogicalName": "name",
          "SchemaName": "Name",
          "DisplayName": "Account Name",
          "AttributeType": "String",
          "IsCustom": false
        }
      ]
    }
  ]
}
```

## Compatibility

- ✅ Works with existing PowerShell scripts
- ✅ Same JSON format as Excel method
- ✅ Compatible with Power BI semantic model generation
- ✅ Can mix old and new methods

## Requirements

- **Python 3.8+**
- **Dataverse access** (read permissions)
- **Internet connection** (for authentication)

## Migration Path

### If You're Using Excel Method Now

**You can:**
1. Keep using Excel method (still supported)
2. Try new method alongside Excel
3. Gradually migrate projects to new method
4. Switch completely (recommended)

**Your existing files are safe:**
- Old Excel files still work
- Existing JSON files remain valid
- No breaking changes

### Recommended Approach

For existing projects:
```bash
# Keep Excel file as backup
# Generate new JSON using direct extraction
python Code/extract_metadata_from_dataverse.py <url> <solution> "Reports/ExistingProject/Metadata"

# Compare outputs
git diff Reports/ExistingProject/Metadata/*.json

# If satisfied, can remove Excel file
```

## Troubleshooting

See [Code/README-DirectExtraction.md](Code/README-DirectExtraction.md) for:
- Authentication issues
- Connection problems
- Missing forms/fields
- Performance optimization

## Future Enhancements

Possible additions:
- Relationship metadata
- Option set values
- Security roles
- Comparison tools
- Service principal auth for CI/CD
- Direct Power BI file generation

## Getting Help

- **Usage Guide**: [Code/README-DirectExtraction.md](Code/README-DirectExtraction.md)
- **Complete Workflow**: [WORKFLOW-GUIDE.md](WORKFLOW-GUIDE.md)
- **Quick Overview**: [SUMMARY.md](SUMMARY.md)
- **Original README**: [README.md](README.md)

## Questions?

Common questions answered in documentation:

**Q: Do I need XrmToolBox anymore?**  
A: No, for basic metadata extraction. Keep it for other utilities.

**Q: What about existing Excel files?**  
A: Still supported. Use whichever method you prefer.

**Q: Can I automate this in CI/CD?**  
A: Yes! Service principal auth can be added for pipelines.

**Q: Will it work with my existing Power BI files?**  
A: Yes, same JSON format ensures compatibility.

**Q: How do I find my solution name?**  
A: Run `test_dataverse_connection.py` - it lists all solutions.

---

**Get Started:** Run `QuickStart.ps1` or see [SUMMARY.md](SUMMARY.md)
