# Dataverse Metadata Extractor

A Windows Forms application for extracting metadata from Microsoft Dataverse environments and exporting it for use with Power BI semantic models.

## Features

- **OAuth Authentication**: Secure MSAL-based authentication to Dataverse environments
- **Solution Browser**: Navigate and select from available Dataverse solutions  
- **Table Selection**: Choose tables from your solution with form/view configuration
- **Attribute Selection**: 
  - View all attributes with display name, logical name, and type
  - Select attributes from forms/views or manually
  - Filter between selected and all attributes
  - Lookup relationship tracking (Targets property)
- **Metadata Export**: Export selected tables and attributes to JSON format
- **24-Hour Caching**: Metadata cache for improved performance
- **Settings Persistence**: Saves preferences and selections between sessions

## Quick Start

### Option 1: Run from Release

```batch
LaunchExtractor.bat
```

### Option 2: Build and Run

```powershell
cd DataverseMetadataExtractor
dotnet build --configuration Release
dotnet run
```

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Dataverse environment access (read permissions)

## Usage

1. **Connect**: Enter your Dataverse environment URL and click Connect
2. **Authenticate**: Sign in via browser when prompted
3. **Select Solution**: Choose your solution from the dropdown
4. **Add Tables**: Click "Add Table" to select tables from your solution
5. **Configure**: For each table, select a form and view using the ✏️ edit button
6. **Select Attributes**: Check/uncheck attributes to include in export
7. **Export**: Click "Export Metadata JSON" to generate the output file

## Output Format

The exported JSON contains:

```json
{
  "Environment": "https://yourorg.crm.dynamics.com",
  "Solution": "YourSolution",
  "ProjectName": "MyProject",
  "Tables": [
    {
      "LogicalName": "account",
      "DisplayName": "Account",
      "SchemaName": "Account",
      "PrimaryIdAttribute": "accountid",
      "PrimaryNameAttribute": "name",
      "Forms": [...],
      "View": {...},
      "Attributes": [
        {
          "LogicalName": "name",
          "DisplayName": "Account Name",
          "SchemaName": "Name",
          "AttributeType": "String",
          "IsCustomAttribute": false,
          "Targets": null
        },
        {
          "LogicalName": "primarycontactid",
          "DisplayName": "Primary Contact",
          "SchemaName": "PrimaryContactId", 
          "AttributeType": "Lookup",
          "IsCustomAttribute": false,
          "Targets": ["contact"]
        }
      ]
    }
  ]
}
```

## Project Structure

```
DataverseMetadata-to-PowerBI-Semantic-Model/
├── DataverseMetadataExtractor/     # WinForms application
│   ├── Forms/                      # UI components
│   │   ├── MainForm.cs            # Main application window
│   │   ├── TableSelectorDialog.cs # Table selection dialog
│   │   └── FormViewSelectorDialog.cs # Form/View selector
│   ├── Models/                     # Data models
│   │   └── DataModels.cs          # All domain objects
│   ├── Services/                   # Business logic
│   │   ├── DataverseClient.cs     # Dataverse API client
│   │   └── SettingsManager.cs     # Settings/cache persistence
│   └── Program.cs                  # Entry point
├── Reports/                        # Output location for projects
│   └── [ProjectName]/
│       └── Metadata/
│           └── *.json              # Generated metadata files
├── PBIP_DefaultTemplate/           # Power BI project template
├── LaunchExtractor.bat             # Quick launcher
└── README.md                       # This file
```

## Cached Data Location

Settings and cache are stored in:
```
%APPDATA%\DataverseMetadataExtractor\
├── .dataverse_metadata_settings.json   # User preferences & selections
└── .dataverse_metadata_cache.json      # Metadata cache (24hr expiry)
```

## Authentication

Uses the official Dataverse OAuth App Registration:
- **Client ID**: 51f81489-12ee-4a9e-aaae-a2591f45987d
- **Authority**: https://login.microsoftonline.com/organizations
- Interactive browser authentication with token caching

## Dependencies

- .NET 8.0 Runtime (Windows)
- Microsoft.Identity.Client 4.61.3 (OAuth/MSAL)
- Newtonsoft.Json 13.0.3 (JSON serialization)

## Building from Source

```powershell
# Clone the repository
git clone https://github.com/your-repo/DataverseMetadata-to-PowerBI-Semantic-Model.git
cd DataverseMetadata-to-PowerBI-Semantic-Model

# Build
dotnet build DataverseMetadataExtractor/DataverseMetadataExtractor.csproj

# Run
dotnet run --project DataverseMetadataExtractor/DataverseMetadataExtractor.csproj
```

## License

MIT License
