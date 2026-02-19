---
description: "Use when writing Dataverse SDK code, IOrganizationService calls, QueryExpression queries, FetchXML parsing, entity/attribute metadata retrieval, or implementing IDataverseConnection. Covers SDK patterns, async wrapping, and metadata query conventions."
applyTo: "DataverseToPowerBI.XrmToolBox/**/*.cs"
---
# Dataverse SDK Interaction

## Architecture

Dataverse access is abstracted behind `IDataverseConnection` (in Core) and implemented by `XrmServiceAdapterImpl` (in XrmToolBox). Core must never reference the SDK directly.

```
IDataverseConnection (Core — interface only)
    └── XrmServiceAdapterImpl (XrmToolBox — uses IOrganizationService)
```

## Async-over-Sync Pattern

The `IDataverseConnection` interface declares `Task<T>` methods, but XrmToolBox manages threading via `WorkAsync`. Implementations wrap synchronous SDK calls:

```csharp
public Task<List<TableInfo>> GetAllTablesAsync()
{
    return Task.FromResult(GetAllTablesSync());
}

private List<TableInfo> GetAllTablesSync()
{
    var request = new RetrieveAllEntitiesRequest
    {
        EntityFilters = EntityFilters.Entity
    };
    var response = (RetrieveAllEntitiesResponse)_service.Execute(request);
    // Map to Core models...
}
```

Do not use `Task.Run()` or `async/await` in the adapter — the caller (`WorkAsync`) already handles the background thread.

## QueryExpression for Data Queries

Use `QueryExpression` (not FetchXML strings) for querying Dataverse data entities:

```csharp
var query = new QueryExpression("solutioncomponent")
{
    ColumnSet = new ColumnSet("objectid", "componenttype"),
    Criteria = new FilterExpression
    {
        Conditions =
        {
            new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
            new ConditionExpression("componenttype", ConditionOperator.Equal, 1) // Entity
        }
    }
};
var results = _service.RetrieveMultiple(query);
```

## Metadata Requests

Use strongly-typed metadata requests for entity/attribute schema:

| Request | Use |
|---------|-----|
| `RetrieveAllEntitiesRequest` with `EntityFilters.Entity` | Bulk entity enumeration (names, display names) |
| `RetrieveEntityRequest` with `EntityFilters.Attributes` | Per-table attribute details |
| `RetrieveEntityRequest` with `EntityFilters.Relationships` | Relationship metadata for a single table |

## Type Alias Convention

Avoid namespace conflicts between Core models and SDK types using type aliases:

```csharp
using CoreAttributeMetadata = DataverseToPowerBI.Core.Models.AttributeMetadata;
using XrmModels = DataverseToPowerBI.Core.Models;
```

## Virtual Attribute Detection

Picklist, Boolean, State, and Status attributes have associated virtual attributes for display names. Use a two-pass approach:
1. Build a dictionary of all attributes by logical name
2. For each option-set type, search for `{logicalname}name` pattern, then apply fallback heuristics

## Common Entity References

| Entity | Use |
|--------|-----|
| `solutioncomponent` (type=1) | Tables in a solution |
| `systemform` (type=2,7) | Main and QuickView forms |
| `savedquery` (querytype=0) | Public views |

## FetchXML Parsing Security

When parsing FetchXML or FormXML from Dataverse, always use `ParseXmlSecurely()`:

```csharp
private static XDocument ParseXmlSecurely(string xml)
{
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };
    using var reader = XmlReader.Create(new StringReader(xml), settings);
    return XDocument.Load(reader);
}
```

Never use `XDocument.Parse()` or `XmlDocument.LoadXml()` directly on untrusted XML — Dataverse-returned XML (FetchXML, FormXML) must always go through secure parsing. This prevents XXE attacks.

## Error Handling & Fallbacks

- If the user lacks `prvReadSolution` privilege, fall back to `GetAllTablesSync()` (retrieves all entities, filters Activity/Intersect types)
- Log metadata retrieval failures and return partial results rather than throwing
- Use `DebugLogger.Log()` for diagnostic output during metadata operations
