# 測試指南

## 測試專案結構

```
tests/
├── GitLabCli.Shared.Tests/         # 共用函式庫測試
│   ├── Configuration/               # 配置類別測試
│   ├── Exceptions/                  # 例外類別測試
│   ├── Models/                      # 模型測試
│   └── TestHelpers/                 # 測試輔助工具
├── GitLabCli.MCP.Server.Tests/      # Server 單元測試
│   ├── Controllers/                 # 控制器測試
│   └── Services/                    # 服務測試
├── GitLabCli.MCP.Client.Tests/      # Client 單元測試
│   └── Commands/                    # 命令測試
└── GitLabCli.Integration.Tests/     # 整合測試
    ├── Api/                         # API 整合測試
    └── Commands/                    # CLI 整合測試
```

## 測試框架和工具

### 使用的測試框架
- **xUnit**: 主要測試框架
- **FluentAssertions**: 流暢的斷言語法
- **Moq**: Mock 物件框架
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core 整合測試

### 程式碼覆蓋率工具
- **coverlet**: 程式碼覆蓋率收集
- **ReportGenerator**: 程式碼覆蓋率報告產生

## 執行測試

### 1. 執行所有測試
```bash
dotnet test
```

### 2. 執行特定測試專案
```bash
# 共用函式庫測試
dotnet test tests/GitLabCli.Shared.Tests/

# Server 測試
dotnet test tests/GitLabCli.MCP.Server.Tests/

# Client 測試
dotnet test tests/GitLabCli.MCP.Client.Tests/

# 整合測試
dotnet test tests/GitLabCli.Integration.Tests/
```

### 3. 執行測試並產生覆蓋率報告
```bash
# 使用腳本執行完整測試套件
scripts/run-tests.bat  # Windows
scripts/run-tests.sh   # Linux/macOS
```

### 4. 使用特定設定執行測試
```bash
dotnet test --settings tests/test.runsettings --collect:"XPlat Code Coverage"
```

## 測試策略

### 單元測試
- **範圍**: 個別類別和方法
- **目標**: 驗證單一功能的正確性
- **Mock**: 隔離外部相依性
- **命名**: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task GetProjectAsync_ValidProjectId_ReturnsProjectInfo()
{
    // Arrange
    var mockService = new Mock<IGitLabService>();
    // ... 設定 Mock

    // Act
    var result = await service.GetProjectAsync(123);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(123);
}
```

### 整合測試
- **範圍**: 多個元件協作
- **目標**: 驗證系統整合功能
- **環境**: 使用測試環境設定
- **資料**: 使用測試資料或 Mock 服務

```csharp
[Fact]
public async Task GetProjects_ShouldReturnValidApiResponse()
{
    // 使用 WebApplicationFactory 測試完整的 HTTP 請求
    var response = await _client.GetAsync("/api/v1/gitlab/projects");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 測試資料建構器

使用建構器模式建立測試資料：

```csharp
var project = TestData.ProjectInfo()
    .WithId(123)
    .WithName("測試專案")
    .AsPrivate()
    .Build();
```

## Mock 服務

### GitLab Mock 服務
- 提供模擬的 GitLab API 回應
- 支援所有主要的 GitLab 操作
- 用於開發和測試環境

### Git Mock 服務
- 提供模擬的 Git 操作
- 避免實際的 Git 儲存庫操作
- 加速測試執行

## 測試最佳實踐

### 1. AAA 模式
```csharp
[Fact]
public void Test_Method()
{
    // Arrange - 準備測試資料和環境
    var input = "test";
    
    // Act - 執行要測試的操作
    var result = Method(input);
    
    // Assert - 驗證結果
    result.Should().Be("expected");
}
```

### 2. 使用 Theory 進行參數化測試
```csharp
[Theory]
[InlineData(1, "專案1")]
[InlineData(2, "專案2")]
public void Test_WithMultipleInputs(int id, string expectedName)
{
    // 測試邏輯
}
```

### 3. 適當的測試隔離
- 每個測試應該獨立執行
- 不依賴其他測試的執行順序
- 清理測試資源

### 4. 有意義的測試名稱
- 描述測試場景
- 說明預期結果
- 使用中文描述更清楚

## 程式碼覆蓋率目標

- **目標覆蓋率**: >= 80%
- **關鍵路徑**: >= 90%
- **排除項目**: 
  - 自動產生的程式碼
  - 設定檔案
  - 啟動程式碼

## 持續整合測試

### GitHub Actions 設定
```yaml
- name: 執行測試
  run: dotnet test --configuration Release --collect:"XPlat Code Coverage"

- name: 產生覆蓋率報告
  run: reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

## 疑難排解

### 常見問題

1. **測試逾時**
   - 增加測試逾時設定
   - 檢查非同步操作

2. **Mock 設定問題**
   - 確認 Mock 方法簽名正確
   - 檢查返回值型別

3. **整合測試失敗**
   - 確認測試環境設定
   - 檢查相依性注入

### 偵錯測試
```bash
# 詳細輸出
dotnet test --verbosity normal

# 偵錯模式
dotnet test --logger console --verbosity diagnostic
```

## 效能測試

對於重要的 API 端點，考慮新增效能測試：

```csharp
[Fact]
public async Task GetProjects_ShouldCompleteWithin500ms()
{
    var stopwatch = Stopwatch.StartNew();
    
    await _service.GetProjectsAsync();
    
    stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
}
```

## 安全性測試

驗證安全性相關功能：

```csharp
[Fact]
public async Task GetProjects_WithoutToken_ShouldReturnUnauthorized()
{
    _client.DefaultRequestHeaders.Authorization = null;
    
    var response = await _client.GetAsync("/api/v1/gitlab/projects");
    
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```
