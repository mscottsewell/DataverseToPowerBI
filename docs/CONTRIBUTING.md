# Developer Guide

This document is for developers who want to build from source, contribute
improvements, or understand the codebase architecture.

For user documentation, see [README.md](README.md).

---

## 🏗️ Repository Structure

```
DataverseToPowerBI/
├── DataverseToPowerBI.Core/           # Shared library (.NET Framework 4.8)
│   ├── Interfaces/
│   │   └── IDataverseConnection.cs    # Connection abstraction
│   └── Models/
│       └── DataModels.cs              # Shared data models
│
├── DataverseToPowerBI.XrmToolBox/     # XrmToolBox plugin (.NET Framework 4.8)
│   ├── Assets/
│   │   ├── DateTable.tmdl             # Date dimension table template
│   │   └── PBIP_DefaultTemplate/      # Power BI project template files
│   ├── Models/
│   │   └── SemanticModelDataModels.cs # Plugin-specific models (ExportRelationship)
│   ├── Services/
│   │   ├── DebugLogger.cs             # File-based diagnostic logging
│   │   ├── FetchXmlToSqlConverter.cs  # FetchXML → SQL WHERE translation
│   │   └── SemanticModelBuilder.cs    # TMDL generation engine (~5,000 lines)
│   ├── CalendarTableDialog.cs         # Date table configuration UI
│   ├── FactDimensionSelectorForm.cs   # Star-schema configuration UI
│   ├── FormViewSelectorForm.cs        # Form/View selection UI
│   ├── PluginControl.cs               # Main plugin UI control
│   ├── SemanticModelManager.cs        # Configuration persistence
│   ├── SemanticModelSelectorDialog.cs # Model management UI
│   ├── SolutionSelectorForm.cs        # Solution picker UI
│   ├── TableSelectorForm.cs           # Table selection UI
│   ├── TmdlPluginTool.cs              # XrmToolBox plugin entry point
│   ├── UrlHelper.cs                   # Shared URL/environment utilities
│   └── XrmServiceAdapterImpl.cs       # SDK-based Dataverse adapter
│
├── Package/                           # NuGet package staging (gitignored)
├── Build-And-Deploy.ps1               # Build and deploy script
├── DataverseToPowerBI.XrmToolBox.nuspec  # NuGet package definition
└── DataverseToPowerBI.sln
```

---

## 🔧 Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| Visual Studio 2022 | 17.0+ | IDE with .NET development workload |
| .NET Framework 4.8 SDK | 4.8 | Target framework for XrmToolBox compatibility |
| XrmToolBox | Latest | For testing the plugin |
| Power BI Desktop | Nov 2023+ | For testing generated models |

### NuGet Packages (Restored Automatically)

**Core Library:**
- `Newtonsoft.Json` - JSON serialization

**XrmToolBox Plugin:**
- Uses assemblies provided by XrmToolBox
  (XrmToolBox.Extensibility, Microsoft.Xrm.Sdk, etc.)
- All authentication is handled by XrmToolBox - no additional packages needed

---

## 🛠️ Building the Project

### Quick Build

```powershell
# Clone the repository
git clone https://github.com/your-org/DataverseToPowerBI.git
cd DataverseToPowerBI

# Restore and build
dotnet build -c Release
```

### Using the Build Script

The `Build-And-Deploy.ps1` script provides a complete build pipeline:

```powershell
# Full build, package, and deploy to local XrmToolBox
.\Build-And-Deploy.ps1

# Build and create NuGet package only (no local deploy)
.\Build-And-Deploy.ps1 -PackageOnly

# Deploy existing build without rebuilding
.\Build-And-Deploy.ps1 -DeployOnly
```

The script:
1. Cleans the solution
2. Updates AssemblyInfo.cs with the version number
3. Builds Core and XrmToolBox projects
4. Creates NuGet package structure
5. Deploys to local XrmToolBox plugins folder

### Manual Deployment

```powershell
# Build outputs
$coreDll = "DataverseToPowerBI.Core\bin\Release\net48\DataverseToPowerBI.Core.dll"
$pluginDll = "DataverseToPowerBI.XrmToolBox\bin\Release\DataverseToPowerBI.XrmToolBox.dll"

# XrmToolBox plugins folder
$plugins = "$env:APPDATA\MscrmTools\XrmToolBox\Plugins"

# Copy DLLs
Copy-Item $coreDll $plugins
Copy-Item $pluginDll $plugins

# Copy Assets folder
Copy-Item "DataverseToPowerBI.XrmToolBox\Assets\*" "$plugins\DataverseToPowerBI"
 -Recurse
```

---

