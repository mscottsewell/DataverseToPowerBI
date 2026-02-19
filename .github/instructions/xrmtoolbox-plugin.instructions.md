---
description: "Use when writing XrmToolBox plugin code, forms, controls, connection handling, WorkAsync background operations, MEF plugin registration, or UI threading. Covers plugin lifecycle, PluginControlBase patterns, and WinForms conventions."
applyTo: "DataverseToPowerBI.XrmToolBox/**/*.cs"
---
# XrmToolBox Plugin Patterns

## Plugin Registration (MEF)

Plugins are registered via MEF attributes on a class inheriting `PluginBase`. All metadata is declarative — icons are Base64-encoded PNGs.

```csharp
[Export(typeof(IXrmToolBoxPlugin))]
[ExportMetadata("Name", "Plugin Name")]
[ExportMetadata("Description", "What the plugin does")]
[ExportMetadata("SmallImageBase64", "...")]
[ExportMetadata("BigImageBase64", "...")]
[ExportMetadata("BackgroundColor", "White")]
[ExportMetadata("PrimaryFontColor", "#000000")]
[ExportMetadata("SecondaryFontColor", "#6c757d")]
public class MyPluginTool : PluginBase
{
    public override IXrmToolBoxControl GetControl() => new PluginControl();
}
```

## Connection Lifecycle

The main control inherits `PluginControlBase`. XrmToolBox provides the `IOrganizationService` — never create your own.

- Override `UpdateConnection` to detect connect/disconnect
- Use `InitializeFromExistingConnection()` when the plugin opens after a connection already exists
- Extract the environment URL from `ConnectionDetail.WebApplicationUrl ?? ConnectionDetail.OrganizationServiceUrl`
- Authentication is entirely handled by XrmToolBox — never add auth code

```csharp
public override void UpdateConnection(IOrganizationService newService,
    ConnectionDetail detail, string actionName, object parameter)
{
    base.UpdateConnection(newService, detail, actionName, parameter);
    if (actionName == "AdditionalOrganization") return; // Ignore secondary connections

    if (newService != null)
    {
        _adapter = new XrmServiceAdapterImpl(newService);
        var envUrl = detail.WebApplicationUrl ?? detail.OrganizationServiceUrl;
        // Initialize plugin state...
    }
    else
    {
        // Handle disconnect
    }
}
```

## WorkAsync Pattern (Background Operations)

All Dataverse SDK calls must run off the UI thread using `WorkAsync`. Never call SDK methods directly on the UI thread.

```csharp
WorkAsync(new WorkAsyncInfo
{
    Message = "Loading tables...",
    Work = (worker, args) =>
    {
        // Background thread — no UI access here
        var result = _adapter.GetAllTablesSync();
        worker.ReportProgress(50, "Processing...");
        args.Result = result;
    },
    ProgressChanged = (args) =>
    {
        SetWorkingMessage(args.UserState?.ToString());
    },
    PostWorkCallBack = (args) =>
    {
        // Back on UI thread
        if (args.Error != null)
        {
            MessageBox.Show(args.Error.Message);
            return;
        }
        var tables = (List<TableInfo>)args.Result;
        PopulateTableList(tables);
    }
});
```

Key rules:
- `Work` runs on a background thread — no UI access except via `this.Invoke()`
- `PostWorkCallBack` runs on the UI thread — safe for UI updates
- Pass data from `Work` to `PostWorkCallBack` via `args.Result`
- Check `args.Error` before accessing `args.Result`
- Use `worker.ReportProgress()` with `ProgressChanged` for status updates during long operations

## UI Threading

- Use `_isLoading` guard flags to prevent re-entrant event handling during programmatic UI updates
- Use `BeginInvoke` for deferred initialization
- Use `BeginUpdate()`/`EndUpdate()` and `SuspendLayout()`/`ResumeLayout()` when populating ListViews or complex layouts
- Cache `Font` objects (e.g., `_boldTableFont`) to prevent GDI resource leaks — don't create Fonts in paint/render loops
- Dispose cached Fonts in `Dispose()` override

## Assembly References

XrmToolBox SDK assemblies are referenced from local paths — never add them as NuGet packages:
- `XrmToolBox.Extensibility`, `Microsoft.Xrm.Sdk`, `McTools.Xrm.Connection` — all `Private=False` (not copied to output, XrmToolBox provides them at runtime)
- Only `DataverseToPowerBI.Core.dll` should be `Private=True` (copied to output)
