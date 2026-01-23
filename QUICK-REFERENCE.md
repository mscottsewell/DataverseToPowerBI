# Quick Reference Card

## 🚀 One-Liners

```bash
# Install
pip install -r Code/requirements.txt

# List solutions
python Code/test_dataverse_connection.py https://yourorg.crm.dynamics.com

# Preview
python Code/preview_metadata_extraction.py https://yourorg.crm.dynamics.com MySolution

# Extract
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/MyProject/Metadata"
```

## 📁 Files Created

| File | Purpose |
|------|---------|
| `Code/extract_metadata_from_dataverse.py` | Main extraction |
| `Code/test_dataverse_connection.py` | Test & list solutions |
| `Code/preview_metadata_extraction.py` | Preview before extract |
| `Code/requirements.txt` | Python dependencies |
| `QuickStart.ps1` / `.bat` | Automated setup |

## 📚 Documentation

| File | What's In It |
|------|--------------|
| [SUMMARY.md](SUMMARY.md) | Quick overview & answer to your question |
| [WHATS-NEW.md](WHATS-NEW.md) | What's changed in this update |
| [Code/README-DirectExtraction.md](Code/README-DirectExtraction.md) | Complete usage guide |
| [WORKFLOW-GUIDE.md](WORKFLOW-GUIDE.md) | End-to-end workflow |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture & data flow |
| [README.md](README.md) | Main project README (updated) |

## ⌨️ Common Commands

### First Time Setup
```bash
cd Code
pip install -r requirements.txt
```

### Find Your Solution Name
```bash
python Code/test_dataverse_connection.py https://yourorg.crm.dynamics.com
```

### Preview What Will Be Extracted
```bash
python Code/preview_metadata_extraction.py https://yourorg.crm.dynamics.com MySolution
```

### Extract Metadata
```bash
# To current directory
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution

# To specific folder
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/MyProject/Metadata"
```

### Check What Changed
```bash
git status
git diff Reports/MyProject/Metadata/*.json
```

### Commit Changes
```bash
git add Reports/MyProject/Metadata/*.json
git commit -m "Updated metadata from Dataverse"
git push
```

## 🔧 Script Parameters

### extract_metadata_from_dataverse.py

```bash
python extract_metadata_from_dataverse.py <environment_url> <solution_name> [output_folder]
```

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| environment_url | Yes | Dataverse URL | `https://yourorg.crm.dynamics.com` |
| solution_name | Yes | Solution unique name | `CoreAI` |
| output_folder | No | Output directory | `Reports/MyProject/Metadata` |

### test_dataverse_connection.py

```bash
python test_dataverse_connection.py <environment_url>
```

### preview_metadata_extraction.py

```bash
python preview_metadata_extraction.py <environment_url> <solution_name>
```

## 🎯 What Each Script Does

| Script | Input | Output | Use When |
|--------|-------|--------|----------|
| **test_dataverse_connection.py** | Environment URL | List of solutions | Finding solution name |
| **preview_metadata_extraction.py** | Environment + Solution | Table/form summary | Before full extraction |
| **extract_metadata_from_dataverse.py** | Environment + Solution | JSON file | Ready to extract |

## 📊 Output Format

```json
{
  "Environment": "https://yourorg.crm.dynamics.com",
  "Solution": "MySolution",
  "Tables": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "SchemaName": "Account",
      "Forms": [{"FormName": "Information", "FieldCount": 25}],
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

## 🚨 Troubleshooting Quick Fixes

| Problem | Quick Fix |
|---------|-----------|
| "Import 'msal' could not be resolved" | `pip install -r Code/requirements.txt` |
| "Solution not found" | Run `test_dataverse_connection.py` to see solution list |
| "Authentication failed" | Clear token cache: Delete `%USERPROFILE%\.msal_token_cache` |
| "No forms found" | Check if table has published main forms |
| Browser doesn't open | Check firewall/proxy settings |

## 🔑 Finding Your Solution Name

The **unique name** (not display name) is required.

### Method 1: Use test_dataverse_connection.py
```bash
python Code/test_dataverse_connection.py https://yourorg.crm.dynamics.com
# Shows: Display Name → Unique Name
```

### Method 2: Power Platform Admin Center
1. Go to https://admin.powerplatform.microsoft.com
2. Environments → Your Environment → Solutions
3. Click solution → See "Name" field

### Method 3: Direct API
Navigate to: `https://yourorg.crm.dynamics.com/api/data/v9.2/solutions?$select=uniquename,friendlyname`

