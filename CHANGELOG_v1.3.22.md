# v1.3.22 Release Notes - Date Table in Tables List

## Release Date: January 31, 2026

## Summary
This release adds the missing Date table display in the tables list after configuring the Calendar/Date table, completing full UI parity with the Configurator app.

## What Was Fixed

### Date Table Display in Tables List
**Issue**: After configuring the Calendar/Date table, the Date table wasn't appearing in the "Selected Tables & Forms" list (only the relationship was showing).

**Root Cause**: The Calendar Table configuration was saved and the relationship was displayed, but the Date table itself wasn't added to the tables list UI.

**Fix**: 
- Added `AddDateTableToDisplay()` method that mirrors the Configurator's implementation
- Inserts the Date table entry with special styling:
  - **Icon**: 📅 (calendar emoji)
  - **Role**: "Dim" (dimension)
  - **Name**: "Date Table"
  - **Filter column**: Year range (e.g., "2020-2030")
  - **Attrs column**: "365+" (estimated row count)
  - **Color**: Dark green for easy identification
  - **Position**: After the Fact table (if present), otherwise at the top

- Integrated into UI refresh flow:
  - Called after Calendar Table dialog succeeds
  - Called from `UpdateTableCount()` to ensure it appears when loading existing models
  - Automatically removes old entry before adding new one (prevents duplicates)

## How It Works Now

1. User configures Calendar/Date table
2. Configuration is saved
3. **NEW**: Date table entry appears in "Selected Tables & Forms" list with calendar icon 📅
4. **NEW**: Relationship to Date table appears in "Relationships" list
5. When building, the Date table is created with the configured year range and timezone adjustments

## Visual Indicators

The Date table is easy to identify:
- **Calendar icon** (📅) in the edit column
- **Green text** for the entire row
- **Year range** displayed in the Filter column (e.g., "2020-2030")
- Positioned prominently near the Fact table

## Technical Changes

### Files Modified
- **PluginControl.cs**:
  - Added `AddDateTableToDisplay()` method (35 lines)
    - Removes existing Date table entry if present
    - Creates ListViewItem with calendar icon and special formatting
    - Inserts at strategic position (after Fact table)
  - `BtnCalendarTable_Click()`: Added call to `AddDateTableToDisplay()` after saving config
  - `UpdateTableCount()`: Added call to `AddDateTableToDisplay()` to ensure display on model load

### Behavior Match with Configurator

This fix achieves complete UI parity with the Configurator app:
- ✅ Date table appears in tables list
- ✅ Date table relationship appears in relationships list
- ✅ Calendar icon and green color for easy identification
- ✅ Year range displayed for context
- ✅ Positioned logically (after Fact table)

## Build Information
- **Version**: v1.3.22.0
- **Build Status**: ✅ SUCCESS
- **Warnings**: 39 (nullable reference type annotations - safe to ignore)
- **Errors**: 0
- **Output**: `bin\Release\DataverseToPowerBI.XrmToolBox.dll`

## Testing Recommendations
1. Configure a Calendar/Date table
2. Verify Date table entry appears immediately in tables list with:
   - Calendar icon 📅
- Green text
   - Year range in Filter column
   - "365+" in Attrs column
3. Verify Date table relationship appears in relationships list
4. Load an existing model with DateTableConfig - verify both displays
5. Configure Date table again - verify old entry is replaced (no duplicates)
6. Build semantic model - verify Date table is created correctly

## Comparison: Before vs After

**Before v1.3.22**:
- Tables list: Only shows user-selected Dataverse tables
- Relationships list: Shows Date table relationship ✅
- User confusion: "Where's the Date table?"

**After v1.3.22**:
- Tables list: Shows Date table 📅 with green color and year range ✅
- Relationships list: Shows Date table relationship ✅
- Clear visual feedback: User immediately sees Date table is configured

## Known Issues
None

## Upgrade Impact
- No breaking changes
- Purely visual enhancement - adds missing UI feedback
- Existing Date table configurations will now display correctly in tables list
