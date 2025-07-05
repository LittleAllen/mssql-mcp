namespace GitLabCli.Shared.Configuration;

/// <summary>
/// GitLab 相關配置選項
/// </summary>
public class GitLabOptions
{
    public const string SectionName = "GitLab";
    
    /// <summary>
    /// GitLab 基礎 URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://gitlab.com";
    
    /// <summary>
    /// GitLab 存取權杖
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// 專案 ID
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// API 請求逾時時間
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// 最大重試次數
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
}
