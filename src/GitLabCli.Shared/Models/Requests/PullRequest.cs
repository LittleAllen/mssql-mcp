using GitLabCli.Shared.Models.Entities;

namespace GitLabCli.Shared.Models.Requests;

/// <summary>
/// Git Pull 請求
/// </summary>
public class PullRequest
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// 分支名稱
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
    
    /// <summary>
    /// 目標本地路徑
    /// </summary>
    public string LocalPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否強制拉取
    /// </summary>
    public bool Force { get; set; }
    
    /// <summary>
    /// 衝突解決策略
    /// </summary>
    public string ConflictStrategy { get; set; } = "manual";
}
