---
description: "Prepare a release: update changelog, nuspec, and check docs for consistency"
agent: "agent"
---

# Prepare Release

You are preparing a release for the DataverseToPowerBI XrmToolBox plugin. Follow these steps exactly.

## Context Files

Read these files to understand the current state:

- [AssemblyInfo.cs](../../DataverseToPowerBI.XrmToolBox/Properties/AssemblyInfo.cs) — current version
- [CHANGELOG.md](../../docs/CHANGELOG.md) — release history
- [DataverseToPowerBI.XrmToolBox.nuspec](../../Package/DataverseToPowerBI.XrmToolBox.nuspec) — NuGet package metadata
- [README.md](../../README.md) — project documentation
- [Build-And-Deploy.ps1](../../Package/Build-And-Deploy.ps1) — build script (auto-increments the build/patch number)

## Version Numbering

The version format is `Major.Year.Minor.Patch` (e.g., `1.2026.5.180`).

The build script (`Build-And-Deploy.ps1`) **auto-increments the Patch number** in AssemblyInfo.cs each time it runs. Therefore:

1. Read the **current** version from `AssemblyInfo.cs` (e.g., `1.2026.5.181`).
2. The **release version** to document is **current + 1** (e.g., `1.2026.5.182`) — this is the version the build will produce.
3. Use this release version in both the CHANGELOG and the nuspec.
4. Do NOT edit AssemblyInfo.cs — the build script handles that.

## Step 1: Determine Changes

Look at the git diff against the `main` branch to identify all changes since the last release. Use `git log` and `git diff` to understand what was changed. Cross-reference with the latest entry in CHANGELOG.md to avoid duplicating already-documented changes.

Summarize the changes and present them to the user grouped by category (Added, Changed, Fixed, Removed) before proceeding. Ask the user to confirm or adjust before writing.

## Step 2: Update CHANGELOG.md

1. Replace the `## [Unreleased]` section content with a blank (keep the heading).
2. Insert a new version section **between** `## [Unreleased]` and the previous release, using today's date:

```markdown
## [X.XXXX.X.XXX] - YYYY-MM-DD

### Added
- ...

### Changed
- ...

### Fixed
- ...
```

Follow the existing changelog style exactly:
- Bold lead text for each bullet (e.g., `**Feature Name**`)
- Em-dash separator between the bold title and the description
- Sub-bullets for details, modes, or technical notes
- Reference links where useful (📚 **Reference:** pattern)
- Keep the `---` horizontal rule between version sections

## Step 3: Update NuSpec

In `Package/DataverseToPowerBI.XrmToolBox.nuspec`:

1. Update `<version>` to the release version.
2. Replace `<releaseNotes>` with a concise summary of this release. Follow the existing style:
   - Lead with a version tag and bold summary line
   - NEW/CHANGED/FIXED prefixes for each paragraph
   - Keep it concise — this appears in NuGet/XrmToolBox tool library
   - End with a link to `docs/CHANGELOG.md` and the GitHub repo

## Step 4: Review README and Docs

Check these files for consistency with the changes made:

- `README.md` — Key Features table, Latest Changes section, any feature descriptions that may need updating
- `docs/troubleshooting.md` — if any fixes affect troubleshooting guidance
- `docs/understanding-the-project.md` — if generated output format changed
- Other docs as relevant to the specific changes

Only propose doc changes if the release actually affects documented behavior. Report what you checked and whether changes are needed.

## Step 5: Summary

Present a final summary:
- Release version number
- Files modified
- Any doc changes made or skipped (with reasoning)