## 🏛️ Architecture Overview

### Project Dependency Graph

```
┌─────────────────────────────────────┐
│   DataverseToPowerBI.XrmToolBox     │  ← XrmToolBox Plugin (UI + Services)
└─────────────────┬───────────────────┘
                  │ references
                  ▼
┌─────────────────────────────────────┐
│     DataverseToPowerBI.Core         │  ← Shared Library (Models + Interfaces)
└─────────────────────────────────────┘
```

### Key Abstractions

**IDataverseConnection** (Core)
```csharp
public interface IDataverseConnection
{
    bool IsConnected { get; }
    Task<string> AuthenticateAsync(bool clearCredentials = false);
    Task<List<DataverseSolution>> GetSolutionsAsync();
    Task<List<TableInfo>> GetSolutionTablesAsync(string solutionId);
    Task<List<AttributeMetadata>> GetAttributesAsync(string tableName);
    // ... additional metadata retrieval methods
}
```

**XrmServiceAdapterImpl** implements this interface using the Dataverse SDK
 (`IOrganizationService`), which XrmToolBox provides.

### Data Flow

```
┌─────────────┐    ┌──────────────────┐    ┌───────────────────┐
│   User UI   │───▶│  PluginControl   │───▶│ SemanticModelMgr  │
│  (Dialogs)  │    │  (Orchestrator)  │    │  (Persistence)    │
└─────────────┘    └────────┬─────────┘    └───────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │  XrmServiceAdapterImpl  │
              │   (Dataverse Queries)   │
              └────────────┬────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │  SemanticModelBuilder   │
              │   (TMDL Generation)     │
              │                         │
              │  Connection Modes:      │
              │  • DataverseTDS         │
              │  • FabricLink           │
              └─────────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │   PBIP Folder Output    │
              │   (*.tmdl, *.pbip)      │
              └─────────────────────────┘
```

---

## 📦 Key Components

### SemanticModelBuilder.cs

The core TMDL generation engine (~3,000 lines). Key responsibilities:

- **Template Management:** Copies and customizes the PBIP template
- **Dual Connection Support:** Generates TMDL for both DataverseTDS and FabricLink connection modes
- **Table Generation:** Creates `{TableName}.tmdl` files with columns,
 partitions, and annotations
- **Relationship Generation:** Builds `relationships.tmdl` from lookup metadata
- **Change Detection:** Compares existing model to detect incremental updates
- **User Code Preservation:** Keeps custom measures when rebuilding
- **Auto-Generated Measures:** Creates count and URL link measures on fact tables
- **Virtual Column Corrections:** Dictionary-based mapping to fix problematic TDS virtual column names (e.g., `contact.donotsendmmname` → `donotsendmarketingmaterial`)
- **Display Name Renaming:** Per-model and per-attribute overrides applied through a Power Query `Table.RenameColumns` step, with duplicate detection
- **Rich Column Metadata:** Generates comprehensive descriptions including Dataverse descriptions, source attribution, and lookup targets

```csharp
// Key methods
public void Build(semanticModelName, outputFolder, dataverseUrl, tables,
 relationships, ...);
public List<SemanticModelChange> AnalyzeChanges(...);  // Preview changes
public bool ApplyChanges(...);  // Apply with optional backup
```

### FetchXmlToSqlConverter.cs

Translates Dataverse view FetchXML filters to T-SQL WHERE clauses for DirectQuery:

```csharp
var converter = new FetchXmlToSqlConverter(utcOffsetHours: -6, isFabricLink: false);
var result = converter.ConvertToWhereClause(fetchXml, tableAlias: "Base");
// Result: "Base.statecode = 0 AND Base.createdon >= '2024-01-01'"
```

**Supported Operators:**
- Comparison: `eq`, `ne`, `gt`, `ge`, `lt`, `le`
- Null: `null`, `not-null`
- String: `like`, `begins-with`, `ends-with`
- Date: `today`, `yesterday`, `this-week`, `last-x-days`, etc.
- Lists: `in`, `not-in`
- User Context (TDS only): `eq-userid`, `ne-userid`, `eq-userteams`, `ne-userteams`

> **Note:** User context operators are not supported in FabricLink mode due to Direct Lake limitations. Set `isFabricLink: true` to skip these operators.

### SemanticModelManager.cs

Manages configuration persistence in JSON format:

```
%APPDATA%\MscrmTools\XrmToolBox\Settings\DataverseToPowerBI\semantic-models.json
```

Stores:
- Model configurations (name, URL, working folder)
- Selected tables and their form/view choices
- Relationship configurations
- Date table settings

### PluginControl.cs

