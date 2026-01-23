# Architecture & Data Flow

## System Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Your Computer                                 │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                     Python Scripts                              │  │
│  │                                                                 │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐ │  │
│  │  │   Test           │  │   Preview        │  │   Extract    │ │  │
│  │  │   Connection     │  │   Metadata       │  │   Metadata   │ │  │
│  │  └──────────────────┘  └──────────────────┘  └──────────────┘ │  │
│  │           │                     │                     │         │  │
│  │           └─────────────────────┴─────────────────────┘         │  │
│  │                              │                                  │  │
│  │                    ┌─────────▼──────────┐                      │  │
│  │                    │   MSAL Library     │                      │  │
│  │                    │  (Authentication)  │                      │  │
│  │                    └─────────┬──────────┘                      │  │
│  └──────────────────────────────┼───────────────────────────────┘  │
│                                  │                                   │
└──────────────────────────────────┼───────────────────────────────────┘
                                   │
                    ┌──────────────▼───────────────┐
                    │   HTTPS (OAuth 2.0 + TLS)   │
                    └──────────────┬───────────────┘
                                   │
        ┌──────────────────────────┴────────────────────────┐
        │                                                     │
┌───────▼──────────┐                              ┌──────────▼─────────┐
│  Microsoft       │                              │   Your Dataverse   │
│  Identity        │◄─────────────────────────────┤   Environment      │
│  Platform        │      Issues Access Token     │                    │
│                  │                              │  • Solutions       │
│  (login.micro-   │                              │  • Tables          │
│   softonline.    │                              │  • Forms           │
│   com)           │                              │  • Fields          │
└──────────────────┘                              └────────────────────┘
```

## Data Flow - Step by Step

### 1. Authentication Flow

```
User              Python Script         MSAL Library      Microsoft Identity    Dataverse
  │                     │                     │                  │                  │
  │  Run Script        │                     │                  │                  │
  ├───────────────────►│                     │                  │                  │
  │                     │  Request Token      │                  │                  │
  │                     ├────────────────────►│                  │                  │
  │                     │                     │  Open Browser    │                  │
  │                     │                     ├─────────────────►│                  │
  │                     │                     │                  │                  │
  │  Login & Consent   │                     │                  │                  │
  │◄────────────────────┼─────────────────────┼──────────────────┤                  │
  ├────────────────────►│                     │                  │                  │
  │                     │                     │  Return Token    │                  │
  │                     │◄────────────────────┼──────────────────┤                  │
  │                     │  Access Token       │                  │                  │
  │                     │◄────────────────────┤                  │                  │
  │                     │                     │                  │                  │
  │                     │  API Request with Token              │                  │
  │                     ├──────────────────────────────────────────────────────────►│
  │                     │                     │                  │    JSON Response │
  │                     │◄──────────────────────────────────────────────────────────┤
```

### 2. Metadata Extraction Flow

```
┌─────────────────────┐
│  1. Query Solution  │
│     Components      │
└──────────┬──────────┘
           │
           │  GET /solutions?$filter=uniquename eq 'MySolution'
           │  Returns: Solution ID
           │
           ▼
┌─────────────────────┐
│  2. Get Solution    │
│     Tables          │
└──────────┬──────────┘
           │
           │  GET /solutioncomponents?$filter=_solutionid_value eq {id}
           │  Returns: List of Entity IDs
           │
           ▼
┌─────────────────────┐
│  3. For Each Table: │
│     Get Metadata    │
└──────────┬──────────┘
           │
           │  GET /EntityDefinitions({entityId})
           │  Returns: LogicalName, DisplayName, SchemaName, etc.
           │
           ▼
┌─────────────────────┐
│  4. Get Main Forms  │
└──────────┬──────────┘
           │
           │  GET /systemforms?$filter=objecttypecode eq 'account' and type eq 2
           │  Returns: Form definitions with XML
           │
           ▼
┌─────────────────────┐
│  5. Parse Form XML  │
│     Extract Fields  │
└──────────┬──────────┘
           │
           │  Parse <control datafieldname="...">
           │  Collect unique field names
           │
           ▼
┌─────────────────────┐
│  6. Get Field       │
│     Metadata        │
└──────────┬──────────┘
           │
           │  GET /EntityDefinitions(LogicalName='account')/Attributes
           │  Returns: Full attribute metadata
           │
           ▼
┌─────────────────────┐
│  7. Generate JSON   │
│     Output File     │
└─────────────────────┘
```

## API Endpoints Used

### Dataverse Web API v9.2

| Endpoint | Purpose | Example |
|----------|---------|---------|
| `/solutions` | Find solution by name | `?$filter=uniquename eq 'CoreAI'` |
| `/solutioncomponents` | Get entities in solution | `?$filter=_solutionid_value eq {guid}` |
| `/EntityDefinitions` | Get entity metadata | `EntityDefinitions({entityId})` |
| `/systemforms` | Get forms for entity | `?$filter=objecttypecode eq 'account'` |
| `/EntityDefinitions/.../Attributes` | Get field metadata | `EntityDefinitions(...)/Attributes` |

## Data Structures

### Internal Object Model

```
DataverseMetadataExtractor
├── __init__(environment_url, access_token)
├── get_solution_tables(solution_name)
│   └── Returns: List[TableMetadata]
├── get_entity_metadata(entity_id)
│   └── Returns: TableMetadata
├── get_main_forms_for_entity(logical_name)
│   └── Returns: List[FormMetadata]
├── extract_fields_from_form_xml(form_xml)
│   └── Returns: Set[field_name]
├── get_entity_attributes(logical_name, fields)
│   └── Returns: List[AttributeMetadata]
└── extract_metadata(solution_name, output_folder)
    └── Returns: CompleteMetadata
