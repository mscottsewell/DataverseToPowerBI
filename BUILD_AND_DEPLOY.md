# Comprehensive Build & Deploy Plan - XrmToolBox Plugin

## Current Issue: ReflectionTypeLoadException
**Root Cause**: Missing dependency - `DataverseToPowerBI.Core.dll` is not being properly loaded by XrmToolBox's MEF loader.

## Solution: Create Proper NuGet Package

### Package Structure Required
```
DataverseToPowerBI.XrmToolBox.nupkg
├── lib/
│   └── net48/
│       ├── DataverseToPowerBI.XrmToolBox.dll
│       └── DataverseToPowerBI.Core.dll
├── content/
│   └── DataverseToPowerBI/
│       ├── PBIP_DefaultTemplate/
│       │   ├── Template.pbip
│       │   ├── Template.Report/
│       │   └── Template.SemanticModel/
│       └── DateTable.tmdl
└── [Content_Types].xml, package metadata
```

### Build Steps

#### 1. Clean Build
```powershell
cd C:\GitHub\DataverseMetadata-to-PowerBI-Semantic-Model
dotnet clean
dotnet build DataverseToPowerBI.Core/DataverseToPowerBI.Core.csproj -c Release
dotnet build DataverseToPowerBI.XrmToolBox/DataverseToPowerBI.XrmToolBox.csproj -c Release
```

#### 2. Create NuGet Package Structure
```powershell
# Create packaging directory
$packageRoot = "C:\GitHub\DataverseMetadata-to-PowerBI-Semantic-Model\Package"
Remove-Item $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path "$packageRoot\lib\net48" -Force
New-Item -ItemType Directory -Path "$packageRoot\content\DataverseToPowerBI" -Force

# Copy assemblies
Copy-Item "DataverseToPowerBI.XrmToolBox\bin\Release\DataverseToPowerBI.XrmToolBox.dll" "$packageRoot\lib\net48\"
Copy-Item "DataverseToPowerBI.Core\bin\Release\netstandard2.0\DataverseToPowerBI.Core.dll" "$packageRoot\lib\net48\"

# Copy content files
Copy-Item "DataverseToPowerBI.XrmToolBox\Assets\*" "$packageRoot\content\DataverseToPowerBI\" -Recurse -Force
```

#### 3. Create .nuspec File
Create `DataverseToPowerBI.XrmToolBox.nuspec` with proper metadata

#### 4. Build NuGet Package
```powershell
nuget pack DataverseToPowerBI.XrmToolBox.nuspec -OutputDirectory Package
```

#### 5. Deploy to XrmToolBox
```powershell
# Manual install via XrmToolBox Plugin Store or:
Copy-Item "$packageRoot\lib\net48\*" "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\" -Force
Copy-Item "$packageRoot\content\DataverseToPowerBI\*" "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\DataverseToPowerBI\" -Recurse -Force
```

## Verification Checklist
- [ ] Clean solution completely
- [ ] Build Core library (netstandard2.0)
- [ ] Build XrmToolBox plugin (.NET Framework 4.8)
- [ ] Verify both DLLs exist in bin\Release
- [ ] Create NuGet package structure
- [ ] Copy both DLLs to lib\net48
- [ ] Copy Assets to content folder
- [ ] Create .nuspec with correct metadata
- [ ] Pack NuGet package
- [ ] Test deployment to XrmToolBox
- [ ] Verify plugin loads without ReflectionTypeLoadException
- [ ] Test all features (connect, select tables, build model)

## Alternative: Direct Reference Fix
If NuGet packaging is complex, we can modify the .csproj to properly embed/copy the Core.dll:

1. Change Core reference from file reference to ProjectReference
2. Set CopyLocal=true  
3. Add post-build event to ensure Core.dll is in plugin directory
4. Deploy both DLLs together

## Next Steps
Choose one approach:
1. **NuGet Package** (recommended for XrmToolBox store)
2. **Direct deployment with proper references** (faster for development)
