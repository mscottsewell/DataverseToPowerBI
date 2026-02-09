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

## Learn More

- [Star Schema in Power BI](https://learn.microsoft.com/power-bi/guidance/star-schema)
- [Understand Star Schema Design](https://learn.microsoft.com/power-bi/guidance/star-schema#star-schema-overview)
- [Importance of Star Schema](https://learn.microsoft.com/power-bi/guidance/star-schema#importance-of-star-schema-design)
- I like this explanation from [Brian Julius](https://www.linkedin.com/in/brianjuliusdc/):
<img width="2048" alt="image of a Star Schema" src="https://github.com/user-attachments/assets/d558cd15-5b8d-4ae4-9a91-3d8413783ba1" />