```

### Output JSON Schema

```json
{
  "Environment": "string",           // URL of Dataverse environment
  "Solution": "string",              // Unique name of solution
  "Tables": [                        // Array of tables
    {
      "LogicalName": "string",       // Entity logical name
      "DisplayName": "string",       // Display name (localized)
      "SchemaName": "string",        // Schema name (PascalCase)
      "ObjectTypeCode": "integer",   // Entity type code
      "PrimaryIdAttribute": "string",// Primary key field
      "PrimaryNameAttribute": "string", // Primary name field
      "MetadataId": "string",        // Unique metadata ID
      "Forms": [                     // Array of forms
        {
          "FormId": "string",        // Form GUID
          "FormName": "string",      // Form display name
          "FieldCount": "integer"    // Number of fields in form
        }
      ],
      "Attributes": [                // Array of fields
        {
          "LogicalName": "string",   // Field logical name
          "SchemaName": "string",    // Field schema name
          "DisplayName": "string",   // Field display name
          "AttributeType": "string", // Data type
          "IsCustom": "boolean"      // Custom vs standard field
        }
      ]
    }
  ]
}
```

## Security & Authentication

### OAuth 2.0 Flow (Interactive)

```
┌──────────────┐
│  User runs   │
│  script      │
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│  MSAL checks cache   │
│  Token valid?        │
└──────┬───────────────┘
       │
       ├─ Yes ──────────────────┐
       │                         │
       ├─ No ──────────┐         │
       │                │         │
       ▼                ▼         ▼
┌──────────────┐  ┌─────────────────┐
│  Open        │  │  Use cached     │
│  Browser     │  │  token          │
└──────┬───────┘  └─────────┬───────┘
       │                     │
       ▼                     │
┌──────────────┐            │
│  User signs  │            │
│  in to MS    │            │
└──────┬───────┘            │
       │                     │
       ▼                     │
┌──────────────┐            │
│  Consent     │            │
│  prompt      │            │
└──────┬───────┘            │
       │                     │
       ▼                     │
┌──────────────┐            │
│  Token       │            │
│  returned    │            │
└──────┬───────┘            │
       │                     │
       └─────────────────────┤
                             │
                             ▼
                   ┌──────────────────┐
                   │  API calls with  │
                   │  Bearer token    │
                   └──────────────────┘
```

### Permissions Required

**User must have:**
- ✓ Read access to Dataverse environment
- ✓ Read access to solution
- ✓ Read access to entity metadata
- ✓ Read access to system forms

**Script uses:**
- Client ID: `51f81489-12ee-4a9e-aaae-a2591f45987d` (Microsoft Dataverse public client)
- Scope: `https://yourorg.crm.dynamics.com/.default`
- Authority: `https://login.microsoftonline.com/organizations`

## Performance Characteristics

### Typical Execution Times

| Operation | Typical Time | Factors |
|-----------|--------------|---------|
| Authentication (first time) | 10-30 sec | User interaction |
| Authentication (cached) | < 1 sec | Token from cache |
| Query solution | 1-2 sec | Network latency |
| Get tables list | 2-5 sec | Number of tables |
| Get forms per table | 0.5-1 sec | Number of forms |
| Parse form XML | < 0.1 sec | Form complexity |
| Get field metadata | 1-2 sec | Number of fields |
| Write JSON | < 0.1 sec | File size |

**Total for 15 tables:** ~30-60 seconds

### Optimization Opportunities

- **Parallel Requests**: Fetch forms for multiple tables simultaneously
- **Batch Operations**: Use OData $batch endpoint
- **Caching**: Cache entity metadata between runs
- **Incremental Updates**: Only fetch changed tables

## Error Handling

```
┌─────────────────┐
│  API Request    │
└────────┬────────┘
         │
         ▼
    ┌────────┐
    │Success?│
    └───┬────┘
        │
   No───┼───Yes
        │        │
        ▼        ▼
┌──────────┐  ┌──────────┐
│HTTP Error│  │Continue  │
└────┬─────┘  └──────────┘
     │
     ├─ 401 Unauthorized → Re-authenticate
     ├─ 403 Forbidden → Check permissions
     ├─ 404 Not Found → Check solution/table name
     ├─ 429 Too Many Requests → Retry with backoff
     ├─ 500 Server Error → Retry
     └─ Other → Log and continue or fail
```

## Comparison: Excel vs Direct Extraction

```
Excel Method:
─────────────
User ──► XrmToolBox ──► Manual Export ──► Excel File ──► PowerShell ──► JSON

Pros: Familiar UI, visual selection
Cons: Manual, error-prone, not repeatable


Direct Extraction:
──────────────────
User ──► Python Script ──┬──► OAuth ──► Dataverse API ──► JSON
                         │
                         └──► Automated, repeatable, version-controlled

Pros: Automated, always current, CI/CD ready, Git-friendly
Cons: Requires Python, initial setup
```

## File Structure After Extraction

```
Reports/
└── MyProject/
    ├── Metadata/
    │   ├── DataverseURL.txt           ← Environment URL for reference
    │   ├── MySolution Metadata Dictionary.json  ← Generated output
    │   └── (optional) *.xlsx          ← Legacy Excel file
    └── PBIP/
        ├── MyProject.pbip             ← Power BI project file
        ├── MyProject.Report/          ← Report definition
        │   └── ...
        └── MyProject.SemanticModel/   ← Semantic model
            ├── definition.pbism
            ├── diagramLayout.json
            └── definition/
                ├── database.tmdl
                ├── expressions.tmdl
                ├── model.tmdl
                ├── relationships.tmdl
                └── tables/
                    ├── Account.tmdl
                    ├── Contact.tmdl
                    └── ...
```

---

**Visual diagrams created to help understand the architecture and data flow!**
