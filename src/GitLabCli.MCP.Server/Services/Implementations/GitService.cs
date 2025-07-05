using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Exceptions;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using RepositoryStatus = GitLabCli.Shared.Interfaces.RepositoryStatus;
using PushResult = GitLabCli.Shared.Models.Responses.PushResult;

namespace GitLabCli.MCP.Server.Services.Implementations;

/// <summary>
/// Git 操作服務實作
/// </summary>
public class GitService : IGitService
{
    private readonly GitOptions _gitOptions;
    private readonly ILogger<GitService> _logger;

    public GitService(IOptions<GitOptions> gitOptions, ILogger<GitService> logger)
    {
        _gitOptions = gitOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// 處理 Push 操作
    /// </summary>
    /// <param name="pushRequest">Push 請求參數</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Push 操作結果</returns>
    public async Task<PushResult> ProcessPushAsync(PushRequest pushRequest, CancellationToken cancellationToken = default)
    {
        if (pushRequest == null)
            throw new ArgumentNullException(nameof(pushRequest));

        try
        {
            _logger.LogInformation("開始處理 Push 操作，專案 ID: {ProjectId}，分支: {BranchName}，Commit 數量: {CommitCount}", 
                pushRequest.ProjectId, pushRequest.BranchName, pushRequest.Commits.Count);

            // TODO: 實作實際的 Git push 邏輯
            // 這裡需要整合 LibGit2Sharp 來執行實際的 Git 操作
            
            await Task.Delay(100, cancellationToken); // 模擬異步操作

            var result = new PushResult
            {
                Success = true,
                CommitCount = pushRequest.Commits.Count,
                BranchName = pushRequest.BranchName,
                LatestCommitSha = pushRequest.Commits.LastOrDefault()?.Sha ?? string.Empty,
                ProcessedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Push 操作完成，專案 ID: {ProjectId}，分支: {BranchName}，處理 {CommitCount} 個 Commit", 
                pushRequest.ProjectId, pushRequest.BranchName, pushRequest.Commits.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push 操作失敗，專案 ID: {ProjectId}，分支: {BranchName}", 
                pushRequest.ProjectId, pushRequest.BranchName);

            return new PushResult
            {
                Success = false,
                BranchName = pushRequest.BranchName,
                ErrorMessage = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 處理 Pull 操作
    /// </summary>
    /// <param name="pullRequest">Pull 請求參數</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Pull 操作結果</returns>
    public async Task<PullResult> ProcessPullAsync(PullRequest pullRequest, CancellationToken cancellationToken = default)
    {
        if (pullRequest == null)
            throw new ArgumentNullException(nameof(pullRequest));

        try
        {
            _logger.LogInformation("開始處理 Pull 操作，專案 ID: {ProjectId}，分支: {BranchName}，本地路徑: {LocalPath}", 
                pullRequest.ProjectId, pullRequest.BranchName, pullRequest.LocalPath);

            // TODO: 實作實際的 Git pull 邏輯
            // 這裡需要整合 LibGit2Sharp 來執行實際的 Git 操作
            
            await Task.Delay(100, cancellationToken); // 模擬異步操作

            var result = new PullResult
            {
                Success = true,
                CommitCount = 0, // TODO: 計算實際拉取的 Commit 數量
                BranchName = pullRequest.BranchName,
                LatestCommitSha = string.Empty, // TODO: 取得最新 Commit SHA
                HasConflicts = false,
                ConflictFiles = new List<string>(),
                ProcessedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Pull 操作完成，專案 ID: {ProjectId}，分支: {BranchName}", 
                pullRequest.ProjectId, pullRequest.BranchName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pull 操作失敗，專案 ID: {ProjectId}，分支: {BranchName}", 
                pullRequest.ProjectId, pullRequest.BranchName);

            return new PullResult
            {
                Success = false,
                BranchName = pullRequest.BranchName,
                ErrorMessage = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 取得本地倉庫狀態
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>倉庫狀態</returns>
    public async Task<RepositoryStatus> GetRepositoryStatusAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(repositoryPath))
            throw new ArgumentException("倉庫路徑不能為空", nameof(repositoryPath));

        try
        {
            _logger.LogInformation("正在取得倉庫狀態，路徑: {RepositoryPath}", repositoryPath);

            await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("GetStatus", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }
            }, cancellationToken);

            using var repo = new Repository(repositoryPath);
            var libGitStatus = repo.RetrieveStatus();

            var repositoryStatus = new GitLabCli.Shared.Interfaces.RepositoryStatus
            {
                CurrentBranch = repo.Head.FriendlyName,
                HasUncommittedChanges = libGitStatus.IsDirty,
                UntrackedFilesCount = libGitStatus.Untracked.Count(),
                ModifiedFilesCount = libGitStatus.Modified.Count(),
                StagedFilesCount = libGitStatus.Staged.Count(),
                HasConflicts = libGitStatus.Where(entry => entry.State.HasFlag(FileStatus.Conflicted)).Any(),
                ConflictFiles = libGitStatus.Where(entry => entry.State.HasFlag(FileStatus.Conflicted))
                    .Select(f => f.FilePath).ToList()
            };

            _logger.LogInformation("倉庫狀態取得完成，路徑: {RepositoryPath}，目前分支: {CurrentBranch}", 
                repositoryPath, repositoryStatus.CurrentBranch);

            return repositoryStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得倉庫狀態失敗，路徑: {RepositoryPath}", repositoryPath);
            throw new GitOperationException("GetStatus", $"取得倉庫狀態失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 取得本地分支清單
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    public async Task<IEnumerable<BranchInfo>> GetLocalBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(repositoryPath))
            throw new ArgumentException("倉庫路徑不能為空", nameof(repositoryPath));

        try
        {
            _logger.LogInformation("正在取得本地分支清單，路徑: {RepositoryPath}", repositoryPath);

            return await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("GetBranches", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }

                using var repo = new Repository(repositoryPath);
                return repo.Branches.Where(b => !b.IsRemote).Select(b => new BranchInfo
                {
                    Name = b.FriendlyName,
                    CommitSha = b.Tip?.Sha ?? string.Empty,
                    CommitMessage = b.Tip?.MessageShort ?? string.Empty,
                    CommitAuthor = b.Tip?.Author?.Name ?? string.Empty,
                    CommitDate = b.Tip?.Author?.When.DateTime ?? DateTime.MinValue,
                    IsDefault = b.IsCurrentRepositoryHead,
                    IsProtected = false // 本地分支沒有保護概念
                }).ToList();
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得本地分支清單失敗，路徑: {RepositoryPath}", repositoryPath);
            throw new GitOperationException("GetBranches", $"取得本地分支清單失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 切換分支
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>是否成功</returns>
    public async Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(repositoryPath))
            throw new ArgumentException("倉庫路徑不能為空", nameof(repositoryPath));
        
        if (string.IsNullOrEmpty(branchName))
            throw new ArgumentException("分支名稱不能為空", nameof(branchName));

        try
        {
            _logger.LogInformation("正在切換分支，路徑: {RepositoryPath}，分支: {BranchName}", repositoryPath, branchName);

            return await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("Checkout", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }

                using var repo = new Repository(repositoryPath);
                var branch = repo.Branches[branchName];
                
                if (branch == null)
                {
                    throw new GitOperationException("Checkout", $"分支不存在: {branchName}");
                }

                Commands.Checkout(repo, branch);
                return true;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換分支失敗，路徑: {RepositoryPath}，分支: {BranchName}", repositoryPath, branchName);
            throw new GitOperationException("Checkout", $"切換分支失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解決衝突
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="conflictFiles">衝突檔案</param>
    /// <param name="strategy">解決策略</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>解決結果</returns>
    public async Task<bool> ResolveConflictsAsync(string repositoryPath, IEnumerable<string> conflictFiles, string strategy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(repositoryPath))
            throw new ArgumentException("倉庫路徑不能為空", nameof(repositoryPath));
        
        if (conflictFiles == null)
            throw new ArgumentNullException(nameof(conflictFiles));

        try
        {
            _logger.LogInformation("正在解決衝突，路徑: {RepositoryPath}，策略: {Strategy}，檔案數量: {FileCount}", 
                repositoryPath, strategy, conflictFiles.Count());

            // TODO: 實作衝突解決邏輯
            // 根據不同的策略（manual, theirs, ours）來解決衝突
            
            await Task.Delay(100, cancellationToken); // 模擬異步操作

            _logger.LogInformation("衝突解決完成，路徑: {RepositoryPath}", repositoryPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解決衝突失敗，路徑: {RepositoryPath}", repositoryPath);
            throw new GitOperationException("ResolveConflicts", $"解決衝突失敗: {ex.Message}", ex);
        }
    }
}
