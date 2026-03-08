# Copilot Instructions

## Project Overview

XrmToolBox plugin that generates Power BI semantic models (PBIP/TMDL format) from Dataverse metadata. Users select Dataverse tables via a star-schema wizard, and the tool outputs a complete Power BI project with optimized DirectQuery expressions.

## Build & Test

```powershell
dotnet build -c Release                                # Full solution
dotnet test DataverseToPowerBI.Tests -c Release         # All tests (~190 xUnit tests)
dotnet test DataverseToPowerBI.Tests -c Release --filter "FullyQualifiedName~FetchXml"  # Tests matching a pattern
dotnet test DataverseToPowerBI.Tests -c Release --filter "DisplayName=YourTestName"     # Single test by name
```

**Target framework is .NET Framework 4.8 with C# 9.0.** Do not upgrade to .NET 6+ or change `LangVersion` — XrmToolBox requires net48. No linter is configured.

See `.github/instructions/build-deploy.instructions.md` for the full build/deploy pipeline, NuGet packaging, and assembly reference rules.

## Architecture

Two-project solution with a strict one-way dependency:

```
DataverseToPowerBI.XrmToolBox  (UI + Services + Plugin hosting)
    └── references → DataverseToPowerBI.Core  (Models + Interfaces only)
```

Core must never reference the SDK or XrmToolBox assemblies.

### Key Components

| Component | Role |
|-----------|------|
| `PluginControl` | Main UI orchestrator — coordinates all dialogs, Dataverse calls, and build operations |
| `XrmServiceAdapterImpl` | Implements `IDataverseConnection` using the Dataverse SDK (`IOrganizationService`) |
| `SemanticModelBuilder` | TMDL generation engine (~5,000 lines) — produces full PBIP folder output |
| `FetchXmlToSqlConverter` | Translates Dataverse view FetchXML filters to T-SQL WHERE clauses |
| `DiagramLayoutGenerator` | Generates `diagramLayout.json` for Power BI Model View |
| `SemanticModelManager` | Persists model configurations to JSON in `%APPDATA%` |

### Data Flow

User dialogs → `PluginControl` → `XrmServiceAdapterImpl` (Dataverse queries) → `SemanticModelBuilder` (TMDL generation) → PBIP folder on disk.

### Dual Connection Modes

The builder generates different TMDL depending on connection mode:
- **DataverseTDS** — `Sql.Database` connector via the TDS endpoint with `Value.NativeQuery`.
- **FabricLink** — `Sql.Database` connector against a Fabric Lakehouse SQL endpoint. Generates JOINs to `OptionsetMetadata`/`GlobalOptionsetMetadata` for display names.

## Code Conventions

- **Naming**: PascalCase for public members, `_camelCase` for private fields, ALL_CAPS for constants.
- **XML doc comments** (`///`) on all public types and methods. File-level header blocks describe PURPOSE, SUPPORTED FEATURES, etc.
- **Nullable reference types** enabled. Null-coalescing (`?? throw new ArgumentNullException()`) and null-conditional operators are standard.
- **`#region`** blocks organize files over ~500 lines.
- **Security**: All XML parsing must use `ParseXmlSecurely()` with `DtdProcessing.Prohibit` and `XmlResolver = null`. Never use `XDocument.Parse()` or `XmlDocument.LoadXml()` on untrusted XML.
- **Logging**: `DebugLogger` for diagnostic file logging; `Action<string>? statusCallback` for UI status updates.
- **Type aliases**: Use `using CoreAttributeMetadata = DataverseToPowerBI.Core.Models.AttributeMetadata;` to avoid namespace conflicts with SDK types.
- **TMDL annotations**: Columns include a `DataverseToPowerBI_LogicalName` annotation for stable lineage tag resolution.
- **Version format**: `Major.Year.Minor.Patch` (e.g., `1.2026.3.0`).

## XrmToolBox Constraints

- XrmToolBox provides `IOrganizationService` — never create your own or add auth code.
- All Dataverse SDK calls must run off the UI thread using `WorkAsync` (see `.github/instructions/xrmtoolbox-plugin.instructions.md`).
- XrmToolBox SDK assemblies are referenced from `%APPDATA%` paths with `Private=False` — never add them via NuGet.
- The `IDataverseConnection` interface declares `Task<T>` methods but implementations wrap synchronous SDK calls with `Task.FromResult()`. Do not use `Task.Run()` or `async/await` in the adapter.

## Test Structure

Tests use xUnit on net48. Key test files:
- `SemanticModelBuilderTests.cs` — TMDL output correctness (85 tests)
- `BuilderIntegrationTests.cs` — End-to-end build scenarios (40 tests)
- `FetchXmlToSqlConverterTests.cs` — FetchXML-to-SQL translation (40 tests)
- `IncrementalUpdateTests.cs` — Incremental/merge build behavior (12 tests)
- `PluginSettingsSerializationTests.cs` — Config round-trip (11 tests)

Helper files: `TestDataBuilders.cs` (fluent test data), `FakeDataverseConnection.cs` (mock adapter), `TmdlAssertions.cs` (custom assertions).
