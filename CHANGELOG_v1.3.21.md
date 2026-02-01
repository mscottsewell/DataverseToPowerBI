# v1.3.21 Release Notes - Date Table Relationship Display Fix

## Release Date: January 31, 2026

## Summary
This is a bug fix release that adds the missing Date table relationship display in the main form after configuring the Calendar/Date table.

## What Was Fixed

### Date Table Relationship Display
**Issue**: After configuring the Calendar/Date table through the dialog, the relationship to the Date table wasn't showing in the Relationships list on the main form.

**Root Cause**: The Calendar Table configuration was saved correctly and the Date table would be generated during build, but the UI wasn't updated to reflect the relationship.

**Fix**: 
- Modified `UpdateRelationshipsDisplay()` to check for `DateTableConfig` and display the relationship to the Date table
- Added call to `UpdateRelationshipsDisplay()` after the CalendarTableDialog returns successfully
- Date table relationship now appears at the top of the relationships list with:
  - From: `{PrimaryDateTable}.{PrimaryDateField}`
  - To: `Date Table 📅`
  - Type: `Active (Date)`
  - Color: Dark green for easy identification

## Technical Changes

###Files Modified
- **PluginControl.cs**:
  - `BtnCalendarTable_Click`: Added `UpdateRelationshipsDisplay()` call after saving configuration
  - `UpdateRelationshipsDisplay()`: Added logic to display Date table relationship if `DateTableConfig` is set

## How It Works Now

1. User clicks "Calendar Table" button and configures the date table
2. Configuration is saved to `PluginSettings.DateTableConfig`
3. **NEW**: Relationships list immediately updates to show the relationship to the Date table
4. The relationship displays as: `{SourceTable}.{DateField}` → `Date Table 📅` (Active Date)
5. When building the semantic model, the Date table is created and the relationship is applied

## Behavior Match with Configurator

This fix brings the XrmToolBox version into alignment with the Configurator app, which has always shown the Date table relationship in the UI after configuration.

## Build Information
- **Version**: v1.3.21.0
- **Build Status**: ✅ SUCCESS  
- **Warnings**: 39 (nullable reference type annotations - safe to ignore)
- **Errors**: 0
- **Output**: `bin\Release\DataverseToPowerBI.XrmToolBox.dll`

## Testing Recommendations
1. Configure a Calendar/Date table
2. Verify the relationship appears immediately in the Relationships list
3. Verify it shows with calendar emoji 📅 and green color
4. Load an existing model with DateTableConfig - verify relationship displays
5. Build semantic model - verify Date table is created correctly

## Known Issues
None

## Upgrade Impact
- No breaking changes
- Purely cosmetic fix - adds missing UI feedback
- Existing Date table configurations will now display correctly
