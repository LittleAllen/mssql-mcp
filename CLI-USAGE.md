# GitLab CLI MCP Client 使用指南

這是一個基於 Model Context Protocol (MCP) 的 GitLab CLI 工具，讓您可以在命令列中管理 GitLab 專案和 Git 倉庫。

## 建構和執行

### 建構專案
```bash
dotnet build
```

### 執行 CLI
```bash
cd src/GitLabCli.MCP.Client/bin/Debug/net8.0
./gitlab-cli.exe
```

## 可用命令

### 1. 專案管理

#### 取得專案清單
```bash
gitlab-cli project list
```

#### 取得專案詳細資訊
```bash
gitlab-cli project info --project-id 123
```

### 2. 分支管理

#### 取得分支清單
```bash
gitlab-cli branch list --project-id 123
```

#### 建立新分支
```bash
gitlab-cli branch create --project-id 123 --name "feature/new-feature" --source "main"
```

#### 切換分支
```bash
gitlab-cli branch checkout --name "develop" --repo "/path/to/repo"
```

### 3. Git 同步

#### 推送變更
```bash
gitlab-cli sync push --repo "/path/to/repo" --branch "main"
```

#### 拉取變更
```bash
gitlab-cli sync pull --repo "/path/to/repo" --branch "main"
```

#### 強制推送/拉取
```bash
gitlab-cli sync push --force
gitlab-cli sync pull --force
```

### 4. 狀態查詢

#### 取得倉庫狀態
```bash
gitlab-cli status repo --repo "/path/to/repo"
```

## 全域選項

- `--verbose` 或 `--debug`: 顯示詳細輸出
- `--config <path>`: 指定配置檔案路徑
- `--help`: 顯示說明資訊
- `--version`: 顯示版本資訊

## 配置

### appsettings.json
```json
{
  "GitLab": {
    "BaseUrl": "https://gitlab.com/api/v4",
    "AccessToken": "your-access-token",
    "ProjectId": 0,
    "Timeout": "00:00:30"
  },
  "Git": {
    "DefaultBranch": "main",
    "AutoPush": false,
    "AutoPull": false
  },
  "Mcp": {
    "ServerUrl": "http://localhost:5000",
    "Timeout": "00:01:00",
    "RetryCount": 3
  }
}
```

### 開發環境配置 (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "GitLab": {
    "BaseUrl": "http://localhost:8080/api/v4",
    "AccessToken": "dev-token-123456"
  }
}
```

## 架構特色

- **模組化設計**: 使用 Command Pattern 和依賴注入
- **錯誤處理**: 統一的例外處理機制
- **美觀輸出**: 使用 Spectre.Console 提供豐富的表格和面板顯示
- **非同步操作**: 所有 I/O 操作都是非同步的
- **模擬服務**: 包含完整的模擬服務用於開發和測試

## 開發注意事項

1. 所有命令都繼承自 `BaseCommand`，提供統一的錯誤處理和輸出格式
2. 使用依賴注入管理服務相依性
3. 配置支援多環境 (Development, Production)
4. 遵循 .NET 8 和 System.CommandLine 最佳實務

## 擴展功能

要新增新命令：

1. 建立新的命令類別繼承 `BaseCommand`
2. 在主要命令群組中註冊新命令
3. 在 `Program.cs` 中註冊相依服務

## 錯誤碼

- `0`: 成功
- `1`: 一般錯誤
- `2`: 有衝突但操作完成

## 範例使用流程

```bash
# 1. 查看可用專案
gitlab-cli project list

# 2. 查看特定專案的分支
gitlab-cli branch list --project-id 1

# 3. 檢查本地倉庫狀態
gitlab-cli status repo

# 4. 推送變更
gitlab-cli sync push

# 5. 拉取最新變更
gitlab-cli sync pull
```

這個 CLI 工具為 GitLab 和 Git 操作提供了一個統一的命令列介面，讓開發者能夠更高效地管理專案和程式碼。
