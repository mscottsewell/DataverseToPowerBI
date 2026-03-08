# Changelog

All notable changes to the Dataverse to Power BI Semantic Model XrmToolBox plugin are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

---

## [1.2026.5.180] - 2026-03-07

### Added

- **SQL Native Query Mode (Sql.Database + Value.NativeQuery)** — **Breaking change:** All generated Power Query expressions now use the standard `Sql.Database` connector with `Value.NativeQuery(...)` instead of the legacy `CommonDataService.Database` connector. This is a fundamental architecture change that affects both connection modes:

  - **Why:** Recent Power BI Desktop releases introduced metadata management failures with the CommonDataService connector that caused reports to break after any model update or refresh. Moving to the standard SQL connector resolves these failures and provides a more stable foundation.
  - **How it works:** Each table partition now generates a Power Query `let...in` expression that connects via `Sql.Database` and passes the full SQL query through `Value.NativeQuery(Source, "<SQL>", null, [PreserveTypes = true, EnableFolding = true])`.
    - `PreserveTypes = true` ensures Power Query preserves SQL-returned data types without re-inference.
    - `EnableFolding = true` allows Power BI to fold additional query operations back into the native query for optimal performance.
  - **TDS mode:** `Source = Sql.Database(DataverseURL, DataverseUniqueDB)` — connects to the Dataverse TDS endpoint using the environment URL and the organization database name.
  - **FabricLink mode:** `Source = Sql.Database(FabricSQLEndpoint, FabricLakehouse)` — connects to the Fabric SQL endpoint with the lakehouse name.
  - **Existing reports:** Reports built with the old `CommonDataService.Database` connector will need to be rebuilt with this tool to migrate to the new connector. The change preview will show the connection type change and all queries will be regenerated.

