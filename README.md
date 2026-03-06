# Photoshop MCP Server

🌐 **Language**: **English** | [한국어](README.ko.md)

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server that enables AI assistants to control Adobe Photoshop via Windows COM automation. Built with .NET 10 and C# 14 using the official [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).

## Overview

This MCP server exposes Photoshop automation as a set of tools that any MCP-compatible AI client (Claude Desktop, GitHub Copilot, etc.) can invoke. The primary tool is `ExecuteJavaScript`, which gives the AI full access to Photoshop's scripting engine — allowing it to flexibly decide what to do at runtime.

## Requirements

- **Windows** (COM automation is Windows-only)
- **Adobe Photoshop** (any version supporting COM automation and `DoJavaScript`)
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** or later

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/airtaxi/PhotoshopMcpServer.git
cd PhotoshopMcpServer
```

### 2. Build

```bash
dotnet build
```

### 3. (Optional) Publish as a self-contained executable

```bash
dotnet publish PhotoshopMcpServer/PhotoshopMcpServer.csproj -c Release -r win-x64 --self-contained
```

The executable will be in `PhotoshopMcpServer/bin/Release/net10.0-windows/win-x64/publish/`.

## MCP Server Configuration

### Claude Desktop

Add the following to your Claude Desktop configuration file:

- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

#### Option A: Run from source (requires .NET SDK)

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

#### Option B: Run published executable

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "C:\\path\\to\\PhotoshopMcpServer.exe"
    }
  }
}
```

### GitHub Copilot (VS Code)

Add to your `.vscode/mcp.json` or VS Code settings:

```json
{
  "servers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

### Cursor

Add to your Cursor MCP settings (`~/.cursor/mcp.json`):

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

> **Note**: Replace `C:\path\to\PhotoshopMcpServer` with the actual path where you cloned the repository.

## Available Tools

| Tool | Description |
|------|-------------|
| `ExecuteJavaScript` | **Primary tool** — execute arbitrary JavaScript in Photoshop's scripting engine |
| `IsPhotoshopRunning` | Check if Photoshop is running and accessible |
| `LaunchPhotoshop` | Launch Photoshop or connect to a running instance |
| `GetPhotoshopVersion` | Get the Photoshop version string |
| `GetActiveDocumentInfo` | Get info about the active document (name, size, color mode, resolution) |
| `GetOpenDocuments` | List all open document names |
| `OpenDocument` | Open an image file by path |
| `SaveActiveDocument` | Save the current document |
| `CreateNewDocument` | Create a new document with specified dimensions |
| `ExportAsPng` | Export the active document as PNG |
| `ExportAsJpeg` | Export the active document as JPEG with quality setting |
| `GetLayerInfo` | Get a summary of all layers in the active document |

### ExecuteJavaScript — The Power Tool

The `ExecuteJavaScript` tool is intentionally flexible. It allows the AI to construct and execute any valid Photoshop JavaScript, giving it full control over Photoshop. Example scripts:

```javascript
// Get document name
app.activeDocument.name

// Create a new document
app.documents.add(1920, 1080, 72, "My Canvas")

// Resize the active document
app.activeDocument.resizeImage(800, 600)

// Flatten all layers
app.activeDocument.flatten()

// Apply Gaussian blur to the active layer
app.activeDocument.activeLayer.applyGaussianBlur(5.0)

// Get all layer names
var names = [];
for (var i = 0; i < app.activeDocument.layers.length; i++)
    names.push(app.activeDocument.layers[i].name);
names.join(", ");
```

## Project Structure

```
PhotoshopMcpServer/
├── .github/
│   └── copilot-instructions.md          # C# code style rules for Copilot
├── PhotoshopMcpServer/
│   ├── Program.cs                       # MCP server entry point (stdio transport)
│   ├── Models/
│   │   └── PhotoshopModels.cs           # Record types for results and document info
│   ├── Services/
│   │   ├── IPhotoshopService.cs         # Photoshop COM service interface
│   │   └── PhotoshopService.cs          # COM automation implementation
│   └── Tools/
│       └── PhotoshopTools.cs            # MCP tool definitions (13 tools)
├── PhotoshopMcpServer.Tests/
│   ├── PhotoshopToolsTests.cs           # Tool unit tests (28 tests)
│   └── PhotoshopServiceTests.cs         # Model tests (7 tests)
├── PhotoshopMcpServer.slnx
├── LICENSE
└── README.md
```

## Running Tests

```bash
dotnet test
```

## How It Works

1. The MCP server starts and communicates over **stdio** (standard input/output)
2. An AI client connects and discovers the available tools
3. When the AI invokes a tool, the server uses **Windows COM automation** to send commands to Photoshop
4. Photoshop executes the command (typically via `DoJavaScript`) and returns the result
5. The result is sent back to the AI client

```
AI Client ←→ MCP (stdio) ←→ PhotoshopMcpServer ←→ COM ←→ Adobe Photoshop
```

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgements

- [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) — Official MCP SDK for .NET
- [GitHub Copilot](https://github.com/features/copilot) — AI-assisted development of this project

## Author

**Howon Lee** ([@airtaxi](https://github.com/airtaxi))
