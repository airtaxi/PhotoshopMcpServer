# Photoshop MCP Server

> **[🇰🇷 한국어 README는 여기를 클릭하세요](#한국어)**

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

---

<a id="한국어"></a>

## 🇰🇷 한국어

# Photoshop MCP 서버

AI 어시스턴트가 Windows COM 자동화를 통해 Adobe Photoshop을 제어할 수 있게 해주는 [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) 서버입니다. .NET 10과 C# 14로 개발되었으며 공식 [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)를 사용합니다.

## 개요

이 MCP 서버는 Photoshop 자동화를 MCP 호환 AI 클라이언트(Claude Desktop, GitHub Copilot 등)가 호출할 수 있는 도구 세트로 제공합니다. 핵심 도구인 `ExecuteJavaScript`는 AI에게 Photoshop 스크립팅 엔진에 대한 완전한 접근 권한을 부여하여 런타임에 유연하게 작업을 결정할 수 있게 합니다.

## 요구 사항

- **Windows** (COM 자동화는 Windows 전용)
- **Adobe Photoshop** (COM 자동화 및 `DoJavaScript`를 지원하는 버전)
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** 이상

## 설치

### 1. 저장소 복제

```bash
git clone https://github.com/airtaxi/PhotoshopMcpServer.git
cd PhotoshopMcpServer
```

### 2. 빌드

```bash
dotnet build
```

### 3. (선택) 자체 포함 실행 파일로 게시

```bash
dotnet publish PhotoshopMcpServer/PhotoshopMcpServer.csproj -c Release -r win-x64 --self-contained
```

## MCP 서버 설정

### Claude Desktop

Claude Desktop 설정 파일에 다음을 추가하세요:

- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

#### 방법 A: 소스에서 실행 (.NET SDK 필요)

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\경로\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

#### 방법 B: 게시된 실행 파일 사용

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "C:\\경로\\PhotoshopMcpServer.exe"
    }
  }
}
```

### GitHub Copilot (VS Code)

`.vscode/mcp.json` 또는 VS Code 설정에 추가:

```json
{
  "servers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\경로\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

### Cursor

Cursor MCP 설정 (`~/.cursor/mcp.json`)에 추가:

```json
{
  "mcpServers": {
    "photoshop": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\경로\\PhotoshopMcpServer\\PhotoshopMcpServer"]
    }
  }
}
```

> **참고**: `C:\경로\PhotoshopMcpServer`를 실제 저장소를 복제한 경로로 변경하세요.

## 사용 가능한 도구

| 도구 | 설명 |
|------|------|
| `ExecuteJavaScript` | **핵심 도구** — Photoshop 스크립팅 엔진에서 임의의 JavaScript 실행 |
| `IsPhotoshopRunning` | Photoshop 실행 및 접근 가능 여부 확인 |
| `LaunchPhotoshop` | Photoshop 실행 또는 실행 중인 인스턴스에 연결 |
| `GetPhotoshopVersion` | Photoshop 버전 문자열 가져오기 |
| `GetActiveDocumentInfo` | 활성 문서 정보 (이름, 크기, 색상 모드, 해상도) |
| `GetOpenDocuments` | 열린 문서 이름 목록 |
| `OpenDocument` | 경로로 이미지 파일 열기 |
| `SaveActiveDocument` | 현재 문서 저장 |
| `CreateNewDocument` | 지정 크기로 새 문서 생성 |
| `ExportAsPng` | 활성 문서를 PNG로 내보내기 |
| `ExportAsJpeg` | 활성 문서를 JPEG로 내보내기 (품질 설정 가능) |
| `GetLayerInfo` | 활성 문서의 모든 레이어 요약 |

## 동작 방식

```
AI 클라이언트 ←→ MCP (stdio) ←→ PhotoshopMcpServer ←→ COM ←→ Adobe Photoshop
```

## 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE)를 따릅니다.

## 감사의 말

- [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) — .NET 공식 MCP SDK
- [GitHub Copilot](https://github.com/features/copilot) — 이 프로젝트의 AI 지원 개발

## 제작자

**이호원 (Howon Lee)** ([@airtaxi](https://github.com/airtaxi))
