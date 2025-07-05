using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.Shared.Interfaces;

/// <summary>
/// Git 操作服務介面
/// </summary>
public interface IGitService
{
    /// <summary>
    /// 處理 Push 操作
    /// </summary>
    /// <param name="pushRequest">Push 請求參數</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Push 操作結果</returns>
    Task<PushResult> ProcessPushAsync(PushRequest pushRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 處理 Pull 操作
    /// </summary>
    /// <param name="pullRequest">Pull 請求參數</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Pull 操作結果</returns>
    Task<PullResult> ProcessPullAsync(PullRequest pullRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得本地倉庫狀態
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>倉庫狀態</returns>
    Task<RepositoryStatus> GetRepositoryStatusAsync(string repositoryPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得本地分支清單
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    Task<IEnumerable<BranchInfo>> GetLocalBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 切換分支
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>是否成功</returns>
    Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 解決衝突
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="conflictFiles">衝突檔案</param>
    /// <param name="strategy">解決策略</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>解決結果</returns>
    Task<bool> ResolveConflictsAsync(string repositoryPath, IEnumerable<string> conflictFiles, string strategy, CancellationToken cancellationToken = default);
}

/// <summary>
/// 倉庫狀態
/// </summary>
public class RepositoryStatus
{
    /// <summary>
    /// 目前分支
    /// </summary>
    public string CurrentBranch { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否有未提交的變更
    /// </summary>
    public bool HasUncommittedChanges { get; set; }
    
    /// <summary>
    /// 未追蹤的檔案數量
    /// </summary>
    public int UntrackedFilesCount { get; set; }
    
    /// <summary>
    /// 已修改的檔案數量
    /// </summary>
    public int ModifiedFilesCount { get; set; }
    
    /// <summary>
    /// 已暫存的檔案數量
    /// </summary>
    public int StagedFilesCount { get; set; }
    
    /// <summary>
    /// 是否有衝突
    /// </summary>
    public bool HasConflicts { get; set; }
    
    /// <summary>
    /// 衝突檔案清單
    /// </summary>
    public List<string> ConflictFiles { get; set; } = new();
}
