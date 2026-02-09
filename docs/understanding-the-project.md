# Understanding Your Generated Project

The project creates a **Power BI Project (PBIP)** which is a format that allows for editing, change management and collaboration. - A PBIP can be saved as a PBIX file for distribution by simply clicking save-as and choosing the PBIX format.

The tool creates a PBIP folder structure. The exact layout depends on your connection mode:

## Dataverse TDS Mode (Default)

```
YourModelName/
├── YourModelName.pbip              ← Open this file in Power BI Desktop
├── YourModelName.SemanticModel/
│   └── definition/
│       ├── model.tmdl              ← Model configuration and table references
│       ├── relationships.tmdl      ← Table relationships
│       └── tables/                 ← Individual table definitions
│           ├── DataverseURL.tmdl   ← Hidden parameter table (connection URL)
│           ├── Date.tmdl           ← Calendar dimension (if configured)
│           ├── Account.tmdl
│           └── Contact.tmdl ...
└── YourModelName.Report/
    └── (report definition files)
```

## FabricLink Mode

```
YourModelName/
├── YourModelName.pbip              ← Open this file in Power BI Desktop
├── YourModelName.SemanticModel/
│   └── definition/
│       ├── model.tmdl              ← Model configuration and expression references
│       ├── expressions.tmdl        ← DataverseURL, FabricSQLEndpoint, FabricLakehouse
│       ├── relationships.tmdl      ← Table relationships
│       └── tables/                 ← Individual table definitions
│           ├── Date.tmdl           ← Calendar dimension (if configured)
│           ├── Account.tmdl
│           └── Contact.tmdl ...
└── YourModelName.Report/
    └── (report definition files)
```

> **Key difference:** In TDS mode, the Dataverse URL is stored as a hidden parameter *table* (`DataverseURL.tmdl`) — this is required for Power BI Desktop to properly resolve the `CommonDataService.Database` connector. In FabricLink mode, connection details are stored as *expressions* in `expressions.tmdl`.

## About PBIP Format

The PBIP format is a folder-based project that's perfect for:
- Version control with Git
- Collaboration with other developers
- Seeing exactly what changed between versions

## Learn More About PBIP

- [Power BI Project Files Overview](https://learn.microsoft.com/en-us/power-bi/developer/projects/projects-overview)
- [Working with PBIP in Power BI Desktop](https://learn.microsoft.com/en-us/power-bi/developer/projects/projects-build)
- [Power BI Desktop Developer Mode](https://learn.microsoft.com/en-us/power-bi/developer/projects/projects-overview#developer-mode)
