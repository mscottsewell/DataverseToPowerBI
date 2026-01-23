# Complete Automated Workflow Guide

This guide explains the complete end-to-end automated workflow for extracting Dataverse metadata and using it with Power BI.

## Overview

The automation eliminates manual steps by:
1. **Authenticating** directly to Dataverse via OAuth
2. **Discovering** tables in your solution automatically
3. **Analyzing** main forms to identify relevant fields
4. **Extracting** complete metadata including display names, schema names, and types
5. **Generating** JSON output compatible with Power BI semantic model tools

## Prerequisites

### Required Software
- **Python 3.8+** ([Download](https://www.python.org/downloads/))
- **Power BI Desktop** (for working with generated models)
- **Git** (optional, for version control)

### Required Access
- **Dataverse Environment Access**: Read permissions on your target environment
- **Solution Access**: The solution must be visible to your user account
- **Microsoft Account**: With MFA and conditional access support

## Step-by-Step Workflow

### Phase 1: Initial Setup (One-time)

#### 1.1 Clone or Download Repository

```bash
git clone https://github.com/yourusername/DataverseMetadata-to-PowerBI-Semantic-Model.git
cd DataverseMetadata-to-PowerBI-Semantic-Model
```

#### 1.2 Install Python Dependencies

```bash
cd Code
pip install -r requirements.txt
```

**Packages installed:**
- `requests` - HTTP client for Dataverse API
- `msal` - Microsoft Authentication Library
- `pandas` - Data manipulation (for Excel fallback)
- `openpyxl` - Excel file handling (for Excel fallback)

### Phase 2: Connect and Discover

#### 2.1 Test Your Connection

```bash
python test_dataverse_connection.py https://yourorg.crm.dynamics.com
```

**This will:**
- Open a browser for authentication
- List all solutions in your environment
- Display solution names and versions
- Verify your access permissions

**Example output:**
```
AVAILABLE SOLUTIONS
================================================================================

Found 5 solution(s):

Display Name                             Unique Name                    Version      Managed
-----------------------------------------------------------------------------------------------
Core AI Platform                         CoreAI                         1.0.0.0      No
Customer Service Hub                     msdyn_CustomerServiceHub       9.1.0.0      Yes
Dynamics 365 Sales                       Sales                          1.0.0.0      No
...
```

#### 2.2 Preview Metadata Extraction

Before doing a full extraction, preview what will be extracted:

```bash
python preview_metadata_extraction.py https://yourorg.crm.dynamics.com CoreAI
```

**This shows:**
- All tables in the solution
- Number of main forms per table
- Tables that might be skipped (no forms)
- Expected output summary

**Example output:**
```
Table Display Name                       Logical Name             Forms
--------------------------------------------------------------------------------
Account                                  account                  3
Contact                                  contact                  2
Opportunity                              opportunity              4
Custom Survey                            new_survey               1
Settings Table                           new_settings             ⚠️  None
...

SUMMARY
================================================================================
Total Tables in Solution: 15
Tables with Forms: 14
Tables without Forms: 1
Total Main Forms: 32
```

### Phase 3: Extract Metadata

#### 3.1 Full Extraction

```bash
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI "Reports/MyProject/Metadata"
```

**What happens:**
1. **Authentication**: Uses cached token from previous step (or prompts for login)
2. **Solution Query**: Retrieves all tables in the solution
3. **Form Discovery**: For each table, finds all main forms
4. **XML Parsing**: Extracts field names from form XML definitions
5. **Metadata Retrieval**: Gets detailed attribute metadata for each field
6. **JSON Generation**: Saves structured metadata to JSON file

**Progress output:**
```
================================================================================
DATAVERSE METADATA EXTRACTION
================================================================================
Environment: https://yourorg.crm.dynamics.com
Solution: CoreAI
================================================================================
Fetching tables from solution: CoreAI...
Found solution: Core AI Platform (12345678-1234-1234-1234-123456789abc)
Found 15 tables in solution
  Processing: Account (account)
  Processing: Contact (contact)
  ...

================================================================================
EXTRACTING FIELDS FROM MAIN FORMS
================================================================================

Account (account):
  Found 3 main form(s)
    - Information: 25 fields
    - Quick Create: 8 fields
    - Mobile: 15 fields
  Total unique fields: 32

Contact (contact):
  Found 2 main form(s)
    - Information: 30 fields
    - Quick Create: 10 fields
  Total unique fields: 35
...
```

#### 3.2 Output Structure

The generated JSON file contains:

```json
{
  "Environment": "https://yourorg.crm.dynamics.com",
  "Solution": "CoreAI",
  "Tables": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "SchemaName": "Account",
      "ObjectTypeCode": 1,
      "PrimaryIdAttribute": "accountid",
      "PrimaryNameAttribute": "name",
      "MetadataId": "12345678-1234-1234-1234-123456789abc",
      "Forms": [
        {
          "FormId": "87654321-4321-4321-4321-210987654321",
          "FormName": "Information",
          "FieldCount": 25
        }
      ],
      "Attributes": [
        {
          "LogicalName": "accountnumber",
          "SchemaName": "AccountNumber",
          "DisplayName": "Account Number",
          "AttributeType": "String",
          "IsCustom": false
        },
        {
          "LogicalName": "new_customfield",
          "SchemaName": "new_CustomField",
          "DisplayName": "Custom Field",
          "AttributeType": "String",
          "IsCustom": true
        }
      ]
    }
  ]
}
```

### Phase 4: Use with Power BI

#### 4.1 Create Power BI Project Structure

If you don't have a project folder yet:

```bash
# The extraction script creates this automatically
Reports/
  MyProject/
    Metadata/
      CoreAI Metadata Dictionary.json
      DataverseURL.txt
    PBIP/
      # Your Power BI project files will go here
```

#### 4.2 Document Environment

Create `DataverseURL.txt` in the Metadata folder:

```
https://yourorg.crm.dynamics.com
```

This documents which environment the metadata came from.

### Phase 5: Refresh Metadata (Regular Updates)

#### 5.1 Quick Refresh

To update metadata after solution changes:

```bash
# Same command as initial extraction
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI "Reports/MyProject/Metadata"
```

This overwrites the existing JSON file with fresh metadata.

#### 5.2 Compare Changes

```bash
# View what changed
git diff Reports/MyProject/Metadata/*.json
```

#### 5.3 Commit to Version Control

```bash
git add Reports/MyProject/Metadata/*.json
git commit -m "Updated metadata from Dataverse - added new Survey fields"
git push
```

## Advanced Usage

### Multiple Environments

Extract from different environments:

```bash
# Development
python extract_metadata_from_dataverse.py https://dev-yourorg.crm.dynamics.com CoreAI "Reports/MyProject-Dev/Metadata"

# Production
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI "Reports/MyProject-Prod/Metadata"

# Compare
git diff Reports/MyProject-Dev/Metadata/*.json Reports/MyProject-Prod/Metadata/*.json
```

### CI/CD Integration

#### GitHub Actions Example

```yaml
name: Extract Dataverse Metadata

on:
  schedule:
    - cron: '0 2 * * 1'  # Every Monday at 2 AM
  workflow_dispatch:  # Manual trigger

jobs:
  extract:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Install dependencies
        run: |
          cd Code
          pip install -r requirements.txt
      
      - name: Extract metadata
        env:
          DATAVERSE_URL: ${{ secrets.DATAVERSE_URL }}
          SOLUTION_NAME: ${{ secrets.SOLUTION_NAME }}
        run: |
          cd Code
          # Use service principal auth for CI/CD
          python extract_metadata_from_dataverse.py "$DATAVERSE_URL" "$SOLUTION_NAME" "../Reports/Production/Metadata"
      
      - name: Commit changes
        run: |
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add Reports/Production/Metadata/*.json
          git diff --staged --quiet || git commit -m "Automated metadata update"
          git push
```

### Filtering and Customization

Modify [extract_metadata_from_dataverse.py](extract_metadata_from_dataverse.py) to customize:

**Include additional fields:**
```python
# Line ~160: Add to important_fields set
important_fields = {
    'createdon', 'modifiedon', 'createdby', 'modifiedby', 
    'ownerid', 'statecode', 'statuscode',
    'versionnumber', 'importsequencenumber'  # Add these
}
```

**Filter by form type:**
```python
# Line ~134: Change FormType filter
# Current: type eq 2 (Main forms only)
# Options: 1=Dashboard, 2=Main, 6=Quick Create, 7=Quick View, 11=Card
query = (
    f"{self.api_url}/systemforms?"
    f"$filter=objecttypecode eq '{entity_logical_name}' and type in (2,6)"  # Main + Quick Create
    ...
)
```

## Troubleshooting

### Authentication Issues

**Problem:** Authentication popup doesn't appear

**Solution:**
```bash
# Clear cached tokens
# Windows
del %USERPROFILE%\.msal_token_cache
# Mac/Linux
rm ~/.msal_token_cache

# Try again
python test_dataverse_connection.py https://yourorg.crm.dynamics.com
```

### No Forms Found

**Problem:** Table shows "0 forms" or is skipped

**Causes:**
1. Table has no main forms published
2. Forms are system-managed (iscustomizable = false)
3. Forms are inactive/unpublished

**Solution:**
- Publish at least one main form for the table
- Use Dataverse maker portal to verify form existence
- Check form properties → Customizable setting

### Missing Fields

**Problem:** Expected fields not in output

**Causes:**
1. Field is not on any main form
2. Field is hidden/removed from forms
3. Field is in a different form type (Quick View, Card, etc.)

**Solution:**
- Add field to at least one main form
- Verify field is visible in form editor
- Modify script to include additional form types (see Customization above)

### Performance Issues

**Problem:** Extraction is slow

**Optimizations:**
- **Parallel requests**: Modify script to fetch forms in parallel
- **Caching**: Cache entity metadata between runs
- **Filtering**: Only extract changed tables (implement change tracking)

## Best Practices

### Regular Maintenance

1. **Weekly Refresh**: Set up automated weekly extraction
2. **Change Reviews**: Review diffs before committing
3. **Documentation**: Update DataverseURL.txt if environment changes

### Version Control

1. **Meaningful Commits**: Describe what metadata changed
2. **Branch Strategy**: Use branches for major schema changes
3. **PR Reviews**: Have team review metadata changes

### Security

1. **Credentials**: Never commit tokens or credentials
2. **Service Principals**: Use service principals for automation
3. **Secrets Management**: Store environment URLs in GitHub Secrets or Azure Key Vault

## Quick Reference

### Common Commands

```bash
# Test connection
python test_dataverse_connection.py https://yourorg.crm.dynamics.com

# Preview extraction
python preview_metadata_extraction.py https://yourorg.crm.dynamics.com MySolution

# Full extraction
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/MyProject/Metadata"

# Check what changed
git diff Reports/MyProject/Metadata/*.json
```

### File Locations

| Purpose | Location |
|---------|----------|
| Extraction scripts | `Code/*.py` |
| Dependencies | `Code/requirements.txt` |
| Output JSON | `Reports/{Project}/Metadata/*.json` |
| Environment URL | `Reports/{Project}/Metadata/DataverseURL.txt` |
| PBIP files | `Reports/{Project}/PBIP/` |

## Support

- **Documentation**: [Code/README-DirectExtraction.md](README-DirectExtraction.md)
- **Issues**: Submit via GitHub Issues
- **Questions**: Review FAQ in main README.md

---

**Next:** Once you have extracted metadata, you can use it to generate Power BI semantic models or integrate with other automation tools.
