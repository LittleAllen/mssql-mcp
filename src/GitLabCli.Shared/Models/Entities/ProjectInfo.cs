namespace GitLabCli.Shared.Models.Entities;

/// <summary>
/// GitLab 專案資訊
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 專案名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 專案描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Git HTTP URL
    /// </summary>
    public string HttpUrlToRepo { get; set; } = string.Empty;
    
    /// <summary>
    /// Git SSH URL
    /// </summary>
    public string SshUrlToRepo { get; set; } = string.Empty;
    
    /// <summary>
    /// 預設分支
    /// </summary>
    public string DefaultBranch { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否為私有專案
    /// </summary>
    public bool IsPrivate { get; set; }
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime LastActivityAt { get; set; }
}
