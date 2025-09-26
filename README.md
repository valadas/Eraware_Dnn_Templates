# Eraware_Dnn_Templates
DNN templates for creating modern web API/WebComponents modules.

Available in the Visual Studio Marketplace at
https://marketplace.visualstudio.com/items?itemName=DanielValadas.eraware-dnn-templates

## Requirements

### Visual Studio Compatibility
This extension supports the following Visual Studio versions:

| Visual Studio Version | Version Range | Release Years | Notes |
|---------------------- |---------------|---------------|-------|
| Visual Studio 2019    | 16.0 - 16.x   | 2019-2022     | x86 architecture |
| Visual Studio 2022    | 17.0 - 18.x   | 2022-2025+    | amd64 architecture |

**Supported Editions:**
- Community (Free)
- Professional
- Enterprise

### Required Workloads
To use these templates effectively, ensure you have the following Visual Studio workloads installed:
- **ASP.NET and web development** - For the Web API backend
- **Node.js development** - For the frontend build process
- **.NET desktop development** - For general .NET development

> **Note:** If you encounter errors opening the `module.web` project, use the Visual Studio Installer to add the "Node.js development" workload.

## Templates Overview

Currently this templates package includes a comprehensive module template with plans to expand into multiple DNN extension types.

## Stencil SPA Module Template
A WebAPI/WebComponents module template that creates modern, standards-compliant DNN modules.

### Frontend Technologies
- **[Stencil.js](https://stenciljs.com/)** - Build-time compiler for creating standards-compliant Web Components
- **[TypeScript](https://www.typescriptlang.org/)** - Enforced for type safety and better development experience
- **JSX** - React-like syntax (note: this template does NOT use React)
- **[DNN Elements](https://www.npmjs.com/package/@eraware/dnn-elements)** - DNN-specific WebComponents library
- **Auto-generated Documentation** - TypeScript builds automatically document each component

### Backend Technologies
- **[Entity Framework](https://www.entityframeworktutorial.net/)** - Code-first ORM with generic repository pattern
- **[Effort](https://entityframework-effort.net/)** - In-memory database for easier testing
- **[Swagger](https://swagger.io/)** - API documentation and TypeScript client generation
- **Dependency Injection** - Modern .NET DI patterns

### Development & Documentation
- **[DocFX](https://dotnet.github.io/docfx/)** - Keeps documentation synchronized with code
- **[XUnit](https://xunit.net/)** - Unit and Integration testing framework
- **100% Unit Test Coverage** - Template starts with comprehensive test coverage

### Current Limitations
1. **Uninstallation**: Still requires SQL script for table cleanup
2. **Object Qualifier**: Not currently supported in Code-First approach

## Installation & Usage

1. Install the extension from the Visual Studio Marketplace
2. Restart Visual Studio
3. Create a new project and search for "Eraware" or "DNN"
4. Select the desired template and follow the setup wizard