Main XrmToolBox UI control. Uses the XrmToolBox SDK pattern:

```csharp
public partial class PluginControl : PluginControlBase
{
    // Background work pattern
    WorkAsync(new WorkAsyncInfo
    {
        Work = (worker, args) => { /* Background thread */ },
        PostWorkCallBack = (args) => { /* UI thread */ }
    });
}
```

---

## 🧪 Testing

### Manual Testing Workflow

1. Build and deploy: `.\Build-And-Deploy.ps1`
2. Launch XrmToolBox
3. Connect to a Dataverse environment
4. Open the plugin and test scenarios:
   - Create new model configuration
   - Select tables from a solution
   - Configure star-schema relationships
   - Build and verify PBIP output
   - Open in Power BI Desktop

### Debug Logging

Enable diagnostic logging via `DebugLogger`:

```csharp
DebugLogger.Log("Processing table: account");
DebugLogger.LogSection("FetchXML", fetchXmlContent);
```

Log location: `%APPDATA%\DataverseToPowerBI\debug_log.txt`

### FetchXML Debug Output

FetchXML conversion debug files can be written to `{outputFolder}/FetchXML_Debug/` during builds. This is **off by default** and must be explicitly enabled via the `enableFetchXmlDebugLogs` constructor parameter on `SemanticModelBuilder`. Debug output includes the source FetchXML, generated SQL, and conversion trace — useful for troubleshooting view filter translation.

### Automated Tests

Run the xUnit test suite (58 builder tests + 28 FetchXML converter tests + 12 incremental update tests):

```powershell
dotnet test DataverseToPowerBI.Tests -c Release
```

---

## 🔐 Security Considerations

### XML Parsing (XXE Prevention)
All XML parsing uses secure settings:

```csharp
private static XDocument ParseXmlSecurely(string xml)
{
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };
    using var reader = XmlReader.Create(new StringReader(xml), settings);
    return XDocument.Load(reader);
}
```

### File Path Validation
User-provided paths are validated to prevent directory traversal.

### No Credential Storage
Authentication is handled by XrmToolBox's connection manager. The plugin never
 stores or accesses credentials directly.

---

## 📝 Code Style Guidelines

- **Naming:** PascalCase for public members, _camelCase for private fields
- **Documentation:** XML doc comments on all public types and methods
- **File Headers:** Each file includes a purpose comment block
- **Regions:** Use `#region` to organize large files (>500 lines)
- **Null Safety:** Use nullable reference types where appropriate
- **TMDL Annotations:** Columns include a `DataverseToPowerBI_LogicalName` annotation containing the Dataverse logical name. This enables stable lineage tag resolution when display names change. The annotation is always regenerated (not treated as a user annotation).

---

## 🚀 Release Process

### Version Numbering

Format: `Major.Year.Minor.Patch` (e.g., `1.2026.3.0`)

- **Major:** Breaking changes or significant features
- **Year:** Calendar year of the release
- **Minor:** New features, backward compatible
- **Patch:** Bug fixes

### Creating a Release

1. Update version in `Build-And-Deploy.ps1`:
   ```powershell
   $version = "1.3.39"
   ```

2. Update `docs/CHANGELOG.md` with release notes

3. Build and test:
   ```powershell
   .\Build-And-Deploy.ps1 -PackageOnly
   ```

4. Commit and tag:
   ```bash
   git add -A
   git commit -m "Release v1.2026.3.0"
   git tag v1.2026.3.0
   git push origin main --tags
   ```

5. Upload `Package/*.nupkg` to XrmToolBox Tool Library

---

## 🤝 Contributing

### Getting Started

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Test thoroughly with XrmToolBox
5. Submit a pull request

### Pull Request Guidelines

- Include a clear description of changes
- Reference any related issues
- Ensure build succeeds
- Add/update documentation as needed
- Follow existing code style

### Reporting Issues

Use GitHub Issues with:
- Clear reproduction steps
- Expected vs. actual behavior
- XrmToolBox and Power BI Desktop versions
- Relevant log output

---

## 📚 Additional Resources

### Microsoft Documentation
- [Dataverse Web API](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/overview)
- [Dataverse SDK](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/overview)
- [TMDL Reference](https://learn.microsoft.com/en-us/analysis-services/tmdl/tmdl-reference)
- [Power BI PBIP Format](https://learn.microsoft.com/en-us/power-bi/developer/projects/projects-overview)

### XrmToolBox Development
- [XrmToolBox SDK Documentation](https://www.xrmtoolbox.com/documentation/)
- [Creating XrmToolBox Plugins](https://www.yourjedi.com/xrmtoolbox-plugin-development/)

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.
