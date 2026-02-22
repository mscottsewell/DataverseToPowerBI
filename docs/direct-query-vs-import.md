# Direct Query vs. Import Mode

Your generated model uses **DirectQuery** by default. Here's what that means and how to choose the right mode for your needs.

## DirectQuery Mode (Default)

**How it works:** Every time you interact with your report, Power BI sends a query to Dataverse and retrieves fresh data.

| ✅ Advantages | ⚠️ Considerations |
|--------------|-------------------|
| Always shows the latest data | Slower interactivity (1-5 second delays) |
| Respects Dataverse row-level security | Some DAX functions not available |
| No scheduled refresh needed | Subject to Dataverse API limits |
| Smaller .pbix file size | Complex calculations may timeout |
| Users see only their permitted data | Best with fewer concurrent users |

**Best for:**
- Real-time operational dashboards  
- Security-sensitive data where each user should only see their records
- Data that changes frequently throughout the day
- Queries that reference 'Current_User' in the filter (e.g. "My Opportunities")

**⚠️ Deployment Requirement for TDS DirectQuery:**
When publishing to Power BI Service, you **must enable Single Sign-On (SSO)** in the data source credentials settings to ensure reports are filtered based on each user's credentials and Dataverse security is enforced. See [Publishing and Deployment](../README.md#-publishing-and-deployment) for configuration steps.

## Import Mode

**How it works:** Data is copied into the Power BI file and stored in memory. Reports query this in-memory cache.

| ✅ Advantages | ⚠️ Considerations |
|--------------|-------------------|
| Blazing fast interactivity | Data must be refreshed regularly |
| Full DAX functionality | Requires scheduled refresh (up to 8x/day) |
| Works offline | Larger file sizes |
| No per-query API limits | Security must be applied at report level (RLS) |
| Handles many concurrent users | Dataverse security not automatic |

**Best for:**
- Historical trend analysis
- Complex calculations requiring full DAX
- Published reports with many users
- Executive dashboards refreshed daily

## Dual Modes (The Best of Both Worlds)

You can use **different storage modes for different tables**—this is often the optimal approach. The tool supports:

- **Dual (All)** — Fact tables stay DirectQuery; all dimension tables use Dual
- **Dual (Select)** — Fact tables stay DirectQuery; only selected dimension tables use Dual

- Keep your large, frequently-changing **fact table in DirectQuery**
- Use Dual for smaller, stable **dimension tables** for fast filtering

| ✅ Advantages | ⚠️ Considerations |
|--------------|-------------------|
| Fast dimension filtering and slicing | Requires refresh schedule for dimensions |
| Always-current fact data | Slightly more complex to configure |
| Efficient for large fact tables | Must plan which tables to import |
| Reduces Dataverse API calls | |

**To switch a table to Import mode:**
1. Open your model in Power BI Desktop
2. Go to **Model** view
3. Select the table you want to change
4. In the **Properties** pane, change **Storage mode** from "DirectQuery" to "Import"
5. Configure a refresh schedule when you publish to Power BI Service

> **Please Note** that any table that was built with a view that referenced the **current user** will need to be updated to use a different view/filter.

## Learn More

- [DirectQuery in Power BI](https://learn.microsoft.com/en-us/power-bi/connect-data/desktop-directquery-about)
- [Storage Mode in Power BI Desktop](https://learn.microsoft.com/en-us/power-bi/transform-model/desktop-storage-mode)
- [Composite Models in Power BI](https://learn.microsoft.com/en-us/power-bi/transform-model/desktop-composite-models)
