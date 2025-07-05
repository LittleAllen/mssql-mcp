# GitLab CLI MCP 專案測試設置指南

這份文件描述如何執行專案的自動化測試。

## 測試架構

本專案採用四層測試架構：

- **GitLabCli.Shared.Tests** - 共用函式庫單元測試
- **GitLabCli.MCP.Client.Tests** - CLI 命令單元測試  
- **GitLabCli.MCP.Server.Tests** - API Server 單元測試
- **GitLabCli.Integration.Tests** - 整合測試

## 前置需求

### 基本環境
- .NET 8.0 SDK
- ASP.NET Core 8.0.0 執行時 (Server 和 Integration 測試需要)

### ASP.NET Core 執行時安裝
如果遇到 ASP.NET Core 版本錯誤，請安裝對應版本：

```bash
# Windows
winget install Microsoft.DotNet.AspNetCore.8

# 或下載安裝程式
# https://dotnet.microsoft.com/download/dotnet/8.0
```

## 快速開始

### 執行所有測試
```bash
# Windows
scripts\run-all-tests.bat

# Linux/macOS  
scripts/run-all-tests.sh
```

### 執行個別測試專案
```bash
# Shared 專案測試 (核心功能)
dotnet test tests/GitLabCli.Shared.Tests/

# Client 專案測試 (CLI 命令)
dotnet test tests/GitLabCli.MCP.Client.Tests/

# Server 專案測試 (需要 ASP.NET Core 8.0)
dotnet test tests/GitLabCli.MCP.Server.Tests/

# 整合測試 (需要 ASP.NET Core 8.0)
dotnet test tests/GitLabCli.Integration.Tests/
```

### 產生覆蓋率報告
```bash
# 執行測試並收集覆蓋率
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# 產生 HTML 報告
dotnet reportgenerator \
  -reports:"TestResults/**/*.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:"HtmlInline_AzurePipelines;Cobertura;TextSummary"

# 查看報告
# Windows: TestResults\CoverageReport\index.html
# Linux/macOS: TestResults/CoverageReport/index.html
```

## 測試框架與工具

- **xUnit 2.8.2** - 主要測試框架
- **Moq 4.20.72** - Mock 物件框架
- **FluentAssertions 6.12.1** - 流暢的斷言語法
- **Coverlet.Collector 6.0.2** - 程式碼覆蓋率收集
- **ReportGenerator 5.3.11** - 覆蓋率報告產生器

## 目前測試狀態

### ✅ 可正常執行的測試
- **GitLabCli.Shared.Tests**: 44 個測試，100% 通過
- **GitLabCli.MCP.Client.Tests**: 22 個測試，100% 通過

### ⚠️ 執行時問題
- **GitLabCli.MCP.Server.Tests**: ASP.NET Core 版本不匹配
- **GitLabCli.Integration.Tests**: ASP.NET Core 版本不匹配

### 📊 覆蓋率統計
- 整體行覆蓋率: 13.6%
- 方法覆蓋率: 21.4%
- 已測試的核心組件達到良好覆蓋率

## 測試資料與 Mock

### Mock 服務
- `MockGitLabService` - GitLab API 模擬
- `MockGitService` - Git 操作模擬
- `MockMcpClient` - MCP 用戶端模擬

### 測試資料建構器
使用 `TestDataBuilders` 類別建立一致的測試資料：

```csharp
var project = TestDataBuilders.CreateProjectInfo()
    .WithId(123)
    .WithName("測試專案")
    .Build();
```

## 常見問題

### Q: 為什麼 Server 測試無法執行？
A: Server 測試需要 ASP.NET Core 8.0.0 執行時，請安裝對應版本。

### Q: 如何提升覆蓋率？
A: 當前覆蓋率較低是因為只有部分專案能執行測試。安裝 ASP.NET Core 8.0 後，覆蓋率會顯著提升。

### Q: 測試執行很慢怎麼辦？
A: 可以使用 `--no-build` 參數跳過重複建構，或執行特定測試專案。

## 開發指南

### 新增測試
1. 在對應的測試專案中建立測試類別
2. 繼承 `TestBase` 取得常用功能
3. 使用 `TestDataBuilders` 建立測試資料
4. 遵循 AAA 模式 (Arrange, Act, Assert)

### 測試命名規範
- 測試類別: `{ClassName}Tests`
- 測試方法: `{MethodName}_{Scenario}_{ExpectedResult}`

範例:
```csharp
[Fact]
public async Task GetProjectAsync_WithValidId_ShouldReturnProject()
{
    // Arrange
    const int projectId = 123;
    
    // Act
    var result = await _service.GetProjectAsync(projectId);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(projectId);
}
```

## 進階功能

### 偵錯測試
```bash
# 在 VS Code 中偵錯測試
# 1. 在測試方法設定中斷點
# 2. 使用 "Debug Test" CodeLens
# 3. 或使用測試總管面板
```

### 效能測試
```bash
# 執行效能測試 (如果有實作)
dotnet test --filter Category=Performance
```

### 平行測試
測試預設會平行執行以提升速度，如需關閉：

```bash
dotnet test -- xUnit.ParallelizeAssembly=false
```

---

如有測試相關問題，請參考 `docs/Testing-Guide.md` 或查看 `docs/Testing-Progress-Report.md` 了解詳細測試狀態。
