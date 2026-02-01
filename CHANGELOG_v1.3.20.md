# v1.3.20 Release Notes - Calendar/Date Table Implementation

## Release Date: January 20, 2025

## Summary
This release implements the Calendar/Date Table feature for XrmToolBox, bringing feature parity with the Configurator app. Users can now configure automatic Date dimension table generation with timezone adjustments and year range customization.

## What's New

### Calendar/Date Table Configuration Dialog
- **Full-Featured Dialog**: Created comprehensive CalendarTableDialog (698 lines) with:
  - Primary date field selection (combo box with table + field selection)
  - Timezone configuration with full Windows timezone support
  - Year range configuration (start/end year with numeric up/down controls)
  - Additional DateTime field wrapping across all selected tables
  - Preview of all DateTime fields with multi-select checkbox list

### Features
1. **Primary Date Field Selection**
   - Choose any DateTime field from selected tables
   - Automatically creates relationship between fact table and Date dimension
   - Displays table and field display names for easy selection

2. **Timezone Adjustment**
   - Select from all Windows timezones
   - Displays current UTC offset for selected timezone
   - Automatically wraps DateTime fields to adjust for timezone
   - Option to convert to date-only (truncate time component)

3. **Year Range Configuration**
   - Set start year (default: current year - 2)
   - Set end year (default: current year + 5)
   - Generates Date table with full date range

4. **Additional DateTime Field Wrapping**
   - Scans all selected tables for DateTime fields
   - Multi-select checkbox list for easy field selection
   - Displays table and field display names
   - Automatically includes primary date field
   - Select All / Clear All buttons for convenience

5. **Configuration Persistence**
   - DateTableConfig saved in PluginSettings
   - Automatically restores previous configuration
   - Survives model reload and application restart

## Technical Changes

### Architecture Improvements
- **Unified Type System**:
  - Removed duplicate `DateTableConfig` class from `XrmToolBox.Models`
  - Now uses `Core.Models.DateTableConfig` throughout the codebase
  - Eliminated type conflicts and ambiguous references
  - Better separation of concerns between XrmToolBox-specific models and shared Core models

- **Type Aliasing**:
  - Added `using XrmModels = DataverseToPowerBI.XrmToolBox.Models` to SemanticModelBuilder
  - Explicit type qualification for `ExportRelationship` to avoid namespace conflicts
  - Consistent use of Core.Models types in PluginControl

- **Build Integration**:
  - Updated `BuildSemanticModel` to pass `dateTableConfig` to semantic model builder
  - Modified `AnalyzeChanges` and `ApplyChanges` to accept DateTableConfig parameter
  - Full integration with change detection and diff preview

### Files Changed
1. **New Files**:
   - `CalendarTableDialog.cs` (698 lines)
     - WinForms dialog with comprehensive UI
     - Timezone selection using TimeZoneInfo API
     - DateTime field scanning and selection logic
     - Configuration validation and persistence

2. **Modified Files**:
   - `PluginControl.cs`:
     - Added `BtnCalendarTable_Click` implementation (full dialog instead of MessageBox)
     - Updated `BuildSemanticModel` to pass dateTableConfig
     - Extended PluginSettings class with DateTableConfig property
   
   - `Models/SemanticModelDataModels.cs`:
     - Removed duplicate DateTableConfig class
     - Removed duplicate DateTimeFieldConfig class
     - Added comment noting use of Core.Models versions
   
   - `Services/SemanticModelBuilder.cs`:
     - Added `using DataverseToPowerBI.Core.Models`
     - Added `using XrmModels = DataverseToPowerBI.XrmToolBox.Models` alias
     - No functional changes, just import updates
   
   - `DataverseToPowerBI.XrmToolBox.csproj`:
     - Added CalendarTableDialog.cs to compilation
   
   - `Properties/AssemblyInfo.cs`:
     - Version bumped to 1.3.20.0

### Bug Fixes
- Fixed string.Contains() overload issue in CalendarTableDialog
  - Changed from `.Contains(string, StringComparison)` to `.IndexOf(string, StringComparison) >= 0`
  - Required for .NET Framework 4.8 compatibility

- Fixed type conversion errors between Core.Models and XrmToolBox.Models
  - Changed `exportRelationships` to use `Core.Models.ExportRelationship`
  - Eliminated 12 compilation errors related to type ambiguity

## Known Limitations (Now Resolved)
- ~~Calendar/Date Table feature not implemented in XrmToolBox~~ ✅ **RESOLVED in v1.3.20**
- Feature now has full parity with Configurator app

## Upgrade Notes
- No breaking changes
- Existing models will work without modification
- New Calendar Table button enabled when tables are selected
- Configuration is optional - existing builds continue to work without date table

## Testing Recommendations
1. Open XrmToolBox plugin and connect to environment
2. Select tables for semantic model
3. Click "Calendar Table" button
4. Configure:
   - Primary date field (e.g., "createdon" from main fact table)
   - Timezone (e.g., "Eastern Standard Time")  
   - Year range (e.g., 2020-2030)
   - Additional DateTime fields to wrap
5. Click OK to save configuration
6. Build semantic model
7. Verify Date table generated with correct:
   - Year range
   - Timezone-adjusted columns
   - Relationships to fact tables
   - Additional wrapped DateTime columns

## Build Information
- **Build Status**: ✅ SUCCESS
- **Warnings**: 39 (all nullable reference type annotations - safe to ignore)
- **Errors**: 0
- **Output**: `bin\Release\DataverseToPowerBI.XrmToolBox.dll`

## Contributor Notes
This was a complex implementation involving:
- 698-line WinForms dialog creation
- Type system refactoring to eliminate duplicates
- Careful namespace management with aliases
- Integration with existing semantic model builder
- .NET Framework 4.8 compatibility fixes

The implementation brings XrmToolBox to full feature parity with the standalone Configurator app for Calendar/Date Table functionality.
