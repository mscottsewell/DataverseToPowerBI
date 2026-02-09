# Troubleshooting

## Common Issues and Solutions

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
4. Switch dimension tables to Dual mode
5. Simplify visuals (fewer visuals per page)
6. Use aggregations for large fact tables
7. For larger datasets, moving the fact table to Import Mode will make the interaction more responsive.
8. Filtering on large Date Ranges using a Direct Query source with the Date Table can cause timeouts based on the way the query is constructed. You may want to apply the query to the date field directly from within the query pane rather than relying on the Date Table.
