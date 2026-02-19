# Copilot Instructions

## Project Overview

XrmToolBox plugin that generates Power BI semantic models (PBIP/TMDL format) from Dataverse metadata. Users select Dataverse tables via a star-schema wizard, and the tool outputs a complete Power BI project with optimized DirectQuery expressions.

## Build

```powershell
dotnet build -c Release
```

Target framework is **.NET Framework 4.8** with **C# 9.0** language version. Both projects must target `net48` for XrmToolBox compatibility. See `.github/instructions/build-deploy.instructions.md` for full build/deploy pipeline details.

## Architecture

Two-project solution with a strict dependency direction:

```
XrmToolBox (UI + Services + Plugin hosting)
    └── references → Core (Models + Interfaces)
```

- **DataverseToPowerBI.Core** — Shared library containing data models (`DataModels.cs`) and the `IDataverseConnection` interface. Framework-agnostic; no UI dependencies.
- **DataverseToPowerBI.XrmToolBox** — The plugin itself. Contains all UI forms, the TMDL generation engine (`SemanticModelBuilder`), FetchXML-to-SQL converter, configuration persistence, and the Dataverse SDK adapter.

### Data Flow

User dialogs → `PluginControl` (orchestrator) → `XrmServiceAdapterImpl` (Dataverse queries) → `SemanticModelBuilder` (TMDL generation) → PBIP folder output on disk.

### Dual Connection Modes

The builder generates different TMDL output depending on connection mode:
- **DataverseTDS** — Uses `CommonDataService.Database` connector with `Value.NativeQuery` and SQL via the TDS endpoint.
- **FabricLink** — Uses `Sql.Database` connector against a Fabric Lakehouse SQL endpoint. Generates JOINs to `OptionsetMetadata`/`GlobalOptionsetMetadata` tables for display names.

## Code Conventions

- **Naming**: PascalCase for public members, `_camelCase` for private fields, ALL_CAPS for constants.
- **XML doc comments** (`///`) on all public types and methods. File-level header blocks describe PURPOSE, SUPPORTED FEATURES, etc.
- **Nullable reference types** are enabled. Null-coalescing (`?? throw new ArgumentNullException()`) and null-conditional operators are standard.
- **`#region`** blocks organize large files (>500 lines).
- **Security**: All XML parsing uses `ParseXmlSecurely()` with `DtdProcessing.Prohibit` and `XmlResolver = null` to prevent XXE attacks. Never parse XML without these settings.
- **Logging**: Use `DebugLogger` (thread-safe, static, lock-based) for diagnostic output. Use `Action<string>? statusCallback` for UI status updates.

## Version Numbering

Format: `Major.Year.Minor.Patch` (e.g., `1.2026.3.0`).
