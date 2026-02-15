# Dataverse to Power BI Semantic Model Generator (PPTB)

PowerPlatformToolBox version of the Dataverse to Power BI Semantic Model Generator.

## Overview

This is a complete port of the XrmToolBox plugin to PowerPlatformToolBox. It generates optimized Power BI semantic models (PBIP/TMDL format) from Dataverse metadata.

## Features

- ✅ Cross-platform (Windows, macOS, Linux) via PPTB
- ✅ Modern React + TypeScript + Fluent UI interface
- ✅ Dual connection mode support (DataverseTDS and FabricLink)
- ✅ Star-schema wizard for fact/dimension modeling
- ✅ Incremental builds with change detection
- ✅ 100% functional parity with XrmToolBox version

## Development

### Prerequisites

- Node.js 18+
- PowerPlatformToolBox desktop application
- Dataverse environment for testing

### Setup

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Run tests
npm run test
```

### Project Structure

```
DataverseToPowerBI.PPTB/
├── src/
│   ├── components/       # React UI components
│   │   ├── common/       # Shared components
│   │   ├── wizard/       # Wizard step components
│   │   └── dialogs/      # Modal dialogs
│   ├── hooks/            # Custom React hooks
│   ├── adapters/         # PPTB API wrappers
│   ├── core/             # Business logic
│   │   ├── tmdl/         # TMDL generation engine
│   │   ├── converters/   # FetchXML to SQL converter
│   │   ├── models/       # Data models
│   │   └── interfaces/   # TypeScript interfaces
│   ├── types/            # TypeScript type definitions
│   ├── utils/            # Utility functions
│   └── assets/           # Static assets
├── public/               # Public assets
├── dist/                 # Build output (PPTB loads from here)
├── index.html            # Entry point
├── vite.config.ts        # Vite configuration (IIFE format)
├── tsconfig.json         # TypeScript configuration
└── package.json          # Dependencies
```

## Installation in PPTB

1. Build the project: `npm run build`
2. Open PowerPlatformToolBox
3. Go to Tools section
4. Click "Install Tool" or "Load Local Tool"
5. Browse to the `dist/` folder
6. The tool should appear in the tool list

## Architecture

This port maintains the same three-layer architecture as the original:

1. **Core Layer**: Business logic (TMDL generation, FetchXML conversion)
2. **Adapter Layer**: PPTB API integration (Dataverse, FileSystem, Settings)
3. **UI Layer**: React components with Fluent UI

## Technology Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Fluent UI v9** - Microsoft design system
- **Zustand** - State management
- **Vite** - Build tool (IIFE format for PPTB)
- **@pptb/types** - PPTB API type definitions

## Documentation

- [Porting Plan](../PPTB_PORTING_PLAN.md) - Detailed migration plan
- [Quick Start Guide](../PPTB_QUICK_START.md) - Implementation guide
- [Original README](../README.md) - XrmToolBox version documentation

## License

MIT - See [LICENSE](../LICENSE)
