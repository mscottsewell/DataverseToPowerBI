# Troubleshooting

## Common Issues and Solutions

### "DataSource.Error after Power BI Desktop update" or "CommonDataService connector metadata failure"
**Cause:** Your model was built with the legacy `CommonDataService.Database` connector, which has progressive metadata management failures in recent Power BI Desktop releases.

**Solution:** Rebuild your model with the latest version of this tool (v1.2026.5.177+). The tool now uses the standard `Sql.Database` connector with `Value.NativeQuery`, which resolves these errors. This is a one-time migration — the change preview will show all queries being regenerated.

### "Report won't connect after moving to a different environment"
**Cause:** For TDS-mode reports, the `Sql.Database` connector requires both a server URL and a database name. If you manually moved the report, you likely only updated the URL.

**Solution:** In Power BI Desktop, go to **Transform Data → Parameters** and update **both**:
1. **DataverseURL** — the environment URL (e.g., `myorg.crm.dynamics.com`)
2. **DataverseUniqueDB** — the organization database name

The database name may differ from the URL subdomain (e.g., if the environment was renamed after provisioning). To find it:
- Connect to the TDS endpoint with **SQL Server Management Studio (SSMS)** — the database name appears in Object Explorer
- Check the **Organization unique name** in the [Power Platform admin center](https://learn.microsoft.com/power-platform/admin/determine-org-id-name#find-your-organization-name)

> **Note:** VS Code's SQL extension does not display the database name in the same way as SSMS.

📚 **References:**
- [Use SQL to query data (Dataverse TDS endpoint)](https://learn.microsoft.com/power-apps/developer/data-platform/dataverse-sql-query)
- [Find your organization name](https://learn.microsoft.com/power-platform/admin/determine-org-id-name#find-your-organization-name)

### "Cannot connect to Dataverse"
- Verify your XrmToolBox connection is active
- Check that you have read permissions in Dataverse
- Try disconnecting and reconnecting
- Ensure your access to the TDS endpoint is not blocked by either permissions inside dataverse or via network policy
- See more here: <https://learn.microsoft.com/power-apps/developer/data-platform/dataverse-sql-query>

### "Build failed with errors"
- Check the working folder path is valid and writable
- Ensure you have selected at least one table
- Review the error message for specific guidance

### "Power BI Desktop won't open the .pbip file"
- Ensure you have Power BI Desktop (November 2023 or later)
- Enable Developer Mode: File → Options → Preview features → Power BI Project files
- Check that all files in the folder exist and aren't corrupted

### "Missing relationships in the model"
- Verify your lookup columns were included in the selected form
- Check that both source and target tables are in the model
- Re-run the table selection and view the created tables

### "My report is slow—what can I do?"
Try these optimizations:
1. Reduce the number of columns (only include what you need)
2. Remove long text fields such as text area fields.
3. Use view more aggressive filters to limit rows to only those typically relevant to the user.
4. Switch dimension tables to Dual mode (Dual (All) or Dual (Select))
5. Simplify visuals (fewer visuals per page)
6. Use aggregations for large fact tables
7. For larger datasets, moving the fact table to Import Mode will make the interaction more responsive.
8. Filtering on large Date Ranges using a Direct Query source with the Date Table can cause timeouts based on the way the query is constructed. You may want to apply the query to the date field directly from within the query pane rather than relying on the Date Table.

### "Users see data they shouldn't see (DirectQuery TDS)"
**Cause:** Single Sign-On (SSO) is not enabled in Power BI Service

If users can see records they shouldn't have access to based on Dataverse security, or if "current user" view filters aren't working:

1. Go to [Power BI Service](https://app.powerbi.com)
2. Navigate to your workspace and find your semantic model
3. Click **...** → **Settings**
4. Expand **Data source credentials**
5. Click **Edit credentials**
6. **✅ REQUIRED:** Check "End users use their own OAuth2 credentials when accessing this data source via DirectQuery"
7. Click **Sign in** and authenticate

Without SSO enabled, the report uses a shared service account instead of each user's credentials, bypassing Dataverse security.

📚 **Learn More:** [Enable Single Sign-On for DirectQuery](https://learn.microsoft.com/power-bi/connect-data/service-azure-sql-database-with-direct-connect#single-sign-on)

### "Current user filters not applied (FabricLink mode)"
**Cause:** Current user operators are not supported in FabricLink

View filters using `eq-userid`, `ne-userid`, `eq-userteams`, or `ne-userteams` cannot be used with FabricLink connections. These filters are automatically skipped and logged in the FetchXML debug output.

**Solution:** Use DataverseTDS connection mode instead if you need row-level security based on current user context.

### "Multi-select choice labels missing or incorrect (FabricLink mode)"
If multi-select choice labels don’t resolve correctly in FabricLink, update to the latest release and rebuild the model. Label resolution should split values on semicolons (`;`) and use the attribute logical name for `OptionSetName`.

See [CHANGELOG.md](CHANGELOG.md) for current release details.
### "Retained/archived rows appearing in FabricLink reports"
**Cause:** The default Long Term Retention mode is "All", which includes both live and retained rows.

**Solution:** Set the retention mode to **Live** for tables where you only want active data. Click the retention mode indicator in the table list to cycle through All → Live → LTR. This adds a `WHERE (Base.msft_datastate = 2 OR Base.msft_datastate IS NULL)` predicate to filter out retained rows.

This only applies to FabricLink mode with tables that have [long term data retention](https://learn.microsoft.com/power-apps/maker/data-platform/data-retention-overview) enabled.

### "I only want to report on archived/retained data"
Set the retention mode to **LTR** for tables where you want only retained rows. This filters to `WHERE (Base.msft_datastate = 1)`. Useful for historical or compliance reporting on archived data.
### "Quick Select changed my filter from Selected to All"
Quick Select now preserves your current attribute filter mode.

- If you were in **Selected**, it remains **Selected** after applying pasted attributes.
- If you were in **All**, it remains **All**.
- Newly selected attributes are refreshed immediately in the current view.

If you still see mode switching, update to the latest release.

See [CHANGELOG.md](CHANGELOG.md) for current release details.

### "Quick-selected attributes are missing after reopening"
Selected attributes are saved in the semantic model configuration (`PluginSettings.SelectedAttributes`) and should reload on open.

Expected behavior:
- Manual selections (including Quick Select) persist across close/reopen.
- Metadata revalidation should not reset selected values to defaults.

Note:
- Attributes that no longer exist in Dataverse are removed during revalidation.
- Required locked fields (primary key/name and relationship-required lookup keys) are always included.

If selections appear to drop, update to the latest release.

See [CHANGELOG.md](CHANGELOG.md) for current release details.

### "What does the Default checkmark mean for expanded child rows?"
In View mode, expanded child rows in the **Default** column are now marked only when they come from the currently selected view's linked columns.

This means:
- Child rows added manually in Expand Lookup are still selected, but are not shown as **Default** unless they exactly match the selected view's linked column definition.
- Previously saved placeholder names for expanded fields are auto-normalized from metadata on load/revalidation.

### "In Selected view, why do I see unchecked child rows under a selected lookup?"
This is expected. In **Show: Selected** mode, selecting a grouped parent lookup now shows all of its child sub-rows (both included and excluded) so you can review and adjust configuration in one place.

Also expected behavior:
- Turning **Include** off for an expanded child row keeps the row visible but excludes it from model generation.
- **Hidden** implies **Include**. Enabling Hidden automatically enables Include.

### "Why are lookup groups collapsed when I reopen the model?"
Lookup groups are collapsed by default to reduce clutter, and your open/collapse state is persisted per model configuration.

Use:
- **Open all groups** to expand everything quickly
- **Collapse all groups** to reset the view