- **DataverseUniqueDB Parameter** — A new hidden parameter table (`DataverseUniqueDB`) is generated for TDS-mode models. This stores the **organization unique name** — the TDS endpoint database name that the `Sql.Database` connector requires as its second argument.

  - **Automatic lookup:** When you connect to a Dataverse environment in XrmToolBox, the tool automatically queries the `organization` entity to retrieve the database name. This value is persisted in the model configuration so it only needs to be looked up once.
  - **Why it's needed:** The organization database name may differ from the environment URL subdomain (e.g., if the environment URL was renamed after provisioning, the database name retains the original provisioning name).
  - **Manual transfer:** If you manually copy or migrate a Power BI report to point to a different environment, you must update **both** the `DataverseURL` and `DataverseUniqueDB` parameter values. The DataverseURL is the environment URL (e.g., `myorg.crm.dynamics.com`), and DataverseUniqueDB is the organization database name.
  - **Finding the database name:** Connect to the TDS endpoint using SQL Server Management Studio (SSMS) — the database name is displayed in the Object Explorer. Note: connecting via VS Code's SQL extension does **not** show the database name in the same way.
  - 📚 **Reference:** [Use SQL to query data (Dataverse TDS endpoint)](https://learn.microsoft.com/power-apps/developer/data-platform/dataverse-sql-query) | [View the Organization unique name](https://learn.microsoft.com/power-platform/admin/determine-org-id-name#find-your-organization-name)
  - **FabricLink mode:** The `DataverseUniqueDB` table is not generated for FabricLink models — FabricLink uses `FabricSQLEndpoint` and `FabricLakehouse` expression parameters instead.

- **Fabric Link Long Term Retention (LTR) Data Support** — Per-table control over which rows are included from Fabric Link tables that have Long Term Retention data. When Dataverse data is synced to Fabric via Fabric Link, retained (soft-deleted or archived) rows are stored alongside live data with a `msft_datastate` system column indicating their status.

  - **Three modes per table:**
    | Mode | SQL Predicate | Description |
    |------|--------------|-------------|
    | **All** (default) | *(none — all rows returned)* | Returns both live and retained rows. This is the default and matches the behavior of querying the Fabric Lakehouse directly. |
    | **Live** | `WHERE (Base.msft_datastate = 2 OR Base.msft_datastate IS NULL)` | Only active/live rows. Filters out retained data. |
    | **LTR** | `WHERE (Base.msft_datastate = 1)` | Only long-term retained rows. Useful for historical/archival reporting. |

  - **Per-table configuration:** Each table can have its own retention mode. Click the retention mode indicator in the table list to cycle through All → Live → LTR. For example, you might set your fact table to "Live" (current data only) while setting an archive dimension to "LTR" (retained records only).
  - **Combined with view filters:** The retention predicate is ANDed with any existing view filter WHERE clause. If you have both a view filter and a retention mode, both conditions apply.
  - **FabricLink only:** This setting only applies to FabricLink connection mode. It is ignored for DataverseTDS connections (Dataverse TDS does not expose the `msft_datastate` column).
  - **Persistence:** Retention mode settings are saved per-table in the model configuration and restored on reload.
  - 📚 **Reference:** [Dataverse long term data retention overview](https://learn.microsoft.com/power-apps/maker/data-platform/data-retention-overview) | [Access retained data in Fabric](https://learn.microsoft.com/power-apps/maker/data-platform/data-retention-view)

- **Choice Sub-Column Configuration** — Per-choice-field control over which generated sub-columns (numeric value and display label) are included or hidden in the semantic model:

  - **Value field:** The raw integer/numeric value column (e.g., `statecode` = 0). Defaults to excluded unless the model-level "Include choice numeric values as hidden" toggle is enabled.
  - **Label field:** The human-readable display name column (e.g., `statecode` = "Active"). Included by default.
  - Per-choice Include/Hidden toggles appear in the attribute grid, matching the same pattern used for lookup sub-columns.
  - Value field display names use PascalCase `SchemaName` when available (e.g., `StatusCode` instead of `statuscode`), falling back to `LogicalName`.
  - Configurations persisted per semantic model and restored on load.

- **Per-Table Count & Record Link Measure Toggles** — Individual tables can now opt in or out of the auto-generated `{TableName} Count` (COUNTROWS) and `Link to {TableName}` (Dataverse URL) measures. Previously, these were only generated on the fact table. Now dimension tables can opt in and any table can opt out. The fact table still defaults to having both measures enabled.

### Changed

- **Connection Architecture (TDS)** — TDS-mode partition expressions changed from `CommonDataService.Database(DataverseURL)` to `Sql.Database(DataverseURL, DataverseUniqueDB)` with `Value.NativeQuery`. This is the most significant internal change in this release. The old connector had progressively worsening metadata management issues in recent Power BI Desktop releases, causing `DataSource.Error` failures after model updates. The new architecture uses the same standard SQL connector for both TDS and FabricLink modes, providing a consistent and stable query generation path.

- **Connection Architecture (FabricLink)** — FabricLink partition expressions now use `Value.NativeQuery(Source, "<SQL>", null, [PreserveTypes = true, EnableFolding = true])` wrapping the SQL query with explicit folding and type preservation options. Previously the SQL was passed as a `Query` record field to `Sql.Database`. The new pattern enables Power BI's query folding optimization.

- **Display Name Rename Option Restricted to Import Mode** — The "Rename columns to display names in Power Query" checkbox is now hidden and disabled when the storage mode is Direct Query or Dual. This option uses a `Table.RenameColumns` Power Query step that is only valid for Import mode; enabling it for DirectQuery or Dual previously caused Power Query errors. The checkbox is automatically shown when Import mode is selected and hidden otherwise.

### Fixed

- **CommonDataService Connector Metadata Failures** — Resolved the root cause of `DataSource.Error` and metadata management failures that occurred with recent Power BI Desktop releases when using the CommonDataService connector. The new `Sql.Database` + `Value.NativeQuery` architecture eliminates these errors entirely.

---

## [1.2026.5.165] - 2026-03-04

### Fixed

- **Geography Data Category Preservation** — Power BI column data categories (e.g. City, Country/Region, Latitude, Longitude) manually assigned in Power BI Desktop are no longer wiped on a model rebuild. The builder now reads back any `dataCategory` property from the existing TMDL and re-emits it on the regenerated column, so geographic heat-maps and map visuals retain their field bindings across builds.

---

## [1.2026.5.162] - 2026-03-03

### Fixed

- **Identifier/Lookup Display Name Stability** — Fixed a bug where display names of lookup fields and unique identifier fields would not update correctly after being edited inline in the attribute grid. This release ensures that any manual edits to display names are properly saved and reflected in the UI, and that reverting to default naming conventions works as expected without leaving stale overrides.

  - The issue was caused by a mismatch between the default naming logic used for display names and the inline edit behavior, which could result in stale override entries that prevented updates from being applied correctly. The fix involved aligning the default naming logic with the inline edit flow and ensuring that clearing an override properly reverts to the default naming convention.

- **Legacy Auto-Override Cleanup** — Removed legacy auto-generated primary-name overrides of the form `{TableDisplayName} {PrimaryNameDisplayName}` during metadata load/revalidation, preventing old saved configurations from reintroducing table-name-prefixed labels.

- **Inline Edit Default Reset Consistency** — Resetting an edited display alias now compares against the same default naming rule used by the grid, so clearing back to default properly removes the override for identifier/lookup fields.

---

## [1.2026.5.154] - 2026-03-02

### Added

- **Advanced Table Selection ("Add Tables to Model")** — A new **"Add Tables to Model…"** button in the star-schema wizard lets users include any Dataverse table that is not reachable through the standard lookup-chain discovery. Use cases include tables that share data via non-lookup relationships, cross-entity joins, or tables that simply live outside the selected solution.

  - **Full-environment table browser** — Lists every table known to the environment (solution tables with rich metadata unioned with all entity display names), filtered to exclude tables already in the star-schema.
  - **Search / filter** — Instant text search by display name or logical name.
  - **Manual relationship builder** — Define many-to-one joins between any two tables at the column level. Each relationship can include table/column/target/active configuration identical to auto-discovered relationships.
  - **Edit support** — Re-opening the wizard restores previously configured additional tables and relationships for modification.
  - **Counter badge** — The wizard shows a live count label (e.g., `+3 table(s), 2 rel(s)`) so users can see the pending selection at a glance.

- **Additional Table & Relationship Persistence** — Additional tables (logical names) and their manually-defined relationships are saved to the semantic model configuration (`AdditionalTableNames` / `AdditionalRelationships`) and fully restored on reload.

- **Relationship-Required Column Enforcement** — Lookup columns required by manually-defined additional relationships are automatically included (same enforcement already applied to auto-discovered relationships).

---

## [1.2026.5.146] - 2026-02-28

### Changed

- **Display Name Rename Execution Path** — Display-name renaming now executes in Power Query using `Table.RenameColumns` after the native query. SQL projections remain stable technical/logical names (no display-name `AS [..]` aliases).
- **Configuration Naming Clarification** — Internal setting/property naming now reflects Power Query behavior (`UseDisplayNameRenamesInPowerQuery`) while preserving compatibility with legacy persisted JSON key names.
- **UI Terminology Alignment** — Semantic model dialogs now label the option as "Rename columns to display names in Power Query" to match generated output.
- **CSV Summary Labeling** — Exported model summary now labels this setting as Power Query renaming rather than SQL aliases.

### Fixed

- **Semantic Model Setting Persistence** — Model save/copy paths now consistently carry rename toggle state, storage mode, and choice numeric visibility defaults across model updates and clones.
- **Legacy Model Compatibility** — Deserialization keeps rename behavior enabled by default when older model files omit the setting.
- **Display Name Override UX Consistency** — Duplicate detection and display-name override handling in attribute editing are now consistently applied without depending on SQL-alias terminology.

### Added

- **Serialization Compatibility Tests** — Added tests ensuring missing rename-setting payloads default to enabled, and explicit legacy JSON key values still deserialize correctly.
- **Build Output Validation for Rename Step** — Integration tests now assert `Table.RenameColumns` generation and verify display-name aliasing is no longer emitted in SQL projections.

---

## [1.2026.5.141] - 2026-02-24

### Added
- **Tests for Unique Column Lineage Tag Generation** — Added tests to ensure that unique lineage tags are generated for columns, preventing conflicts when multiple columns have the same logical name.

- **Tests for Expanded Lookup Handling** — Added tests to verify the correct handling of expanded lookup fields, including their inclusion, exclusion, and hidden states during semantic model generation.

## [1.2026.5.125] - 2026-02-24

### Added

- **Expanded Child In-Grid Controls** — Expanded lookup child rows now expose interactive `Include` and `Hidden` toggles directly in the main attribute grid (in addition to the Expand dialog). This enables rapid include/hide adjustments without reopening the picker dialog.

- **Bulk Group Expansion Controls** — Added `Open all groups` and `Collapse all groups` actions to the attribute pane for faster visual navigation of grouped lookup/choice sections.

### Changed

- **Selected View Group Visibility** — In `Show: Selected` mode, grouped lookup/choice child rows now remain visible under selected parent groups even when individual child fields are currently not included. This supports quick include-on-demand workflows.

- **Group Default State and Persistence** — Lookup/choice groups now default to collapsed when first shown, and per-group open/collapse state is persisted in semantic model settings and restored on reload.

- **Expanded Column UX** — The expand-action column now has a header (`Expanded`) and row labels now show item counts when present (`▶ Expand (n)`). Column widths were rebalanced (reduced `Type`, widened `Expanded`) to prevent truncation/wrapping.

- **Choice Value Include Default** — For choice fields, enabling `Include` on the numeric/value sub-column now auto-sets `Hidden = true` by default (still user-toggleable).

- **Expanded Include Semantics** — Unchecking `Include` on an expanded child now behaves as an exclusion toggle while keeping the row visible (unchecked) in the grid. It no longer immediately removes the row from the current display.

### Fixed

- **Expanded Field Hidden Propagation** — Hidden state for expanded child attributes is now serialized, cloned, filtered for CSV export model copies, and honored during semantic model generation.

- **Expanded Field Build Output Control** — Semantic model generation now respects expanded child `Include` state (excluded children are omitted from output) and `Hidden` state (included hidden children are emitted as hidden columns).

---

## [1.2026.5.110] - 2026-02-21

### Fixed

- **Description Preservation in TMDL** — `WriteTmdlFile` no longer strips `description:` properties from table and column TMDL files. The regex removal is now scoped to `relationships.tmdl` only, where Power BI Desktop (Feb 2026) rejects descriptions. User-authored descriptions on columns, tables, and measures now survive incremental regeneration.

- **Null Safety in IsLookupType** — `IsLookupType()` and `IsPolymorphicLookupType()` now use `string.Equals()` static method instead of instance `.Equals()` on a nullable parameter, preventing potential `NullReferenceException`.

### Changed

- **FetchXML Debug Logs Opt-In** — FetchXML conversion debug files (`FetchXML_Debug/` folder) are no longer written to the output folder on every build. Debug logging is now gated behind an opt-in `enableFetchXmlDebugLogs` constructor parameter (default: `false`). This prevents sensitive view filter data from persisting to disk and reduces unnecessary I/O.

- **Lineage Stability Across Display Name Renames** — Column lineage tags are now backed by a stable `DataverseToPowerBI_LogicalName` annotation. When a Dataverse display name changes, the lineage tag is preserved via the logical name fallback, preventing Power BI from treating renamed columns as new (which would break existing report visuals).

- **ExtractEnvironmentName Deduplicated** — Consolidated the duplicated `ExtractEnvironmentName()` method (previously in both `PluginControl.cs` and `SemanticModelBuilder.cs`) into a shared `UrlHelper` static class.

### Added

- **FetchXmlToSqlConverter C# Tests** — Added 28 unit tests covering all operator categories: basic comparison, null, string, date (timezone-adjusted), list (in/not-in), filter logic (AND/OR/nested), user context operators (TDS vs FabricLink vs Import mode), link-entity subqueries, and edge cases (empty XML, invalid XML, custom alias).

- **Incremental Update Integration Tests** — Added 12 integration tests for lineage preservation with logical name fallback, column metadata fallback, description survival, user measure preservation, and annotation preservation.

### Removed

- **PPTB (Power Platform Toolbox) Port** — Removed the experimental `DataverseToPowerBI.PPTB` TypeScript/React project. The port requires significant rework before it would be usable. Code is preserved in git history for potential future revival.

---

## [1.2026.5.101] - 2026-02-21

### Added

- **Lookup Sub-Column Configuration** — Per-lookup control over which generated sub-columns (ID/GUID, Name, Type, Yomi) are included or hidden in the semantic model. New `LookupSubColumnConfig` model with nullable boolean fields; null values fall through to smart defaults based on relationship status:
  - Lookups in a relationship default to: ID=Included+Hidden, Name=Excluded
  - Lookups not in a relationship default to: ID=Excluded, Name=Included
  - Owner/Customer polymorphic lookups additionally expose Type and Yomi sub-columns
  - Include/Hidden toggles appear as clickable `☑`/`☐` text checkboxes in the attribute list
  - Configurations persisted per semantic model and restored on load

- **Collapsible Lookup Groups** — Lookup attributes in the attribute list now render as expandable/collapsible groups with `▼`/`▶` toggle headers. Click the display name to collapse/expand sub-rows. Expanded lookup child rows appear nested under the group header.

- **Polymorphic Lookup Support (Owner/Customer)** — Full support for Owner and Customer polymorphic lookup types:
  - Type (`{lookup}type`) and Yomi (`{lookup}yominame`) sub-columns exposed as toggleable rows
  - Virtual sub-columns (e.g., `owneridname`, `owneridtype`, `owneridyominame`) automatically filtered from the main attribute list to prevent duplicate selection
  - Migration logic upgrades previously-selected polymorphic virtual columns into the new sub-column config system
  - TMDL generation, M query, and expected column calculations all updated to conditionally include/exclude each sub-column

- **Cross-Chain Ambiguity Detection** — The Fact/Dimension selector now detects when the same dimension table is reachable via multiple active relationship paths from different source tables (e.g., Contact as both a direct dimension and a snowflake off Account). Amber/orange highlighting distinguishes cross-chain conflicts from same-source conflicts (red). A warning dialog on Finish prompts the user to resolve ambiguous paths.

- **Source Table Column in Relationship Selector** — New "Source Table" column added to the Fact/Dimension selector ListView, making it clear which table each lookup originates from. Particularly useful when snowflake relationships introduce lookups from multiple source tables. Dialog widened from 900px to 980px to accommodate.

- **Dimension Chain Grouping** — Relationships in the Fact/Dimension selector are now grouped by dimension chain rather than just target table. Direct dimensions and their snowflake descendants form a single visual group, with items sorted by lineage order (direct → snowflake L1 → snowflake L2). Group headers reflect the chain structure.

- **Snowflake Table Tracking** — Snowflake dimension targets that aren't in the current solution are now tracked in a separate `_snowflakeAddedTables` list, enabling deeper snowflake chaining (Fact → Dim → Snowflake → Snowflake2). `FindTableByLogicalName()` searches both solution and snowflake-added tables.

- **Lookup Sub-Column Unit Tests** — 378 lines of new xUnit tests covering lookup sub-column configuration behavior in TMDL generation: ID include/exclude, Name include/exclude, polymorphic type/yomi columns, hidden flag propagation, and interaction with relationship-required columns.

### Changed

- **Attribute List Column Headers** — Renamed abbreviated "Incl" and "Hid" column headers to full "Include" and "Hidden" labels for clarity. Rebalanced column widths across the attribute grid (Default, Type, and Expand columns reduced proportionally to accommodate the wider labels).

- **Lookup Sub-Row Checkbox Cleanup** — The "Sel" checkbox column no longer renders empty, non-functional checkboxes on lookup sub-rows (sublookup child rows and expanded lookup child rows). Uses native Win32 `LVM_SETITEM` to selectively hide the state image on rows where selection is managed via the Include/Hidden toggles instead.

- **Expand Lookup Always Enabled** — Removed `FeatureFlags.EnableExpandLookup` experimental gate. The Expand Lookup feature is now always available for all Lookup, Owner, and Customer attribute types.

- **TMDL Lookup Column Generation Overhaul** — `GenerateTableTmdl()`, `GenerateMQuery()`, and `GenerateExpectedColumns()` refactored to use `ResolveLookupSubColumnFlags()` for consistent, config-driven lookup sub-column generation. Each sub-column (ID, Name, Type, Yomi) is conditionally included/excluded and marked hidden based on the per-lookup config. Replaces the previous behavior of always emitting both ID (hidden) and Name (visible) columns.

- **User-Added Relationship Marker** — Preserved user relationships in TMDL output now include a `/// User-added relationship (preserved by DataverseToPowerBI)` comment marker if not already present, making it clear which relationships were manually added vs. tool-generated.

- **Auto-Check Prevention on Re-Edit** — When re-opening the Fact/Dimension selector for an existing fact table, relationships are no longer auto-checked. Only first-time setup or switching to a different fact table triggers auto-selection, preventing the tool from re-adding relationships the user previously removed.

- **Select All / Deselect All Safety** — Select All and Deselect All now skip internal sub-rows (`__sublookup__` and `__expanded__` tags), preventing synthetic sub-row tags from being added to `_selectedAttributes`.

### Fixed

- **Relationship Lookup GUID Auto-Defaults** — When adding a dimension relationship (e.g., Case → Product via `productid`), the lookup's GUID/ID sub-column is now correctly defaulted to Include=true and Hidden=true. Previously, stale explicit `LookupSubColumnConfig` entries (created when toggling any sub-column before the relationship existed) could override the smart defaults, leaving the foreign key column excluded.

- **Relationship-Required Columns in Build Output** — `PrepareExportData()` now includes relationship-required lookup columns in the exported attribute list, matching the display behavior. Previously, if a lookup column was required by a relationship but not explicitly in `_selectedAttributes`, it could be omitted from the TMDL build output.

- **Relationship-Required Columns Auto-Selected** — Lookup columns required by relationships are now automatically added to `_selectedAttributes` during metadata load (alongside primary key and primary name attributes). Ensures consistent state between the UI display and the underlying selection model.

- **Relationship GUID Include Lock** — The Include checkbox on the GUID/ID sub-row is now read-only (grayed, click-ignored) when the parent lookup supports a defined relationship. The foreign key column cannot be accidentally excluded while the dimension table is part of the model; the Hidden toggle remains editable.

- **Cross-Chain Relationship Status Refresh** — Checking, unchecking, or double-clicking a relationship now refreshes the status display for ALL relationships sharing the same target table (including cross-chain items from different source tables), not just same-source items. Prevents stale status/highlighting when relationships from multiple chains point to the same dimension.

- **Polymorphic Virtual Column Filtering** — `GenerateTableTmdl()`, `GenerateMQuery()`, and `GenerateExpectedColumns()` now skip polymorphic virtual sub-columns (e.g., `owneridname`, `customeridtype`) that are handled by the parent lookup's sub-column config. Prevents duplicate columns in the generated output when both the virtual column and its parent lookup are selected.

---

## [1.2026.5.84] - 2026-02-20

### Fixed

- **Expanded Lookup SQL JOIN Table Name Resolution** — Expanded-lookup JOINs now always use the related table logical/schema name instead of display name, preventing invalid SQL when a selected view includes related-table columns.

---

## [1.2026.5.83] - 2026-02-20

### Added

- **View-Based Expanded Field Defaults (Strict Matching)** — The `Default` checkmark for expanded child rows now appears only when a child field matches the currently selected view's linked columns (lookup + target table + attribute). Manually edited expanded rows are no longer treated as view-defaults unless they exactly match the selected view definition.

- **Expanded Lookup Metadata Normalization on Load** — Existing saved expanded lookup settings are auto-upgraded during metadata load/revalidation:
  - Replaces legacy placeholder display names with resolved Dataverse display names
  - Refreshes target table display names
  - Refreshes attribute metadata (type/schema/targets/virtual attribute)
  - Persists upgraded values automatically so users do not need to reselect views

### Changed

- **Quick Select UX (Paste Attributes Dialog)** — Applying Quick Select now preserves the current attribute filter mode (`All` or `Selected`) instead of switching users to `All`.

- **Quick Select Refresh Behavior** — After applying quick-selected attributes, the attribute list refreshes in-place so newly selected attributes are immediately visible in the current mode.

- **Selection Persistence Reliability** — Selected attribute sets are now handled with case-insensitive comparison when restored/initialized, preventing saved selections from appearing unchecked after close/reopen due to casing differences.

### Fixed

- **Saved Selections Lost on Reopen (View Mode)** — Removed load/revalidation behavior that reapplied default field selection and could overwrite user-selected attributes. Revalidation now updates metadata/default indicators without resetting saved `Selected` values (except for valid cleanup of attributes that no longer exist and required locked fields).

---

## [1.2026.5.62] - 2026-02-19

### Fixed

- **FabricLink Expanded Multi-Select Label Resolution** — Expanded multi-select choice fields in FabricLink mode now use `OUTER APPLY` with `STRING_SPLIT` / `STRING_AGG` and metadata JOINs, matching the label resolution pattern used by regular multi-select fields. Previously, expanded multi-select fields referenced a non-existent `name` column from the joined table, which could return raw integer values instead of localized labels.

---

## [1.2026.5.61] - 2026-02-19

### Added

- **Expand Lookups (Denormalization)** — Pull attributes from related tables directly into a parent table's query via LEFT OUTER JOIN, without adding a full dimension table
  - Click ⊚ on any lookup attribute to open the Expand Lookup dialog
  - Select attributes from the related table's form; checked attributes appear as child rows under the lookup field
  - Type-aware SQL generation: lookups select the name column, choices/booleans use virtual name columns (TDS) or metadata JOINs (FabricLink), multi-select choices handled with the same pattern
  - Display names use "{TargetTable} : {AttributeDisplayName}" convention
  - Expanded lookup configurations are persisted per semantic model and restored on load
  - Allow zero selections to remove an expansion
  - Marked as experimental — LEFT OUTER JOINs may affect DirectQuery performance on large tables

- **CSV Documentation Export** — Export semantic model configuration as human-readable CSV files for review and audit
  - Generates Summary.csv, Tables.csv, Attributes.csv, Relationships.csv, and ExpandedLookups.csv
  - Per-table selection dialog — choose which tables to include in the documentation
  - RFC 4180 compliant CSV formatting
  - Export-only (not importable); JSON export/import remains for full round-trip configuration sharing

### Changed

- **Export Flow Redesign** — Export button now prompts for format (JSON or CSV) before proceeding
  - JSON: exports the complete model configuration directly (full schema, importable)
  - CSV: opens a table selection dialog, then exports filtered CSV documentation to a named subfolder
  - Replaces the previous single-format JSON-only export

---

## [1.2026.5.60] - 2026-02-19

### Fixed

- **FabricLink Multi-Select Choice Resolution** — Fixed metadata join behavior for multi-select choice fields in FabricLink SQL generation
  - Uses semicolon delimiter (`;`) in `STRING_SPLIT` for Dataverse multi-select values (instead of comma)
  - Uses the attribute logical name for `OptionSetName` in metadata joins (instead of the choice definition name)
  - Applied consistently to both generated SQL and query-comparison logic to prevent false change detection

---

## [1.2026.5.37] - 2026-02-16

### Fixed

- **TMDL Description Property Compatibility** — Fixed Power BI Desktop schema validation errors caused by `description` properties on relationships
  - Power BI Desktop (Feb 2026) rejects TMDL files containing description properties in relationship blocks
  - Added `SanitizeRelationshipBlock()` and `SanitizeRelationshipsTmdl()` methods to strip unsupported properties
  - Global TMDL sanitization in `WriteTmdlFile()` removes description properties from all generated files
  - Prevents "relationship schema validation failed" errors when opening PBIP projects

- **Ambiguous Relationship Path Resolution** — Automatically resolves conflicts when multiple active relationships exist between same source→target table pair
  - Added `ResolveAmbiguousRelationshipPaths()` method that marks extra active relationships as inactive
  - Only one active relationship allowed per source→target path (Power BI requirement)
  - Logs resolved conflicts to debug output for transparency
  - Prevents "ambiguous relationship path" errors in Power BI Desktop

- **Relationship Dialog Null Reference Crashes** — Fixed `NullReferenceException` when checking/unchecking relationships in Fact/Dimension selector
  - Added comprehensive null checks throughout `ListViewRelationships_ItemChecked` event handler
  - Added null guards to all LINQ queries accessing `_allRelationshipItems` collection
  - Fixed iteration safeguards in `UpdateItemStatus`, `FilterRelationships`, and `BtnFinish_Click`
  - Applied same fixes to snowflake dimension selector dialog (`SnowflakeDimensionForm`)

- **Relationship State Tracking** — Fixed relationship active/inactive state determination using incorrect ListView.Checked property
  - Added `IsChecked` property to `RelationshipTag` and `SnowflakeTag` classes
  - State now tracked in tag object instead of relying on ListView control state
  - Prevents race conditions when items are filtered out of view but still logically selected
  - Fixed conflict detection to use `IsChecked` from tag for accurate state determination

- **Out-of-Solution Relationship Filtering** — Fixed "Solution tables only" filter hiding already-selected relationships to tables outside solution
  - Filter now preserves relationships that are already checked/selected
  - Only hides unselected relationships to out-of-solution tables
  - Prevents accidental loss of snowflake dimension selections when toggling filter

- **Duplicate Partition Mode Declaration** — Fixed duplicate `mode:` line appearing in generated table partition TMDL
  - Removed redundant `sb.AppendLine($"\t\tmode: {GetPartitionMode(table.Role, table.LogicalName)}");` statement
  - TMDL now contains single partition mode declaration per table

- **Relationship Ambiguity User Prompts** — Added confirmation dialogs when selecting relationships that conflict with existing active relationships
  - User can choose to keep newly selected relationship active or existing one
  - Applied to both checkbox selection and double-click activation
  - Prevents silent relationship state changes that could break DAX measures

---

## [1.2026.5.24] - 2026-02-14

### Added

- **Storage Mode Support** — Full storage mode selection for semantic models: Direct Query, Dual - All Dimensions, Dual - Select Tables, and Import
  - **Direct Query**: All tables use `directQuery` partition mode (existing default behavior)
  - **Dual - All Dimensions**: Fact tables remain `directQuery`, all dimension tables use `dual` mode
  - **Dual - Select Tables**: Per-table storage mode overrides — individually toggle dimension tables between `directQuery` and `dual`
  - **Import**: All Dataverse/FabricLink tables use `import` mode; automatically deletes `cache.abf` when switching away from Import
  - Storage mode persisted per semantic model configuration
  - Auto-strips `current_user` view filters when appropriate (Import: always; Dual: on dimensions; DualSelect: on dual tables)

- **Per-Table Storage Mode Editing** — Mode column in Selected Tables list shows each table's storage mode
  - Always visible with values: "Direct Query", "Dual", or "Import"
  - Click a dimension table's Mode cell to toggle between Direct Query ↔ Dual
  - Automatically switches model to "Dual - Select Tables" mode when individual table modes are changed, with user notification
  - Fact tables and Import mode tables are read-only

- **Export/Import Configurations** — Export or import individual semantic model configurations as JSON files
  - Export saves selected model's full configuration (settings, table selections, storage modes) to a `.json` file
  - Import loads a configuration file, auto-renames if name conflicts, and reminds user to update folder paths
  - Enables sharing configurations between machines or team members

- **Incremental Update Preservation** — Comprehensive preservation of user customizations during model rebuilds
  - **LineageTag preservation**: Table, column, measure, expression, and relationship lineageTags preserved across updates — report visuals and refresh history stay connected
  - **User relationship preservation**: Relationships not matching Dataverse metadata detected and preserved with `/// User-added relationship` marker
  - **Column description preservation**: User-edited descriptions (those not matching the tool's `Source:` pattern) preserved; tool descriptions regenerated
  - **Column formatting preservation**: User changes to `formatString` and `summarizeBy` preserved when column data type is unchanged
  - **User annotation preservation**: Custom annotations on columns preserved; tool annotations (`SummarizationSetBy`, `UnderlyingDateTimeDataType`) regenerated
  - **Platform ID preservation**: `.platform` file logical IDs preserved during incremental updates

- **Table Rename Detection** — Detects table display name changes via `/// Source:` comment in TMDL files
  - LineageTags, user measures, and column metadata carried over from old file
  - Old file automatically deleted after successful migration

- **Date Relationship Dedup** — Stale date→Date.Date relationships automatically cleaned up when date field configuration changes
  - Prevents old date relationships from being preserved as "user-added"

- **Connection Type Change Warnings** — AnalyzeChanges now warns when connection type differs between existing model and new configuration (TDS↔FabricLink)

- **Redesigned Change Preview Dialog** — Complete redesign of the pre-build change preview
  - **Grouped TreeView**: Changes organized under ⚠ Warnings, 📋 Tables, 🔗 Relationships, 🔌 Data Sources
  - **Summary statistics bar**: Colored badges showing warning/new/updated/preserved counts at a glance
  - **Filter toggles**: Show/hide Warnings, New, Updates, Preserved items (Preserved hidden by default)
  - **Impact indicators**: Each change tagged as Safe, Additive, Moderate, or Destructive
  - **Detail pane**: Dark-themed Consolas panel showing before/after context, column lists, and contextual guidance
  - **Expand/collapse**: Table nodes expand to show column-level changes; preserved categories collapse by default
  - **Resizable dialog**: Proper anchoring and minimum size constraints

- **Vista Folder Picker** — Modern Windows Explorer-style folder selection dialogs
  - Replaced legacy `FolderBrowserDialog` with Vista `IFileOpenDialog` (COM interop, no external dependencies)
  - Full navigation, search, breadcrumb bar, and favorites pane
  - Applied to all 4 folder selection locations (Working Folder and PBIP Template in both Manager and New Model dialogs)

- **Attribute List Performance** — Pre-built sorted attribute cache during metadata loading
  - Attributes sorted once during refresh, cached for instant display when switching tables or filter modes
  - Uses `Items.AddRange()` batch insertion instead of individual adds
  - Added `_isLoading` guard to prevent `ItemChecked` events from corrupting selection state during list population

- **Unit Test Project** — 43 xUnit tests covering preservation logic
  - Tests for lineage tag parsing, column metadata extraction, relationship GUID preservation, user measure extraction, date relationship dedup, table rename detection, and auto-measure cleanup
  - TMDL fixture files for reproducible testing
  - `InternalsVisibleTo` for testability of 14 internal methods

- **Comprehensive README** — Major documentation update
  - New sections: Storage Modes, Model Configuration Management, Change Preview & Impact Analysis
  - Expanded FetchXML operator support table (20+ operators across 7 categories)
  - FabricLink user context limitation documented
  - New FAQs for config sharing and storage mode selection
  - Updated positioning and feature highlights

### Changed

- **Semantic Model Manager Dialog** — Widened from 935px to 1050px for better content display
  - Added Storage Mode column to model list (Name, Fact Table, Tables, Mode, Connection, Last Used)
  - Storage Mode moved above Connection Type in details panel
  - Last Used column now shows date-only (was date+time) to prevent truncation
  - Column widths optimized to fit within list pane without horizontal scrolling

- **Selected Tables Column Widths** — Adjusted for better proportions: Table=90, Mode=90, Form=90, Filter=100, Attrs=30
  - Fixed `ResizeTableColumns()` resize handler to respect new proportions (was overriding Designer widths)
  - Mode column included in proportional resize calculations

- **Storage Mode Comparison** — Added `NormalizeStorageMode()` to treat "Dual" and "DualSelect" as equivalent
  - Prevents false storage mode change warnings when rebuilding without changes

- **Boolean Field Query Comparison** — `GenerateMQuery` now uses `GlobalOptionsetMetadata` for Boolean fields
  - Matches `GenerateTableTmdl` behavior; eliminates false "query structure changed" warnings

### Fixed

- **Relationship Active/Inactive Normalization** — Fixed unchecked relationships incorrectly showing "Active" status
  - When unchecking a relationship, Active/Inactive states are now normalized for that dimension
  - If exactly one relationship to a dimension remains checked, it's automatically set to Active and all others to Inactive
  - If zero relationships remain checked, all are marked Active (ready for user to select whichever one they want)
  - If multiple remain checked, first one is automatically set to Active and others to Inactive (user can double-click another to change which is Active)
  - Double-clicking any relationship toggles it to Active and automatically marks all others to that dimension as Inactive
  - Applied to both main Fact/Dimension selector and "Add Parent Tables" snowflake dialog

- **Collection Modification Exception** — Fixed crash when reopening relationship dialog after filtering by solution tables
  - Added safety checks to `FilterRelationships()` to prevent manipulation during form disposal
  - Added try-catch protection and null/disposed checks before manipulating ListView items
  - Prevents `InvalidOperationException: Collection was modified` errors

- **Relationship Conflict Highlighting Bug** — Fixed red highlighting appearing on unchecked relationships
  - `UpdateItemStatus` now only counts checked relationships when detecting conflicts
  - Unchecked relationships are ignored for conflict detection even if they have `IsActive = true`
  - Applied to both main Fact/Dimension selector and "Add Parent Tables" snowflake dialog

- **DATEDIFF Bug in FetchXML Converter** — Fixed `older-than-x-*` operators passing integer where date expression was expected
  - `DATEDIFF(day, column, 30)` → `DATEDIFF(day, column, GETDATE()) > 30`

- **Date Table Relationship Display Names** — Date table relationships now honor user display name overrides
  - Previously used logical name even when display name alias was configured

- **Font GDI Resource Leaks** — Cached Font objects in `PluginControl` and `TmdlPreviewDialog` to prevent GDI handle exhaustion
  - Added proper `Dispose` overrides to clean up cached fonts

- **False Storage Mode Warning** — Fixed spurious "Changing from Dual to DualSelect" warning on immediate rebuild
  - Both modes produce identical TMDL output and are now treated as equivalent

- **False Query Change Warning** — Fixed spurious "query structure changed" for tables with Boolean fields in FabricLink mode
  - `GenerateMQuery` now correctly uses `GlobalOptionsetMetadata` for Boolean attributes

---

## [1.2026.5.1] - 2026-02-12

### Added

- **Visual Grouping for Multiple Relationships** — Dimensions with multiple relationships are now visually grouped in the relationship selector
  - Group headers display dimension name with "(Multiple Relationships)" label
  - Only tables with 2+ relationships are grouped; single relationships remain ungrouped for clarity
  - Applied to both main Fact/Dimension selector dialog and "Add Parent Tables" snowflake dialog
  - Makes it easier to identify which relationships belong to the same dimension

- **Solution Tables Filter** — Added "Solution tables only" checkbox next to Solution dropdown
  - Enabled by default to show only relationships to tables in the current solution
  - Unchecking shows relationships to all tables (including those outside the solution)
  - Works seamlessly with search filter
  - Reduces clutter when working with specific solutions

- **Relationship Search** — Added search box to filter relationships by field names and dimension table names
  - Real-time filtering as you type
  - Searches across: Lookup Field name, Lookup Logical Name, Target Table name, Target Logical Name
  - Validation still checks all relationships (not just filtered ones)
  - Preserves checked state when filtering

### Changed

- **Default Relationship Status** — All relationships now default to "Active" status
  - Previous behavior: Multi-relationship dimensions defaulted to "Inactive"
  - New behavior: All relationships start as "Active" for clearer initial state
  - Users explicitly choose which relationship to make inactive when multiple exist

- **Smart Relationship Selection** — Enhanced automatic inactivation when selecting relationships
  - Checking a relationship automatically marks ALL other relationships to that same dimension as "Inactive"
  - Applied whether the other relationships are checked or not
  - Double-clicking a relationship to make it Active automatically inactivates all others to that dimension
  - Prevents conflicts and ensures only one active relationship per dimension

- **Column Width Optimization** — Adjusted column widths for better data visibility
  - **Main grid (Selected Tables & Forms)**: Type column increased from 100px to 140px; Filter column now gets 50% of flexible space (up from 33.33%)
  - **Main grid (Attributes)**: Type column increased from 100px to 140px to prevent truncation of "Uniqueidentifier" and other long type names
  - **Fact/Dimension Selector**: Reduced width from 1050px to 900px (150px narrower); column widths adjusted to fit
  - **Add Parent Tables dialog**: Lookup Field and Parent Table columns reduced to 150px (75% of original 200px) to fit Logical Name column without scrolling

- **Dialog Layout Improvements** — Reorganized Fact/Dimension selector for better usability
  - "Include one-to-many relationships" checkbox moved to left (first position)
  - Search box moved to far right, aligned with grid's right edge
  - Finish and Cancel buttons repositioned to remain fully visible at new dialog width

- **Improved Relationship Conflict Highlighting** — Enhanced visual indicators for relationship status
  - Type column now displays "(Inactive)" suffix for inactive relationships (e.g., "Direct (Inactive)")
  - Red highlighting (light salmon) now only appears when multiple ACTIVE relationships exist to the same dimension (conflict state)
  - Inactive relationships no longer highlighted — clean white background for clarity
  - Applied to both main Fact/Dimension selector and "Add Parent Tables" snowflake dialog
  - Makes it immediately clear which relationships have conflicts vs. which are safely inactive

### Fixed

- **NullReferenceException in ItemChecked Event** — Fixed crash when opening Fact/Dimension selector dialog
  - Added `_suppressItemCheckedEvent` flag to prevent events during list manipulation
  - Added comprehensive null checks in all event handlers (`ItemChecked`, `DoubleClick`, `UpdateItemStatus`, `UpdateSnowflakeButtonState`)
  - Protected LINQ queries with `.Where(i => i.Tag != null)` filters
  - Fixed iteration to use `_allRelationshipItems` instead of `listViewRelationships.Items` for complete coverage
  - Applied same safety features to snowflake "Add Parent Tables" dialog

- **FabricLink Current User Filter** — FetchXML filters with current user operators now handled correctly for FabricLink
  - User context operators (`eq-userid`, `ne-userid`, `eq-userteams`, `ne-userteams`) are now skipped in FabricLink mode
  - These operators continue to work with DataverseTDS connection mode
  - Skipped operators are logged as "not supported in FabricLink (use TDS for current user filters)"
  - Prevents SQL syntax errors in Direct Lake scenarios that don't support `CURRENT_USER` constructs
  - Debug logs clearly indicate which operators were skipped due to FabricLink limitations

---

## [1.2026.4.16] - 2026-02-10

### Added

- **TMDL Preview Icon** — Preview TMDL toolbar button now displays with a dedicated preview icon for better visual identification
  - Icon image: `TMDLPreviewIcon.png` loaded via `RibbonIcons.PreviewIcon`
  - All ribbon toolbar buttons now have consistent icon styling

- **Virtual Column Name Corrections** — Table-scoped correction dictionary for problematic virtual columns that don't exist in TDS endpoint
  - Format: `"tablename.incorrectcolumnname" → "correctcolumnname"`
  - Example: `"contact.donotsendmmname" → "donotsendmarketingmaterial"`
  - Prevents SQL errors from bad metadata by applying corrections automatically
  - Applied across all three generation methods (TMDL export, change analysis, comparison queries)
  - Add new corrections to `VirtualColumnCorrections` dictionary in `SemanticModelBuilder.cs`

- **Enhanced Column Descriptions** — TMDL column descriptions now include comprehensive metadata
  - **Dataverse Description**: User-provided description from attribute metadata (if available) shown first
  - **Source Attribution**: `Source: {tableLogicalName}.{attributeLogicalName}` format for clear data lineage
  - **Lookup Targets**: Target table names for lookup/polymorphic lookup columns
  - Example: `"The primary contact for the account | Source: account.primarycontactid | Targets: contact"`
  - Applied to all column types (regular, lookup, choice/boolean, multi-select, primary keys, relationship-required lookups)
  - `Description` property added to `AttributeMetadata` model, populated from XRM SDK metadata
  - `BuildDescription()` method refactored to accept table/attribute names and Dataverse description

### Changed

- **TMDL Preview Sort Order** — Reordered preview list for better usability and logical flow
  - **New order**: Fact Tables → Dimension Tables → Date Table → Expressions (alphabetically within each category)
  - **Previous order**: Expressions → Date Table → Fact Table → Dimension Tables
  - `TmdlEntryType` enum values changed: `FactTable=0, DimensionTable=1, DateTable=2, Expression=3`
  - More intuitive for users to find their primary data tables first

- **TMDL Preview SQL Formatting** — Fixed line breaks in Windows Forms TextBox display
  - Replaced all embedded `\n` (LF) with `\r\n` (CRLF) in SQL SELECT list, JOIN clauses, and OUTER APPLY subqueries
  - Windows Forms TextBox requires CRLF for proper line rendering (LF-only appears on same line)
  - Affected areas: SELECT field continuation, State/Status/Boolean/Picklist JOINs, multi-select OUTER APPLY
  - Fixed: `FROM {table} as Base` → `FROM {table} AS Base` (uppercase AS for consistency)

- **UseDisplayNameAliasesInSql Default Handling** — Added `[OnDeserializing]` callback to ensure correct default value
  - `DataContractJsonSerializer` bypasses constructors and property initializers
  - Models saved before the aliasing feature existed were loading with `false` instead of declared `true` default
  - `SetDefaults()` method in `SemanticModelConfig` sets `UseDisplayNameAliasesInSql = true` before deserialization

### Fixed

- **PrepareExportData Bug Fixes** — Critical metadata propagation issues resolved
  - `HasStateCode` now correctly set on `ExportTable` by checking if `_tableAttributes` contains `statecode` attribute
  - `IsGlobal` and `OptionSetName` now copied from `AttributeMetadata` to `AttributeDisplayInfo`
  - Prevents missing WHERE clause in SQL queries (when `HasStateCode` was false incorrectly)
  - Ensures FabricLink JOINs use correct metadata table (`GlobalOptionsetMetadata` vs `OptionsetMetadata`)

- **Build Warnings Eliminated** — Removed all NuGet package conflicts and nullable reference warnings
  - Fixed 2× MSB3277 assembly version conflicts (System.ValueTuple, System.Text.Json)
  - Fixed 6× CS8618 non-nullable field warnings in `TmdlPreviewDialog.cs` (added `= null!` initialization)
  - Removed `System.ValueTuple` NuGet package (built into .NET Framework 4.8)
  - Upgraded `System.Text.Json` from 8.0.0 to 8.0.5 (then removed entirely as unused)
  - Removed ALL 17 unused NuGet packages left from deleted `SqlQueryValidator` (Azure.Identity, Azure.Core, Microsoft.Data.SqlClient, etc.)
  - Emptied `packages.config` and deleted all package folders
  - **Build now succeeds with 0 warnings and 0 errors**

---

## [1.2026.4.10] - 2026-02-10

### Added

- **TMDL Preview Feature** — Replaced "View SQL" with "Preview TMDL" showing the exact TMDL statements that will be written to the semantic model
  - Displays sorted preview list with automatic categorization: Expressions → Date Table → Fact Table → Dimensions (alphabetically)
  - **Connection Mode Awareness**: 
    - TDS mode: Shows "DataverseURL" parameter table as Expression type
    - FabricLink mode: Shows both "Expressions" (FabricSQLEndpoint + FabricLakehouse parameters) and "DataverseURL" table
  - **Date Table Integration**: Date table entry appears automatically if configured, positioned after Expressions and before data tables; omitted if date table is not configured
  - **Preview-to-Build Consistency**: Output is identical to actual build — uses the real `SemanticModelBuilder.GenerateTmdlPreview()` API instead of divergent inline generation
  - Visual distinction by entry type: italic blue text for Config/Date entries, bold for Fact table, normal for Dimensions
  - Type labels in second column: "Config", "Date", "Fact", "Dimension"
  - **Export Capabilities**:
    - Copy selected TMDL to clipboard with one click
    - Save individual `.tmdl` file with sanitized filename based on entry name
    - Save All button exports all entries as separate `.tmdl` files to chosen folder
  - Full TMDL content with UTF-8 encoding (no BOM): complete table declarations, column definitions with data types, measures, hierarchies, and M partition expressions

- **Display Name Aliases in SQL** — Columns can now use display names as SQL aliases (`AS [Display Name]`) for human-readable column names in the semantic model
  - **Per-model toggle**: "Use Display Name Aliases in SQL" checkbox in Semantic Model Settings (enabled by default for new models)
  - **Per-attribute override**: Double-click the Display Name column in the attributes list to set a custom alias for any column; setting it back to the original removes the override
  - **Auto-override for primary name attributes**: Primary name columns automatically get "{Table Name} {Column Name}" aliases to avoid duplicates across tables (e.g., "Account Name" instead of "Name")
  - **Duplicate detection**: Conflicting display name aliases are highlighted in light red in the attributes list; build and preview are blocked with a descriptive error listing all conflicts
  - **Override indicators**: Overridden display names show an asterisk (*) suffix in the attributes list
  - **Full TMDL integration**: Aliases are applied consistently across `GenerateTableTmdl`, `GenerateMQuery`, and `GenerateExpectedColumns` for all column types (regular, lookup, choice/boolean, multi-select, datetime-wrapped)
  - **Settings persistence**: Overrides are saved per-table and restored when loading a semantic model

### Changed

- **SQL Preview → TMDL Preview** — Renamed toolbar button from "Validate SQL" (btnValidateSql) to "Preview TMDL" (btnPreviewTmdl)
  - Shows actual TMDL output that matches the build output exactly
  - No longer shows divergent inline SQL generation (eliminated ~200 lines of out-of-sync code)
  - Uses the real `SemanticModelBuilder.GenerateTmdlPreview()` API instead of duplicated preview logic

- **SemanticModelBuilder Refactoring** — Extracted reusable components for preview generation
  - Added public `GenerateTmdlPreview()` method to generate full TMDL preview with all entry types
  - Extracted private `GenerateDataverseUrlTableTmdl()` method to decouple content generation from file I/O
  - `WriteDataverseUrlTable()` now calls the extracted method for content generation

- **Build Pipeline Optimization** — Extracted `PrepareExportData()` helper in PluginControl
  - Shared method builds `exportTables`, `exportRelationships`, and `attributeDisplayInfo` structures
  - Used by both incremental build (`BuildSemanticModel`) and TMDL preview flows
  - Eliminates code duplication for export data preparation

### Removed

- **Obsolete SQL Validation Infrastructure** — Removed all dependencies and code related to the old SQL validation feature:
  - Deleted `SqlQueryValidator.cs` service class
  - Deleted `SqlQueryValidationDialog.cs` form (replaced by `TmdlPreviewDialog.cs`)
  - Removed NuGet package references: Azure.Identity, Azure.Core, Microsoft.Data.SqlClient, Microsoft.Identity.Client, Microsoft.IdentityModel.Abstractions, System.Memory.Data, Microsoft.Identity.Client.Extensions.Msal
  - Deleted entire `packages/` folder containing NuGet assemblies
  - Reduced project bloat by ~350 MB of unused dependencies

---

## [1.2026.3.10] - 2026-02-08

### Added

- **Pre-Build Integrity Validation** — Automatic detection of incomplete PBIP structures before incremental builds
  - Validates presence of critical files: `.pbip`, `.platform`, `definition.pbism`, `model.tmdl`, `tables/` folder, Report folder
  - If structural files are missing, automatically performs full rebuild instead of failing on incremental update
  - Adds Warning-type changes to preview dialog showing each missing element
  - Prevents `DirectoryNotFoundException` and other build failures from corrupted project folders

- **Relationship-Required Column Locking** — Lookup fields used by active relationships are now automatically locked in the attribute selection UI
  - Lookup columns required for dimension relationships cannot be unchecked while the target dimension is in the model
  - Shows 🔒 icon with blue/bold styling for locked relationship columns
  - Survives metadata refresh and deselect-all operations
  - Prevents relationship breakage from accidental column deselection

- **FabricLink Connection Mode** — Full support for building semantic models that query Dataverse data via Microsoft Fabric Link (Lakehouse SQL endpoint)
  - FabricLink queries use `Sql.Database(FabricSQLEndpoint, FabricLakehouse, [Query="..."])` connector
  - Automatic JOINs to `OptionsetMetadata`, `GlobalOptionsetMetadata`, and `StatusMetadata` tables for human-readable choice/status labels
  - Separate handling of global vs. entity-specific optionsets
  - `INNER JOIN` for state/status metadata; `LEFT OUTER JOIN` for optional choice fields
  - Connection parameters stored as expressions in `expressions.tmdl`

- **Auto-Generated Fact Table Measures** — Two starter measures are automatically created on the fact table:
  - `{TableName} Count` — `COUNTROWS` for quick record counts
  - `Link to {TableName}` — Clickable URL to open records in Dataverse using `WEBURL` DAX function
  - Auto-generated measures are excluded from user measure preservation (they regenerate on each build)

- **Virtual Attribute Support** — Picklist/Boolean attributes now use actual virtual attribute names from metadata instead of assuming `{attributename}name` pattern
  - Handles edge cases like `donotsendmm` → `donotsendmarketingmaterial` (not `donotsendmmname`)
  - Global vs. entity-specific optionset detection for correct FabricLink metadata table JOINs

- **Language Code Parameter** — Builder now accepts a `languageCode` parameter (default 1033/English) for localizing metadata labels in FabricLink queries

### Changed

- **DataverseURL Architecture (TDS)** — DataverseURL is now stored as a hidden parameter *table* with `mode: import` and `IsParameterQuery=true` metadata (Enable Load pattern) instead of an expression
  - Resolves `KeyNotFoundException` that occurred during `CommonDataService.Database` refresh when DataverseURL was an expression
  - The parameter table pattern matches how Power BI Desktop natively handles Power Query parameters

- **DataverseURL Architecture (FabricLink)** — DataverseURL is now a hidden parameter *table* (not an expression in `expressions.tmdl`)
  - Both TDS and FabricLink now use identical DataverseURL table pattern for consistency
  - FabricLink `expressions.tmdl` contains only `FabricSQLEndpoint` and `FabricLakehouse` (DataverseURL moved to table)
  - Enables DAX measure references to work correctly in both connection modes

- **DAX Measure Table References** — Auto-generated measures now always use single-quoted table names: `'{displayName}'` instead of conditional quoting
  - Ensures consistent DAX syntax regardless of table name characters
  - Prevents syntax errors in measures when table names contain spaces or special characters

- **Money/Decimal Data Type Mapping** — Both TDS and FabricLink now map `money` and `decimal` types to `double` (matching Power BI Desktop's runtime behavior) to eliminate false "dataType changed" notifications on incremental rebuilds

- **Status/State Metadata JOINs (FabricLink)** — `statecode` uses `Base.statecode` (not `Base.statecodename`) and JOINs on `State` column (not `Option`); `statuscode` JOINs on both `State` and `Option` columns for correct label resolution

- **Stale Artifact Cleanup** — When switching between TDS and FabricLink modes, the builder now removes stale artifacts from the previous mode (e.g., removes `expressions.tmdl` when building TDS, removes `DataverseURL.tmdl` when building FabricLink)

### Fixed

- **Duplicate Column Generation** — Fixed bug where selecting both a lookup/choice ID field AND its virtual name column caused duplicate columns in TMDL
  - Example: Selecting `pbi_reportedbyid` and `pbi_reportedbyidname` no longer generates `pbi_reportedbyidname` twice
  - Applies to lookup name columns, choice/boolean label columns, and multi-select choice columns
  - All three generation methods (`GenerateTableTmdl`, `GenerateExpectedColumns`, `GenerateMQuery`) now check if synthesized name columns already exist before adding them
  - Prevents SQL query errors from duplicate column names in SELECT list

- Fixed `DataSource.Error` caused by virtual attribute name mismatch (e.g., `donotsendmmname` did not exist — actual name is `donotsendmarketingmaterial`)
- Fixed `KeyNotFoundException` during TDS model refresh by replacing expression-based DataverseURL with parameter table pattern
- Fixed false change detection on incremental rebuild where `double` columns were regenerated as `decimal`
- Fixed FabricLink `statuscode` label resolution to use compound JOIN on both `State + Option` columns

---

## [1.2026.2.47] - 2026-02-04

### Fixed
- Patched a bug that arose when a relationship was tied to a table outside the solution,
and when the Date table referenced a date attribute that wasn't 'selected' in the list of 
Attributes.


## [1.3.22] - 2026-01-31

### Added

- Date table now displays in the "Selected Tables & Forms" list after configuration
- Calendar icon (📅) and green styling for easy identification
- Year range shown in Filter column (e.g., "2020-2030")
- `AddDateTableToDisplay()` method that mirrors the Configurator's implementation

### Changed

- Date table entry positioned after Fact table for logical grouping
- UI refresh flow updated to show Date table when loading existing models

---

## [1.3.21] - 2026-01-31

### Fixed

- Date table relationship now displays in Relationships list after configuration
- `UpdateRelationshipsDisplay()` called after CalendarTableDialog returns successfully
- Relationship shows as: `{SourceTable}.{DateField}` → `Date Table 📅` (Active Date)

---

## [1.3.20] - 2025-01-20

### Added

- **Calendar/Date Table Configuration Dialog** (698 lines) with:
  - Primary date field selection (combo box with table + field selection)
  - Full Windows timezone support with UTC offset display
  - Year range configuration (start/end year with numeric controls)
  - Additional DateTime field wrapping across all selected tables
  - Multi-select checkbox list for DateTime field preview
  - Select All / Clear All buttons

- **Configuration Persistence** - DateTableConfig saved in PluginSettings

### Changed

- **Unified Type System**:
  - Removed duplicate `DateTableConfig` class from `XrmToolBox.Models`
  - Now uses `Core.Models.DateTableConfig` throughout the codebase
  - Added type aliasing with `using XrmModels = DataverseToPowerBI.XrmToolBox.Models`

- **Build Integration**:
  - `BuildSemanticModel` now passes `dateTableConfig` to semantic model builder
  - `AnalyzeChanges` and `ApplyChanges` accept DateTableConfig parameter

### Fixed

- `string.Contains()` overload compatibility with .NET Framework 4.8
- Type conversion errors between Core.Models and XrmToolBox.Models

---

## [1.3.0] - 2025-01-15

### Added

- Initial XrmToolBox plugin release
- Star-schema configuration with fact and dimension tables
- Form and view selection for column filtering
- FetchXML to SQL WHERE clause conversion
- TMDL generation for Power BI semantic models
- Relationship auto-detection from lookup fields
- Incremental update support with user measure preservation

---
