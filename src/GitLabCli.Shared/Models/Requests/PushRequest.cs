using GitLabCli.Shared.Models.Entities;

namespace GitLabCli.Shared.Models.Requests;

/// <summary>
/// Git Push 請求
/// </summary>
public class PushRequest
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
    /// Commit 清單
    /// </summary>
    public List<CommitInfo> Commits { get; set; } = new();
    
    /// <summary>
    /// 是否強制推送
    /// </summary>
    public bool Force { get; set; }
    
    /// <summary>
    /// 推送標籤
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
