# Direct Dataverse Metadata Extraction

Extract metadata directly from Dataverse without needing Excel files. This method uses the Dataverse Web API to query solution components, forms, and fields programmatically.

## Prerequisites

1. **Python 3.8+** installed
2. **Install required packages:**
   ```bash
   pip install -r requirements.txt
   ```

3. **Dataverse Access:**
   - You must have read access to the Dataverse environment
   - You need to know the **solution unique name** (not the display name)

## Finding Your Solution Name

The solution unique name is different from the display name you see in the UI. To find it:

### Option 1: Power Platform Admin Center
1. Go to https://admin.powerplatform.microsoft.com
2. Navigate to Environments → Your Environment → Solutions
3. Click on your solution
4. The unique name is shown in the solution details

### Option 2: Using PowerShell
```powershell
# Install the module if needed
Install-Module -Name Microsoft.PowerApps.Administration.PowerShell

# Connect and list solutions
Add-PowerAppsAccount
Get-AdminPowerAppEnvironment | Get-AdminPowerAppSolution | Select-Object DisplayName, SolutionName
```

### Option 3: Direct API Query
Navigate to: `https://yourorg.crm.dynamics.com/api/data/v9.2/solutions?$select=uniquename,friendlyname`

## Usage

### Basic Usage

```bash
python extract_metadata_from_dataverse.py <environment_url> <solution_name>
```

**Example:**
```bash
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI
```

### With Custom Output Folder

```bash
python extract_metadata_from_dataverse.py <environment_url> <solution_name> <output_folder>
```

**Example:**
```bash
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI "Reports/MyProject/Metadata"
```

## How It Works

The script performs the following steps:

1. **Authentication**: Opens a browser window for you to sign in with your Microsoft account
2. **Solution Query**: Retrieves all tables (entities) that are part of the specified solution
3. **Form Analysis**: For each table, finds all main forms
4. **Field Extraction**: Parses form XML to identify which fields are used in forms
5. **Metadata Collection**: Retrieves detailed metadata for each field including:
   - Logical name
   - Schema name
   - Display name
   - Attribute type
   - Custom vs. standard field
6. **JSON Export**: Saves all metadata to a JSON file in the output folder

## Output

The script creates a JSON file named `{SolutionName} Metadata Dictionary.json` with the structure:

```json
{
  "Environment": "https://yourorg.crm.dynamics.com",
  "Solution": "CoreAI",
  "Tables": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "SchemaName": "Account",
      "Forms": [
        {
          "FormId": "...",
          "FormName": "Account",
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
        }
      ]
    }
  ]
}
```

## Advantages Over Excel Export

✓ **Automated**: No manual export process  
✓ **Current**: Always gets the latest metadata  
✓ **Consistent**: Eliminates human error in form selection  
✓ **Repeatable**: Easy to refresh metadata anytime  
✓ **Version Control**: JSON format works well with Git  
✓ **CI/CD Ready**: Can be integrated into automated pipelines  

## Authentication Details

The script uses **MSAL (Microsoft Authentication Library)** with interactive browser authentication:

- First run: Browser window opens for sign-in
- Subsequent runs: Uses cached token (no sign-in needed)
- Tokens are stored securely by MSAL
- Works with MFA and conditional access policies

## Troubleshooting

### "Solution not found"
- Verify the solution unique name (not the display name)
- Ensure you have access to the environment
- Check that the solution exists in the environment

### "Authentication failed"
- Ensure you have the correct permissions
- Try clearing cached tokens: Delete `%USERPROFILE%\.msal_token_cache`
- Check your network/proxy settings

### "No forms found"
- Verify the table has main forms published
- Check that forms are not all system forms
- Ensure forms have `iscustomizable` = true

### Import errors (msal, requests)
- Run: `pip install -r requirements.txt`
- Ensure you're using the correct Python environment

## Next Steps

After extracting metadata, you can:

1. Use the JSON with the existing PowerShell script to generate Power BI semantic models
2. Compare metadata across environments
3. Automate metadata refresh in CI/CD pipelines
4. Generate documentation from the metadata

## Example: Complete Workflow

```bash
# 1. Extract metadata
python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI "Reports/MyProject/Metadata"

# 2. Generate Power BI files (if you have the PowerShell script configured)
# The JSON output can be used by other automation scripts

# 3. Version control
git add Reports/MyProject/Metadata/*.json
git commit -m "Updated metadata from Dataverse"
```
