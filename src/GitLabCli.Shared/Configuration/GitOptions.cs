namespace GitLabCli.Shared.Configuration;

/// <summary>
/// Git 相關配置選項
/// </summary>
public class GitOptions
{
    public const string SectionName = "Git";
    
    /// <summary>
    /// 預設分支名稱
    /// </summary>
    public string DefaultBranch { get; set; } = "main";
    
    /// <summary>
    /// 是否啟用自動同步
    /// </summary>
    public bool AutoSync { get; set; } = true;
    
    /// <summary>
    /// 衝突解決策略
    /// </summary>
    public string ConflictStrategy { get; set; } = "manual";
    
    /// <summary>
    /// Git 倉庫路徑
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 排除的檔案模式
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = new();
}
