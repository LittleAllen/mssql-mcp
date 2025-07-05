namespace GitLabCli.Shared.Models.Entities;

/// <summary>
/// Git Commit 資訊
/// </summary>
public class CommitInfo
{
    /// <summary>
    /// Commit SHA
    /// </summary>
    public string Sha { get; set; } = string.Empty;
    
    /// <summary>
    /// Commit 訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 作者名稱
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 作者電子郵件
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// 作者時間
    /// </summary>
    public DateTime AuthorDate { get; set; }
    
    /// <summary>
    /// 提交者名稱
    /// </summary>
    public string CommitterName { get; set; } = string.Empty;
    
    /// <summary>
    /// 提交者電子郵件
    /// </summary>
    public string CommitterEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// 提交時間
    /// </summary>
    public DateTime CommitterDate { get; set; }
    
    /// <summary>
    /// 變更的檔案清單
    /// </summary>
    public List<FileChange> ChangedFiles { get; set; } = new();
}

/// <summary>
/// 檔案變更資訊
/// </summary>
public class FileChange
{
    /// <summary>
    /// 檔案路徑
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 變更類型 (Added, Modified, Deleted, Renamed)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;
    
    /// <summary>
    /// 舊檔案路徑 (重新命名時使用)
    /// </summary>
    public string? OldFilePath { get; set; }
    
    /// <summary>
    /// 新增行數
    /// </summary>
    public int AddedLines { get; set; }
    
    /// <summary>
    /// 刪除行數
    /// </summary>
    public int DeletedLines { get; set; }
}
