# Understanding Star-Schema Design

This tool guides you into building a **star-schema** data model, which is the recommended design pattern for Power BI. Understanding this concept will help you build better reports.

## What is a Star-Schema?

A star-schema organizes your data into two types of tables:

**Fact Tables** (the center of the star ⭐)
- Contains your transactional or event data
- Examples: Orders, Opportunities, Cases, Appointments
- Has many rows that grow over time
- Contains the numbers you want to analyze (amounts, quantities, counts)

**Dimension Tables** (the points of the star)
- Contains descriptive attributes for filtering and grouping
- Examples: Customers, Products, Employees, Territories
- Changes less frequently
- Provides the "who, what, where, when" context for your facts

## Why Star-Schema Matters

- ✅ **Faster Performance** — Power BI's engine is optimized for this pattern  
- ✅ **Simpler DAX** — Calculations are easier to write and understand  
- ✅ **Intuitive Filtering** — Filters flow naturally from dimensions to facts  
- ✅ **Smaller Model Size** — Less data duplication than flat tables

## A Note About Snowflake Schemas

Sometimes a dimension table has its own lookup to another table (for example, Customer → Territory → Region). This creates a "snowflake" pattern where dimensions branch into sub-dimensions.

⚠️ **Be cautious with snowflaking:**
- Each additional level adds complexity
- Filters must travel through more relationships
- Performance can degrade with deep hierarchies

**Our recommendation:** Keep your schema as flat as possible. Only add parent dimensions when you truly need to filter or group by those attributes.

## Handling Multiple Relationships to the Same Dimension

In Dataverse, it's very common to have multiple lookup fields pointing to the same table. For example:

- **Cases** might have "Primary Contact," "Reported By," and "Modified By"—all pointing to **Contact**
- **Opportunities** might have "Customer" and "Partner Account"—both pointing to **Account**
- **Work Orders** might have "Billing Account" and "Service Account"—both pointing to **Account**

In a star-schema Power BI model, **only one relationship between two tables can be "Active"** at a time. Other relationships must be marked "Inactive" but can still be used in DAX calculations with the `USERELATIONSHIP()` function.

### Visual Grouping

The tool automatically groups multiple relationships to the same dimension under a header like:

```
Contact (Multiple Relationships)
  ☐ Many:1  Primary Contact       primarycontactid    Contact  contact  Active ⚠
  ☐ Many:1  Reported By          pbi_reportedbyid    Contact  contact  Inactive ⚠
  ☐ Many:1  Responsible Contact  responsiblecontactid Contact contact  Inactive ⚠
```

This makes it easy to see at a glance which relationships belong together and which one is active.

### Smart Selection Behavior

When you check or double-click a relationship:

1. **The selected relationship becomes Active**
2. **All other relationships to that dimension automatically become Inactive**
3. This applies even to relationships you haven't checked yet
4. You can't accidentally create multiple active relationships

This intelligent behavior ensures your model is always in a valid state.

### Conflict Detection and Visual Indicators

The relationship selector uses color coding to help you identify and resolve conflicts before they cause problems in your model:

**Normal State (White Background):**
- Inactive relationships show `Direct (Inactive)` or `Snowflake (Inactive)` in the Type column
- White background indicates no conflict—this is a valid, safely inactive relationship
- These relationships can be used with `USERELATIONSHIP()` in DAX

**Conflict State (Red Background):**
- **Red highlighting only appears when multiple ACTIVE relationships exist to the same dimension**
- This is an invalid state—Power BI cannot have more than one active relationship between two tables
- You must resolve the conflict before building:
  - Double-click one of the red-highlighted relationships to make it Inactive, OR
  - Check a different relationship to automatically inactivate the others

**Why This Matters:**

Power BI's engine uses the active relationship for automatic filter propagation. When you filter a dimension (e.g., select a Contact), that filter flows through the **active** relationship to your fact table. If multiple relationships were active, Power BI wouldn't know which path to use for filtering, creating ambiguous query results.

By marking one relationship as "Inactive," you're telling Power BI: *"This path exists, but don't use it automatically—I'll explicitly tell you when to use it with USERELATIONSHIP()."* This eliminates ambiguity and ensures predictable filtering behavior.

**Example Conflict:**
```
Contact (Multiple Relationships)
  ☑ Many:1  Primary Contact       primarycontactid    Active    Direct           ← RED BACKGROUND
  ☑ Many:1  Reported By          pbi_reportedbyid    Active    Direct           ← RED BACKGROUND
  ☐ Many:1  Responsible Contact  responsiblecontactid Inactive  Direct (Inactive) ← White background
```

In this example, both "Primary Contact" and "Reported By" are active, creating an ambiguous filter path. The red highlighting alerts you to resolve this by making one inactive.

### Using Inactive Relationships in DAX

Inactive relationships are still valuable—they allow you to analyze your data from different perspectives:

```dax
// Default measure uses the active relationship (Primary Contact)
Total Cases = COUNTROWS(Case)

// Use inactive relationship to analyze by Reported By
Cases by Reporter = 
CALCULATE(
    [Total Cases],
    USERELATIONSHIP(Case[pbi_reportedbyid], Contact[contactid])
)

// Use another inactive relationship to analyze by Responsible Contact
Cases by Responsible = 
CALCULATE(
    [Total Cases],
    USERELATIONSHIP(Case[responsiblecontactid], Contact[contactid])
)
```

### Choosing Which Relationship to Make Active

Choose the relationship that represents your **primary** or **most common** analysis:

- For Cases: Make "Primary Contact" active if most reports analyze cases by their main contact
- For Opportunities: Make "Customer" active if analyzing by customer is more common than by partner
- For Work Orders: Make "Service Account" active if service location is more important than billing

The active relationship is used automatically in slicers, filters, and calculations. Inactive relationships require explicit `USERELATIONSHIP()` calls in your DAX measures.

### Search and Filter

When working with many relationships, use the built-in tools:

- **Solution tables only** checkbox (enabled by default): Hides relationships to tables outside your solution
- **Search box**: Filter by field name, logical name, or dimension table name
- Both filters work together to help you focus on relevant relationships

## Learn More

- [Star Schema in Power BI](https://learn.microsoft.com/power-bi/guidance/star-schema)
- [Understand Star Schema Design](https://learn.microsoft.com/power-bi/guidance/star-schema#star-schema-overview)
- [Importance of Star Schema](https://learn.microsoft.com/power-bi/guidance/star-schema#importance-of-star-schema-design)
- I like this explanation from [Brian Julius](https://www.linkedin.com/in/brianjuliusdc/):
<img width="2048" alt="image of a Star Schema" src="https://github.com/user-attachments/assets/d558cd15-5b8d-4ae4-9a91-3d8413783ba1" />
