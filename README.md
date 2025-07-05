# GitLab CLI MCP 專案

基於 MCP (Model Context Protocol) 的 GitLab CLI 工具，用於實現地端 Git 倉庫與遠端 GitLab 平台之間的資料同步。

## 專案架構

```
gitlab-cli-mcp/
├── src/
│   ├── GitLabCli.MCP.Server/          # MCP Server 專案 (ASP.NET Core)
│   ├── GitLabCli.MCP.Client/          # MCP Client 專案 (CLI 工具)
│   └── GitLabCli.Shared/              # 共用函式庫
├── tests/                             # 測試專案
├── docs/                              # 文件
└── scripts/                           # 建構和部署腳本
```

## 快速開始

### 先決條件

- .NET 8.0 SDK
- Git
- GitLab 帳號和 Access Token

### 建構專案

```bash
# 複製倉庫
git clone <repository-url>
cd gitlab-cli-mcp

# 還原 NuGet 套件
dotnet restore

# 建構解決方案
dotnet build
```

### 設定

1. 複製配置檔案範本：
```bash
cp src/GitLabCli.MCP.Server/appsettings.json src/GitLabCli.MCP.Server/appsettings.Development.json
```

2. 編輯 `appsettings.Development.json`，設定您的 GitLab 資訊：
```json
{
  "GitLab": {
    "BaseUrl": "https://gitlab.com",
    "AccessToken": "您的GitLab存取權杖",
    "ProjectId": 您的專案ID
  }
}
```

### 執行 MCP Server

```bash
cd src/GitLabCli.MCP.Server
dotnet run
```

Server 會在 `http://localhost:5000` 啟動，Swagger UI 可在根路徑存取。

### 使用 CLI 工具

```bash
cd src/GitLabCli.MCP.Client
dotnet run -- --help
```

## API 文件

MCP Server 提供以下主要 API：

### GitLab API
- `GET /api/v1/gitlab/projects` - 取得專案清單
- `GET /api/v1/gitlab/projects/{id}` - 取得專案資訊
- `GET /api/v1/gitlab/projects/{id}/branches` - 取得分支清單
- `POST /api/v1/gitlab/projects/{id}/branches` - 建立分支

### Git 操作 API
- `POST /api/v1/git/push` - 處理 Push 操作
- `POST /api/v1/git/pull` - 處理 Pull 操作
- `GET /api/v1/git/status` - 取得倉庫狀態
- `GET /api/v1/git/branches` - 取得本地分支清單

## CLI 命令

### 基本命令
```bash
gitlab-cli init                        # 初始化專案設定
gitlab-cli config set-token <token>    # 設定 GitLab token
gitlab-cli config set-server <url>     # 設定 MCP server URL
```

### 同步命令
```bash
gitlab-cli sync push                   # 手動觸發 push 同步
gitlab-cli sync pull                   # 手動觸發 pull 同步
gitlab-cli sync status                 # 查看同步狀態
```

### 專案管理
```bash
gitlab-cli project list                # 列出可用專案
gitlab-cli project connect <id>       # 連接到指定專案
gitlab-cli project info                # 顯示專案資訊
```

## 開發

### 建構

```bash
# 建構所有專案
dotnet build

# 建構特定專案
dotnet build src/GitLabCli.MCP.Server/
dotnet build src/GitLabCli.MCP.Client/
```

### 測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試專案
dotnet test tests/GitLabCli.Tests/
```

### 發布

```bash
# 發布 Server
dotnet publish src/GitLabCli.MCP.Server/ -c Release -o publish/server

# 發布 Client
dotnet publish src/GitLabCli.MCP.Client/ -c Release -o publish/client
```

## Docker 支援

### 建構 Docker 映像

```bash
# 建構 Server 映像
docker build -f src/GitLabCli.MCP.Server/Dockerfile -t gitlab-cli-mcp-server .

# 執行 Server 容器
docker run -p 5000:5000 gitlab-cli-mcp-server
```

## 貢獻

1. Fork 本專案
2. 建立功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交變更 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 開啟 Pull Request

## 授權

本專案使用 MIT 授權條款。詳見 [LICENSE](LICENSE) 檔案。

## 支援

如有問題或建議，請在 [Issues](../../issues) 頁面提出。
