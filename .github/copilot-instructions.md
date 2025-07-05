# GitLab CLI MCP 開發準則

## 專案概述
這是一個基於 MCP (Model Context Protocol) 的 GitLab CLI 工具專案，使用 .NET 8 開發，包含 Server 和 Client 兩個主要元件。

## 程式碼規範

### 命名規範
- **類別名稱**: PascalCase (例如: `GitLabService`, `MergeRequestModel`)
- **方法名稱**: PascalCase (例如: `ProcessPushAsync`, `GetProjectInfo`)
- **屬性名稱**: PascalCase (例如: `ProjectId`, `AccessToken`)
- **欄位名稱**: camelCase，私有欄位使用 `_` 前綴 (例如: `_httpClient`, `_logger`)
- **參數名稱**: camelCase (例如: `projectId`, `branchName`)
- **常數**: PascalCase (例如: `DefaultTimeout`, `MaxRetryCount`)
- **介面**: 使用 `I` 前綴 (例如: `IGitLabService`, `IMcpClient`)

### 檔案結構規範
```
專案/
├── Controllers/           # API 控制器 (僅 Server 專案)
├── Services/             # 業務邏輯服務
│   ├── Interfaces/       # 服務介面
│   └── Implementations/  # 服務實作
├── Models/              # 資料模型
│   ├── Requests/        # 請求模型
│   ├── Responses/       # 回應模型
│   └── Entities/        # 實體模型
├── Commands/            # CLI 命令 (僅 Client 專案)
├── Extensions/          # 擴展方法
├── Configuration/       # 配置類別
└── Exceptions/          # 自訂例外
```

## 架構設計原則

### 1. 相依性注入 (Dependency Injection)
- 所有服務必須使用介面抽象化
- 透過建構函式注入相依性
- 使用 Microsoft.Extensions.DependencyInjection

```csharp
// 正確示例
public class GitLabService : IGitLabService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitLabService> _logger;
    
    public GitLabService(IHttpClientFactory httpClientFactory, ILogger<GitLabService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
}
```

### 2. 非同步程式設計
- 所有 I/O 操作必須使用非同步方法
- 方法名稱以 `Async` 結尾
- 返回 `Task` 或 `Task<T>`
- 使用 `ConfigureAwait(false)` 避免死鎖

```csharp
public async Task<ProjectInfo> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
{
    var response = await _httpClient.GetAsync($"projects/{projectId}", cancellationToken).ConfigureAwait(false);
    return await response.Content.ReadFromJsonAsync<ProjectInfo>(cancellationToken: cancellationToken).ConfigureAwait(false);
}
```

### 3. 錯誤處理
- 使用自訂例外類別
- 實作全域例外處理中介軟體
- 記錄詳細錯誤資訊
- 對外 API 返回統一格式的錯誤回應

```csharp
public class GitLabApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    
    public GitLabApiException(int statusCode, string errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
```

### 4. 配置管理
- 使用 `IOptions<T>` 模式
- 支援環境變數覆蓋
- 敏感資訊使用 Secret Manager 或 Azure Key Vault

```csharp
public class GitLabOptions
{
    public const string SectionName = "GitLab";
    
    public string BaseUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

## 程式碼品質準則

### 1. SOLID 原則
- **單一職責原則**: 每個類別只負責一項功能
- **開放封閉原則**: 對擴展開放，對修改封閉
- **里氏替換原則**: 子類別可以替換父類別
- **介面隔離原則**: 使用小而專精的介面
- **相依性反轉原則**: 依賴抽象而非具體實作

### 2. 程式碼註解
- 公開 API 必須包含 XML 文件註解
- 複雜邏輯需要解釋性註解
- 使用 TODO 標記待辦事項

```csharp
/// <summary>
/// 處理 Git push 操作並同步到 GitLab
/// </summary>
/// <param name="pushRequest">Push 請求參數</param>
/// <param name="cancellationToken">取消權杖</param>
/// <returns>Push 操作結果</returns>
/// <exception cref="GitLabApiException">當 GitLab API 呼叫失敗時拋出</exception>
public async Task<PushResult> ProcessPushAsync(PushRequest pushRequest, CancellationToken cancellationToken = default)
{
    // TODO: 實作分支保護檢查
    // 驗證 push 請求參數
    if (pushRequest == null)
        throw new ArgumentNullException(nameof(pushRequest));
}
```

### 3. 效能考量
- 使用物件池模式重用昂貴物件
- 實作適當的快取策略
- 避免記憶體洩漏，正確處置資源
- 使用 `IMemoryCache` 或 `IDistributedCache`

## API 設計規範

### 1. RESTful API 設計
- 使用標準 HTTP 動詞 (GET, POST, PUT, DELETE)
- 使用複數名詞作為資源名稱
- 支援版本控制 (`/api/v1/`)
- 統一的回應格式

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProjectDto>>>> GetProjects(
        [FromQuery] ProjectQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        // 實作邏輯
    }
}
```

### 2. 回應格式標準化
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 3. 驗證和授權
- 使用 FluentValidation 進行模型驗證
- 實作 JWT 驗證機制
- API 限流和節流控制

