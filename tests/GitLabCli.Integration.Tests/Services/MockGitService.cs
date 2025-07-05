using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.Integration.Tests.Services;

/// <summary>
/// Mock Git 服務 (整合測試用)
/// </summary>
public class MockGitService : IGitService
{
    /// <summary>
    /// 處理 Push 操作
    /// </summary>
    public async Task<PushResult> ProcessPushAsync(PushRequest pushRequest, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // 模擬處理時間
        
        return new PushResult
        {
            Success = true,
            CommitCount = pushRequest.Commits.Count,
            BranchName = pushRequest.BranchName,
            LatestCommitSha = "abc123456789",
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 處理 Pull 操作
    /// </summary>
    public async Task<PullResult> ProcessPullAsync(PullRequest pullRequest, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // 模擬處理時間
        
        return new PullResult
        {
            Success = true,
            CommitCount = 3,
            BranchName = pullRequest.BranchName,
            LatestCommitSha = "def987654321",
            HasConflicts = false,
            ConflictFiles = new List<string>(),
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 取得本地倉庫狀態
    /// </summary>
    public async Task<RepositoryStatus> GetRepositoryStatusAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // 模擬處理時間
        
        return new RepositoryStatus
        {
            CurrentBranch = "main",
            HasUncommittedChanges = true,
            UntrackedFilesCount = 2,
            ModifiedFilesCount = 3,
            StagedFilesCount = 1,
            HasConflicts = false,
            ConflictFiles = new List<string>()
        };
    }

    /// <summary>
    /// 取得本地分支清單
    /// </summary>
    public async Task<IEnumerable<BranchInfo>> GetLocalBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // 模擬處理時間
        
        var branches = new List<BranchInfo>
        {
            new BranchInfo
            {
                Name = "main",
                CommitSha = "abc123456789",
                CommitMessage = "Initial commit",
                CommitAuthor = "Test User",
                CommitDate = DateTime.UtcNow.AddDays(-1),
                IsDefault = true,
                IsProtected = true
            },
            new BranchInfo
            {
                Name = "develop",
                CommitSha = "def987654321",
                CommitMessage = "Add new feature",
                CommitAuthor = "Test Developer",
                CommitDate = DateTime.UtcNow.AddHours(-2),
                IsDefault = false,
                IsProtected = false
            },
            new BranchInfo
            {
                Name = "feature/test",
                CommitSha = "ghi456789012",
                CommitMessage = "Work in progress",
                CommitAuthor = "Test Developer",
                CommitDate = DateTime.UtcNow.AddMinutes(-30),
                IsDefault = false,
                IsProtected = false
            }
        };

        return branches;
    }

    /// <summary>
    /// 切換分支
    /// </summary>
    public async Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // 模擬處理時間
        
        // 模擬成功的分支切換，除非是無效的分支名稱
        return !string.IsNullOrEmpty(branchName) && branchName != "invalid-branch";
    }

    /// <summary>
    /// 解決衝突
    /// </summary>
    public async Task<bool> ResolveConflictsAsync(string repositoryPath, IEnumerable<string> conflictFiles, string strategy, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken); // 模擬處理時間
        
        // 模擬成功的衝突解決，除非策略無效
        var validStrategies = new[] { "ours", "theirs", "manual", "recursive" };
        return validStrategies.Contains(strategy) && conflictFiles.Any();
    }
}
