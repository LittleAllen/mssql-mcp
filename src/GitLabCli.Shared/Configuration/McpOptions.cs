namespace GitLabCli.Shared.Configuration;

/// <summary>
/// MCP 相關配置選項
/// </summary>
public class McpOptions
{
    public const string SectionName = "MCP";
    
    /// <summary>
    /// MCP Server URL
    /// </summary>
    public string ServerUrl { get; set; } = "http://localhost:5000";
    
    /// <summary>
    /// 客戶端 ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// 客戶端密鑰
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// API 版本
    /// </summary>
    public string ApiVersion { get; set; } = "v1";
    
    /// <summary>
    /// 連線逾時時間
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
