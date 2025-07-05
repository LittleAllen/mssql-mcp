# GitLab Service 實作說明

## 概述
本文件說明了對 `GitLabService.cs` 進行的實作，將原本的模擬資料替換為真實的 GitLab API 呼叫。

## 主要變更

### 1. 新增相依性和配置
- 新增 `IHttpClientFactory` 相依性注入
- 新增 JSON 序列化選項配置（使用 snake_case 命名策略以符合 GitLab API）
- 新增 HTTP 客戶端建立和配置方法

### 2. 實作的 API 方法

#### GetProjectAsync
- **功能**: 取得單一專案資訊
- **API 端點**: `GET /api/v4/projects/{id}`
- **特色**: 包含完整的錯誤處理和 HTTP 狀態碼檢查

#### GetProjectsAsync  
- **功能**: 取得使用者的專案清單
- **API 端點**: `GET /api/v4/projects?membership=true&simple=true&per_page=100`
- **特色**: 只取得使用者有權限的專案，使用簡化格式提升效能

#### GetBranchesAsync
- **功能**: 取得專案的分支清單
- **API 端點**: `GET /api/v4/projects/{id}/repository/branches`
- **特色**: 包含分支的 commit 資訊和保護狀態

#### GetBranchAsync
- **功能**: 取得單一分支詳細資訊
- **API 端點**: `GET /api/v4/projects/{id}/repository/branches/{branch}`
- **特色**: URL 編碼分支名稱以處理特殊字元

#### CreateBranchAsync
- **功能**: 建立新分支
- **API 端點**: `POST /api/v4/projects/{id}/repository/branches`
- **特色**: 支援從指定來源分支建立新分支

#### GetCommitsAsync
- **功能**: 取得分支的 commit 清單
- **API 端點**: 
  - `GET /api/v4/projects/{id}/repository/commits?ref_name={branch}`
  - `GET /api/v4/projects/{id}/repository/commits/{sha}` (詳細資訊)
  - `GET /api/v4/projects/{id}/repository/commits/{sha}/diff` (檔案差異)
- **特色**: 
  - 取得完整的 commit 資訊包含檔案變更
  - 解析 diff 內容計算新增/刪除行數
  - 判斷檔案變更類型（新增、修改、刪除、重新命名）

### 3. 錯誤處理
- **GitLabApiException**: 自訂例外類別處理 API 錯誤
- **網路錯誤**: 處理 `HttpRequestException`
- **逾時處理**: 處理 `TaskCanceledException`
- **HTTP 狀態碼**: 檢查回應狀態並提供詳細錯誤資訊

### 4. 新增的 DTO 類別
為了與 GitLab API 互動，新增了以下 DTO 類別：

- `GitLabProjectDto`: 專案資訊
- `GitLabBranchDto`: 分支資訊  
- `GitLabCommitDto`: 基本 commit 資訊
- `GitLabCommitDetailDto`: 詳細 commit 資訊（包含統計）
- `GitLabStatsDto`: commit 統計資訊
- `GitLabDiffDto`: 檔案差異資訊

### 5. 輔助方法
- `CreateHttpClient()`: 建立配置好的 HTTP 客戶端
- `HandleResponseAsync()`: 統一處理 HTTP 回應和錯誤
- `DetermineChangeType()`: 判斷檔案變更類型
- `ParseLineCount()`: 解析 diff 內容中的行數變更

## 配置需求

在 `appsettings.json` 中需要設定：

```json
{
  "GitLab": {
    "BaseUrl": "https://gitlab.com",
    "AccessToken": "your-access-token",
    "ProjectId": 123,
    "Timeout": "00:00:30",
    "MaxRetryCount": 3
  }
}
```

## 安全性考量

1. **存取權杖**: 建議使用環境變數或 Azure Key Vault 儲存敏感資訊
2. **HTTPS**: 所有 API 呼叫都透過 HTTPS 進行
3. **逾時設定**: 避免長時間等待的請求
4. **錯誤資訊**: 不洩露敏感的內部資訊

## 效能最佳化

1. **HTTP 客戶端工廠**: 使用 `IHttpClientFactory` 管理連線池
2. **非同步操作**: 所有 I/O 操作都是非同步的
3. **適當的逾時**: 避免無限等待
4. **分頁支援**: 大型清單使用分頁限制資料量

## 後續改進建議

1. **快取機制**: 實作記憶體快取以減少 API 呼叫
2. **重試策略**: 使用 Polly 實作指數退避重試
3. **批次處理**: 對於大量操作提供批次 API
4. **監控和指標**: 新增 API 呼叫的效能監控
5. **單元測試**: 新增完整的單元測試覆蓋
