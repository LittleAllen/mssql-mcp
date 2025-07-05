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

            // 實作實際的 Git push 邏輯
            var repositoryPath = _gitOptions.RepositoryPath;
            if (string.IsNullOrEmpty(repositoryPath))
            {
                throw new GitOperationException("Push", "Git 倉庫路徑未設定");
            }

            return await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("Push", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }

                using var repo = new Repository(repositoryPath);
                
                // 檢查目前分支是否為目標分支
                if (repo.Head.FriendlyName != pushRequest.BranchName)
                {
                    var targetBranch = repo.Branches[pushRequest.BranchName];
                    if (targetBranch == null)
                    {
                        // 建立新分支
                        targetBranch = repo.CreateBranch(pushRequest.BranchName);
                    }
                    Commands.Checkout(repo, targetBranch);
                }

                // 取得 remote
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    throw new GitOperationException("Push", "找不到 origin remote");
                }

                // 執行 push 操作
                var options = new PushOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) => 
                        new DefaultCredentials()
                };

                try
                {
                    var currentBranch = repo.Head;
                    repo.Network.Push(currentBranch, options);
                    
                    var result = new PushResult
                    {
                        Success = true,
                        CommitCount = pushRequest.Commits.Count,
                        BranchName = pushRequest.BranchName,
                        LatestCommitSha = currentBranch.Tip?.Sha ?? string.Empty,
                        ProcessedAt = DateTime.UtcNow
                    };

                    _logger.LogInformation("Push 操作完成，專案 ID: {ProjectId}，分支: {BranchName}，處理 {CommitCount} 個 Commit", 
                        pushRequest.ProjectId, pushRequest.BranchName, pushRequest.Commits.Count);

                    return result;
                }
                catch (LibGit2SharpException ex)
                {
                    throw new GitOperationException("Push", $"Push 操作失敗: {ex.Message}", ex);
                }
            }, cancellationToken);
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

            // 實作實際的 Git pull 邏輯
            var repositoryPath = string.IsNullOrEmpty(pullRequest.LocalPath) 
                ? _gitOptions.RepositoryPath 
                : pullRequest.LocalPath;
                
            if (string.IsNullOrEmpty(repositoryPath))
            {
                throw new GitOperationException("Pull", "Git 倉庫路徑未設定");
            }

            return await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("Pull", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }

                using var repo = new Repository(repositoryPath);
                
                // 檢查目前分支是否為目標分支
                if (repo.Head.FriendlyName != pullRequest.BranchName)
                {
                    var targetBranch = repo.Branches[pullRequest.BranchName];
                    if (targetBranch == null)
                    {
                        // 尋找遠端分支
                        var remoteBranch = repo.Branches[$"origin/{pullRequest.BranchName}"];
                        if (remoteBranch != null)
                        {
                            targetBranch = repo.CreateBranch(pullRequest.BranchName, remoteBranch.Tip);
                            repo.Branches.Update(targetBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                        }
                        else
                        {
                            throw new GitOperationException("Pull", $"分支不存在: {pullRequest.BranchName}");
                        }
                    }
                    Commands.Checkout(repo, targetBranch);
                }

                // 記錄 pull 前的狀態
                var beforePullCommit = repo.Head.Tip;

                // 取得 remote
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    throw new GitOperationException("Pull", "找不到 origin remote");
                }

                // 執行 fetch 操作
                var fetchOptions = new FetchOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) => 
                        new DefaultCredentials()
                };

                try
                {
                    Commands.Fetch(repo, remote.Name, new string[0], fetchOptions, null);

                    // 執行 merge 操作
                    var signature = new Signature("GitLab CLI", "gitlab-cli@example.com", DateTimeOffset.Now);
                    var mergeOptions = new MergeOptions
                    {
                        FileConflictStrategy = pullRequest.ConflictStrategy switch
                        {
                            "ours" => CheckoutFileConflictStrategy.Ours,
                            "theirs" => CheckoutFileConflictStrategy.Theirs,
                            _ => CheckoutFileConflictStrategy.Normal
                        }
                    };

                    var remoteBranch = repo.Branches[$"origin/{pullRequest.BranchName}"];
                    if (remoteBranch == null)
                    {
                        throw new GitOperationException("Pull", $"遠端分支不存在: origin/{pullRequest.BranchName}");
                    }

                    var mergeResult = repo.Merge(remoteBranch, signature, mergeOptions);

                    // 計算新增的 Commit 數量
                    var afterPullCommit = repo.Head.Tip;
                    var newCommits = 0;
                    if (beforePullCommit != null && afterPullCommit != null && beforePullCommit.Sha != afterPullCommit.Sha)
                    {
                        var commits = repo.Commits.QueryBy(new CommitFilter
                        {
                            ExcludeReachableFrom = beforePullCommit,
                            IncludeReachableFrom = afterPullCommit
                        });
                        newCommits = commits.Count();
                    }

                    // 檢查衝突
                    var hasConflicts = mergeResult.Status == MergeStatus.Conflicts;
                    var conflictFiles = new List<string>();
                    
                    if (hasConflicts)
                    {
                        var status = repo.RetrieveStatus();
                        conflictFiles = status.Where(entry => entry.State.HasFlag(FileStatus.Conflicted))
                            .Select(f => f.FilePath).ToList();
                    }

                    var result = new PullResult
                    {
                        Success = !hasConflicts || pullRequest.ConflictStrategy != "manual",
                        CommitCount = newCommits,
                        BranchName = pullRequest.BranchName,
                        LatestCommitSha = afterPullCommit?.Sha ?? string.Empty,
                        HasConflicts = hasConflicts,
                        ConflictFiles = conflictFiles,
                        ProcessedAt = DateTime.UtcNow
                    };

                    _logger.LogInformation("Pull 操作完成，專案 ID: {ProjectId}，分支: {BranchName}", 
                        pullRequest.ProjectId, pullRequest.BranchName);

                    return result;
                }
                catch (LibGit2SharpException ex)
                {
                    throw new GitOperationException("Pull", $"Pull 操作失敗: {ex.Message}", ex);
                }
            }, cancellationToken);
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

            // 實作衝突解決邏輯
            // 根據不同的策略（manual, theirs, ours）來解決衝突
            return await Task.Run(() =>
            {
                if (!Repository.IsValid(repositoryPath))
                {
                    throw new GitOperationException("ResolveConflicts", $"無效的 Git 倉庫路徑: {repositoryPath}");
                }

                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus();
                var actualConflictFiles = status.Where(entry => entry.State.HasFlag(FileStatus.Conflicted)).ToList();
                
                if (!actualConflictFiles.Any())
                {
                    _logger.LogInformation("沒有發現衝突檔案，路徑: {RepositoryPath}", repositoryPath);
                    return true;
                }

                switch (strategy.ToLower())
                {
                    case "ours":
                        // 使用我們的版本
                        foreach (var conflictFile in actualConflictFiles)
                        {
                            repo.Index.Add(conflictFile.FilePath);
                        }
                        break;

                    case "theirs":
                        // 使用他們的版本
                        foreach (var conflictFile in actualConflictFiles)
                        {
                            var indexEntry = repo.Index[conflictFile.FilePath];
                            if (indexEntry != null)
                            {
                                // 從 MERGE_HEAD 取得他們的版本
                                var theirCommit = repo.Lookup<Commit>("MERGE_HEAD");
                                if (theirCommit != null)
                                {
                                    var theirTreeEntry = theirCommit.Tree[conflictFile.FilePath];
                                    if (theirTreeEntry != null)
                                    {
                                        repo.Index.Add(conflictFile.FilePath);
                                    }
                                }
                            }
                        }
                        break;

                    case "manual":
                        // 手動解決，這裡只是標記為已解決
                        // 實際的手動解決需要在外部完成
                        _logger.LogWarning("手動解決策略需要外部介入，檔案: {Files}", 
                            string.Join(", ", actualConflictFiles.Select(f => f.FilePath)));
                        return false;

                    default:
                        throw new GitOperationException("ResolveConflicts", $"不支援的衝突解決策略: {strategy}");
                }

                // 提交解決後的變更
                try
                {
                    var signature = new Signature("GitLab CLI", "gitlab-cli@example.com", DateTimeOffset.Now);
                    repo.Commit($"解決衝突 - 策略: {strategy}", signature, signature);
                    
                    _logger.LogInformation("衝突解決並提交完成，路徑: {RepositoryPath}，策略: {Strategy}", 
                        repositoryPath, strategy);
                    return true;
                }
                catch (LibGit2SharpException ex)
                {
                    throw new GitOperationException("ResolveConflicts", $"提交衝突解決失敗: {ex.Message}", ex);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解決衝突失敗，路徑: {RepositoryPath}", repositoryPath);
            throw new GitOperationException("ResolveConflicts", $"解決衝突失敗: {ex.Message}", ex);
        }
    }
}
