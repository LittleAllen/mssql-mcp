namespace GitLabCli.Shared.Models.Responses;

/// <summary>
/// Pull 操作結果
/// </summary>
public class PullResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 拉取的 Commit 數量
    /// </summary>
    public int CommitCount { get; set; }
    
    /// <summary>
    /// 分支名稱
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit SHA
    /// </summary>
    public string LatestCommitSha { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否有衝突
    /// </summary>
    public bool HasConflicts { get; set; }
    
    /// <summary>
    /// 衝突檔案清單
    /// </summary>
    public List<string> ConflictFiles { get; set; } = new();
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 處理時間
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
