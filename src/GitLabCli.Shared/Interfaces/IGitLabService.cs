using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.Shared.Interfaces;

/// <summary>
/// GitLab 服務介面
/// </summary>
public interface IGitLabService
{
    /// <summary>
    /// 取得專案資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案資訊</returns>
    Task<ProjectInfo> GetProjectAsync(int projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得專案清單
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案清單</returns>
    Task<IEnumerable<ProjectInfo>> GetProjectsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得分支清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    Task<IEnumerable<BranchInfo>> GetBranchesAsync(int projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得分支資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支資訊</returns>
    Task<BranchInfo> GetBranchAsync(int projectId, string branchName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 建立分支
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="sourceBranch">來源分支</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>建立的分支資訊</returns>
    Task<BranchInfo> CreateBranchAsync(int projectId, string branchName, string sourceBranch, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得 Commit 清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Commit 清單</returns>
    Task<IEnumerable<CommitInfo>> GetCommitsAsync(int projectId, string branchName, CancellationToken cancellationToken = default);
}
