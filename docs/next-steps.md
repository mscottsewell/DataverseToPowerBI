# Suggested Next Steps

Once you've built and opened your model in Power BI Desktop, here are recommended enhancements:

## 1. Create Key Measures
The tool auto-generates a record count and URL link measure on your fact table. Build on those with your own business metrics:
```dax
Total Revenue = SUM(Orders[Amount])
Average Order Value = DIVIDE([Total Revenue], [Order Count])
Win Rate = DIVIDE(
    COUNTROWS(FILTER(Opportunities, Opportunities[Status] = "Won")),
    COUNTROWS(Opportunities)
)
```

## 2. Build Hierarchies
Create drill-down paths in your dimension tables:
- **Date:** Year → Quarter → Month → Week → Day
- **Geography:** Country → State/Province → City
- **Organization:** Business Unit → Team → Owner

## 3. Apply Any Additional Formatting Desired

## 4. Hide Technical Columns
By default, this utility follows the best practice of hiding columns that end users don't need:
- GUID/ID columns (like `accountid`)
- They are kept in the model for relationships, but hidden from report view
- If you find that you have additional values that are only needed for formulas, you can hide them as well.

## 5. Create a Model Documentation Page
Add a report page that shows:
- Your data model diagram
- Key measure definitions
- Data refresh information
- Known limitations

## 6. Consider Row-Level Security (for Import Mode)
If using Import mode, implement RLS to control data access:
1. Go to **Modeling** → **Manage roles**
2. Create roles that filter data based on user context
3. Assign users to roles in Power BI Service

## 7. Deploy to Power BI Service

Once you publish your report to Power BI Service:

### For DirectQuery (TDS) Models:
**⚠️ Critical:** Enable Single Sign-On (SSO) in data source credentials:
- This ensures each user sees only their permitted data
- Required for Dataverse row-level security to work
- Enables view filters based on current user context
- See the [Publishing and Deployment](../README.md#-publishing-and-deployment) section for detailed steps

### For Import or Dual Mode:
- Configure scheduled refresh (up to 8 times per day)
- Set up failure notifications
- Consider implementing Power BI RLS if needed (see step 6)

## Learn More

- [Create Measures in Power BI](https://learn.microsoft.com/en-us/power-bi/transform-model/desktop-measures)
- [Create Hierarchies](https://learn.microsoft.com/en-us/power-bi/transform-model/desktop-create-and-manage-relationships)
- [Row-Level Security in Power BI](https://learn.microsoft.com/en-us/power-bi/enterprise/service-admin-rls)
- [Enable Single Sign-On for DirectQuery](https://learn.microsoft.com/power-bi/connect-data/service-azure-sql-database-with-direct-connect#single-sign-on)
