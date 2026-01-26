# Dataverse Metadata Extractor - Windows Forms Application

## Overview
Standalone Windows Forms application for extracting metadata from Dataverse environments. This replaces the Python tkinter UI with a native C# application that provides reliable visual feedback and will eventually integrate with XrmToolBox.

## Features
- **OAuth Authentication**: MSAL-based authentication to Dataverse environments
- **Solution Browser**: Navigate and select from available Dataverse solutions
- **Table Viewer**: Browse tables within selected solutions
- **Attribute Selection**: 
  - View all attributes with display name, logical name, and type
  - Select/deselect individual attributes with visual feedback (checkboxes + row highlighting)
  - Bulk select/deselect all attributes
- **Metadata Export**: Export selected attributes with table metadata to JSON
- **Caching**: 24-hour metadata cache for improved performance
- **Settings Persistence**: Saves last used environment URL and preferences

## Architecture

### Models (DataModels.cs)
- `AppSettings`: User preferences
- `MetadataCache`: 24hr cache with validity check
- `DataverseSolution`, `TableMetadata`, `AttributeMetadata`: Dataverse objects
- `FormMetadata`: Form definitions with field extraction
- `ExportMetadata`: Export package structure

### Services
- `SettingsManager`: JSON persistence to AppData folder
- `DataverseClient`: HTTP client for Dataverse Web API
  - OAuth with MSAL (interactive/silent auth)
  - Solution/table/attribute queries
  - Form metadata with XML parsing

### UI (MainForm)
- SplitContainer layout: Solutions/Tables | Attributes
- ListView for solutions and tables
- DataGridView for attributes with checkboxes
- Status bar with progress indicator
- Async loading throughout

## Building & Running

### Quick Start
```
LaunchExtractor.bat
```

### Manual Build
```
cd DataverseMetadataExtractor
dotnet build --configuration Release
dotnet run
```

## Dependencies
- .NET 8.0 Runtime (Windows)
- Microsoft.Identity.Client 4.61.3 (OAuth/MSAL)
- Newtonsoft.Json 13.0.3 (JSON serialization)

## Visual Feedback Fix
The C# rewrite solves the tkinter rendering issue:
- **Before**: Python tkinter checkboxes and row colors not updating despite correct data
- **After**: Native Windows Forms DataGridView with reliable checkbox state and row highlighting
- Row colors change immediately when checkbox state changes (LightGreen for selected, default for unselected)

## Future Integration
This standalone application is designed to be ported to XrmToolBox:
- Same architecture (MSAL auth, Web API client, async patterns)
- Same data models and business logic
- UI will be adapted to XrmToolBox PluginControlBase
- Services layer remains identical

## Cached Data Location
`%APPDATA%\DataverseMetadataExtractor\`
- `settings.json`: User preferences
- `cache.json`: Metadata cache (auto-expires after 24 hours)

## Authentication
Uses the official Dataverse OAuth App Registration:
- Client ID: 51f81489-12ee-4a9e-aaae-a2591f45987d
- Authority: https://login.microsoftonline.com/organizations
- Interactive browser auth flow for initial login
- Silent token refresh for subsequent requests
