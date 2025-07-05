namespace GitLabCli.Shared.Models.Entities;

/// <summary>
/// Git 分支資訊
/// </summary>
public class BranchInfo
{
    /// <summary>
    /// 分支名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit SHA
    /// </summary>
    public string CommitSha { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit 訊息
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit 作者
    /// </summary>
    public string CommitAuthor { get; set; } = string.Empty;
    
    /// <summary>
    /// 最新 Commit 時間
    /// </summary>
    public DateTime CommitDate { get; set; }
    
    /// <summary>
    /// 是否為預設分支
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// 是否受保護
    /// </summary>
    public bool IsProtected { get; set; }
}