## CLI 設計規範

### 1. 命令結構
- 使用 System.CommandLine 框架
- 命令採用動詞-名詞結構 (例如: `get projects`, `sync push`)
- 支援全域選項 (例如: `--verbose`, `--config`)

```csharp
public class SyncCommand : Command
{
    public SyncCommand() : base("sync", "同步 Git 和 GitLab 資料")
    {
        AddCommand(new PushCommand());
        AddCommand(new PullCommand());
    }
}
```

### 2. 使用者體驗
- 提供清晰的錯誤訊息
- 支援進度顯示
- 彩色輸出和表格格式化
- 支援 `--help` 和 `--version`

## 測試策略

### 1. 單元測試
- 使用 xUnit 測試框架
- 覆蓋率目標 >= 80%
- 使用 Moq 進行 Mock
- 測試命名: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task GetProjectAsync_ValidProjectId_ReturnsProjectInfo()
{
    // Arrange
    var mockHttpClient = new Mock<IHttpClientFactory>();
    var service = new GitLabService(mockHttpClient.Object, Mock.Of<ILogger<GitLabService>>());
    
    // Act
    var result = await service.GetProjectAsync(123);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(123, result.Id);
}
```

### 2. 整合測試
- 使用 WebApplicationFactory 測試 API
- 使用 TestContainers 進行資料庫測試
- 模擬外部服務呼叫

### 3. 端到端測試
- 使用真實的 GitLab 測試環境
- 自動化 CLI 命令測試
- 驗證完整工作流程

## 日誌記錄規範

### 1. 日誌等級使用
- **Trace**: 詳細的除錯資訊
- **Debug**: 開發除錯資訊
- **Information**: 一般資訊
- **Warning**: 警告訊息
- **Error**: 錯誤訊息
- **Critical**: 嚴重錯誤

### 2. 結構化日誌
```csharp
_logger.LogInformation("Processing push request for project {ProjectId} with {CommitCount} commits", 
    request.ProjectId, request.Commits.Count);
```

### 3. 日誌配置
- 使用 Serilog 作為日誌提供者
- 支援多種輸出目標 (Console, File, Seq)
- 生產環境自動輪替日誌檔案

## 安全性準則

### 1. 敏感資訊處理
- 永不記錄敏感資訊
- 使用 SecureString 處理密碼
- 實作資料加密和解密

### 2. API 安全
- 實作 CORS 政策
- 使用 HTTPS 傳輸
- API 限流和 DDoS 防護

### 3. 輸入驗證
- 驗證所有使用者輸入
- 防範 SQL 注入和 XSS 攻擊
- 使用 AntiXSS 函式庫

## 效能最佳化

### 1. HTTP 客戶端
- 使用 IHttpClientFactory
- 實作重試策略
- 配置連線池和逾時設定

### 2. 記憶體管理
- 使用 ArrayPool 重用陣列
- 實作物件池模式
- 正確處置 IDisposable 物件

### 3. 快取策略
- API 回應快取
- 設定適當的快取過期時間
- 使用快取鍵命名規範

## 部署和 DevOps

### 1. 容器化
- 使用多階段 Dockerfile
- 最小化映像檔大小
- 實作健康檢查端點

### 2. CI/CD 管線
- 自動化建構和測試
- 程式碼品質檢查 (SonarQube)
- 自動化部署流程

### 3. 監控和觀測
- 實作應用程式指標
- 健康檢查端點
- 分散式追蹤

## 開發工具和環境

### 1. 推薦工具
- **IDE**: Visual Studio 2022 或 JetBrains Rider
- **版本控制**: Git with GitFlow
- **API 測試**: Postman 或 REST Client
- **程式碼分析**: SonarLint

### 2. NuGet 套件規範
- 使用中央套件管理 (Central Package Management)
- 定期更新相依套件
- 安全性掃描

### 3. 開發環境設定
- 使用 .editorconfig 統一程式碼格式
- 配置 Git hooks 進行預提交檢查
- 使用 Docker Compose 進行本地開發

## 文件規範

### 1. README 檔案
- 專案簡介和功能說明
- 安裝和使用指南
- 開發環境設定
- 貢獻指南

### 2. API 文件
- 使用 Swagger/OpenAPI
- 提供範例請求和回應
- 錯誤碼說明

### 3. 程式碼文件
- XML 文件註解
- 架構決策記錄 (ADR)
- 變更日誌 (CHANGELOG)

## 品質門檻

### 1. 程式碼品質指標
- 程式碼覆蓋率 >= 80%
- 技術債務評級 <= A
- 複雜度分數 <= 10
- 重複程式碼 <= 3%

### 2. 效能指標
- API 回應時間 <= 500ms (95th percentile)
- CLI 命令回應時間 <= 2s
- 記憶體使用量 <= 100MB (正常操作)

### 3. 可靠性指標
- 系統可用性 >= 99.9%
- 錯誤率 <= 0.1%
- 平均修復時間 <= 1 小時

這些開發準則將確保專案程式碼的一致性、可維護性和高品質。所有開發人員都應遵循這些準則進行開發工作。