## ⚡ Performance Tips

| Scenario | Typical Time |
|----------|--------------|
| First run (with auth) | 30-60 seconds |
| Subsequent runs | 10-30 seconds (cached token) |
| 15 tables | ~30 seconds |
| 50 tables | ~2 minutes |

## 🎨 Example Workflows

### New Project
```bash
# 1. Test connection
python Code/test_dataverse_connection.py https://yourorg.crm.dynamics.com

# 2. Preview
python Code/preview_metadata_extraction.py https://yourorg.crm.dynamics.com MySolution

# 3. Extract
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/NewProject/Metadata"

# 4. Commit
git add Reports/NewProject/
git commit -m "Initial metadata extraction"
```

### Refresh Existing Project
```bash
# 1. Extract (overwrites JSON)
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution "Reports/ExistingProject/Metadata"

# 2. Review changes
git diff Reports/ExistingProject/Metadata/*.json

# 3. Commit if satisfied
git add Reports/ExistingProject/Metadata/*.json
git commit -m "Refreshed metadata - added new fields"
```

### Compare Environments
```bash
# Dev
python Code/extract_metadata_from_dataverse.py https://dev.crm.dynamics.com MySolution "Reports/Dev/Metadata"

# Prod
python Code/extract_metadata_from_dataverse.py https://prod.crm.dynamics.com MySolution "Reports/Prod/Metadata"

# Compare
git diff --no-index Reports/Dev/Metadata/*.json Reports/Prod/Metadata/*.json
```

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| requests | 2.31.0+ | HTTP API calls |
| msal | 1.25.0+ | Microsoft authentication |
| pandas | 2.0.0+ | Data handling |
| openpyxl | 3.1.0+ | Excel compatibility |

## 🔗 Useful Links

| Resource | Link |
|----------|------|
| Python Download | https://python.org/downloads/ |
| Power Platform Admin | https://admin.powerplatform.microsoft.com |
| Dataverse Web API Docs | https://learn.microsoft.com/power-apps/developer/data-platform/webapi/overview |
| MSAL Python Docs | https://github.com/AzureAD/microsoft-authentication-library-for-python |

## 💡 Tips & Tricks

### Tip 1: Cache Authentication
After first run, token is cached. Subsequent runs don't need browser login.

### Tip 2: Use Wildcards
You can run commands from anywhere:
```bash
python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com MySolution
```

### Tip 3: Redirect Output
Capture console output:
```bash
python Code/extract_metadata_from_dataverse.py ... > extraction.log 2>&1
```

### Tip 4: Version Control
Always commit JSON files after extraction:
```bash
git add Reports/*/Metadata/*.json
git commit -m "Updated all project metadata"
```

### Tip 5: Batch Multiple Solutions
```bash
for solution in CoreAI Sales Service; do
  python Code/extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com $solution "Reports/$solution/Metadata"
done
```

## 📋 Checklist: New Project

- [ ] Run `pip install -r Code/requirements.txt`
- [ ] Test connection with `test_dataverse_connection.py`
- [ ] Find solution unique name
- [ ] Preview extraction with `preview_metadata_extraction.py`
- [ ] Run full extraction
- [ ] Create `DataverseURL.txt` in Metadata folder
- [ ] Commit JSON to Git
- [ ] Document any customizations

## 🎯 Quick Decision Guide

**Should I use direct extraction or Excel?**

Use **Direct Extraction** if:
- ✅ You want automation
- ✅ You want always-current metadata
- ✅ You want CI/CD integration
- ✅ You want Git-friendly output

Use **Excel** if:
- ✅ You prefer visual tools
- ✅ You want to manually select specific forms
- ✅ You don't have Python
- ✅ One-time extraction only

---

**Need more details?** See [SUMMARY.md](SUMMARY.md) or [WORKFLOW-GUIDE.md](WORKFLOW-GUIDE.md)
