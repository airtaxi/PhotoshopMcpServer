# Photoshop MCP 서버

🌐 **언어**: [English](README.md) | **한국어**

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

실행 파일 위치: `PhotoshopMcpServer/bin/Release/net10.0-windows/win-x64/publish/`

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

### ExecuteJavaScript — 핵심 도구

`ExecuteJavaScript` 도구는 의도적으로 유연하게 설계되었습니다. AI가 유효한 Photoshop JavaScript를 직접 구성하고 실행할 수 있어 Photoshop에 대한 완전한 제어권을 가집니다. 스크립트 예시:

```javascript
// 문서 이름 가져오기
app.activeDocument.name

// 새 문서 생성
app.documents.add(1920, 1080, 72, "My Canvas")

// 활성 문서 크기 변경
app.activeDocument.resizeImage(800, 600)

// 모든 레이어 병합
app.activeDocument.flatten()

// 활성 레이어에 가우시안 블러 적용
app.activeDocument.activeLayer.applyGaussianBlur(5.0)

// 모든 레이어 이름 가져오기
var names = [];
for (var i = 0; i < app.activeDocument.layers.length; i++)
    names.push(app.activeDocument.layers[i].name);
names.join(", ");
```

## 프로젝트 구조

```
PhotoshopMcpServer/
├── .github/
│   └── copilot-instructions.md          # Copilot용 C# 코드 스타일 규칙
├── PhotoshopMcpServer/
│   ├── Program.cs                       # MCP 서버 진입점 (stdio 전송)
│   ├── Models/
│   │   └── PhotoshopModels.cs           # 결과 및 문서 정보 레코드 타입
│   ├── Services/
│   │   ├── IPhotoshopService.cs         # Photoshop COM 서비스 인터페이스
│   │   └── PhotoshopService.cs          # COM 자동화 구현체
│   └── Tools/
│       └── PhotoshopTools.cs            # MCP 도구 정의 (13개 도구)
├── PhotoshopMcpServer.Tests/
│   ├── PhotoshopToolsTests.cs           # 도구 단위 테스트 (28개)
│   └── PhotoshopServiceTests.cs         # 모델 테스트 (7개)
├── PhotoshopMcpServer.slnx
├── LICENSE
└── README.md
```

## 테스트 실행

```bash
dotnet test
```

## 동작 방식

1. MCP 서버가 시작되어 **stdio** (표준 입출력)를 통해 통신합니다
2. AI 클라이언트가 연결하고 사용 가능한 도구를 탐색합니다
3. AI가 도구를 호출하면 서버가 **Windows COM 자동화**로 Photoshop에 명령을 전달합니다
4. Photoshop이 명령을 실행하고 (`DoJavaScript`) 결과를 반환합니다
5. 결과가 AI 클라이언트에게 전달됩니다

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
