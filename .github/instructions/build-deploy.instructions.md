---
description: "Use when working on build scripts, NuGet packaging, deployment, assembly versioning, Build-And-Deploy.ps1, .nuspec files, or .csproj project references. Covers the build pipeline, version numbering, and local DLL reference management."
applyTo:
  - "Package/**"
  - "**/*.csproj"
  - "**/*.nuspec"
---
# Build & Deploy Pipeline

## Target Framework

Both projects target **.NET Framework 4.8** with **C# 9.0** language version. This is required for XrmToolBox compatibility — do not upgrade to .NET 6+ or change `LangVersion`.

```xml
<TargetFramework>net48</TargetFramework>
<LangVersion>9.0</LangVersion>
```

## Build Commands

```powershell
dotnet build -c Release                              # Full solution
dotnet build DataverseToPowerBI.Core -c Release       # Core library only
dotnet build DataverseToPowerBI.XrmToolBox -c Release  # Plugin only
```

## Build-And-Deploy.ps1

The primary build script at `Package/Build-And-Deploy.ps1` runs a 6-step pipeline:

1. **Clean** — removes `bin/Release` directories
2. **Build Core** — `dotnet build` the Core library
3. **Build Plugin** — `dotnet build` the XrmToolBox project
4. **Verify** — confirms expected DLLs exist in output
5. **NuGet Pack** — creates `.nupkg` from `.nuspec`
6. **Deploy** — copies files to `%APPDATA%\MscrmTools\XrmToolBox\Plugins\`

Switches:
- `-PackageOnly` — build + package, skip deployment
- `-DeployOnly` — skip build, deploy existing outputs

The script auto-increments the build number in `AssemblyInfo.cs` on each non-deploy-only run.

## Version Format

`Major.Year.Minor.Patch` (e.g., `1.2026.3.0`). The script updates:
- `Properties/AssemblyInfo.cs` — assembly version attributes
- `.nuspec` — NuGet package version

## NuGet Packaging

Uses a `.nuspec` file (not `dotnet pack`). Layout:
- `lib/net48/` — plugin DLLs
- `content/DataverseToPowerBI/` — asset files (icons, templates)

The script downloads `nuget.exe` from `dist.nuget.org` if not found in PATH.

## Assembly References

XrmToolBox SDK assemblies are referenced from local `%APPDATA%` paths, not NuGet:

```xml
<Reference Include="XrmToolBox.Extensibility">
  <HintPath>$(APPDATA)\MscrmTools\XrmToolBox\Plugins\XrmToolBox.Extensibility.dll</HintPath>
  <Private>False</Private>  <!-- XrmToolBox provides at runtime -->
</Reference>
```

Rules:
- **XrmToolBox/SDK assemblies** → `Private=False` (not copied to output)
- **Core project reference** → `Private=True` (copied to output)
- Never add XrmToolBox assemblies via NuGet — versions must match the user's installed XrmToolBox

## Post-Build Asset Copy

An MSBuild target copies `Assets/` and PBIP template files to the XrmToolBox Plugins directory after successful build:

```xml
<Target Name="CopyAssets" AfterTargets="Build">
  ...
</Target>
```

## Deployment Target

All plugin files deploy to: `%APPDATA%\MscrmTools\XrmToolBox\Plugins\`

Configuration persists to: `%APPDATA%\MscrmTools\XrmToolBox\Settings\DataverseToPowerBI\`
