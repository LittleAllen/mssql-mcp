namespace GitLabCli.Shared.Models.Responses;

/// <summary>
/// Push 操作結果
/// </summary>
public class PushResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 推送的 Commit 數量
    /// </summary>
    public int CommitCount { get; set; }
    
    /// <summary>
    /// 推送的分支名稱
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit SHA
    /// </summary>
    public string LatestCommitSha { get; set; } = string.Empty;
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 處理時間
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
