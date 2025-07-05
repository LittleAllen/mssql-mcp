using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.MCP.Client.Services;

/// <summary>
/// Git 服務模擬實作 (開發測試用)
/// </summary>
public class MockGitService : IGitService
{
    public async Task<PushResult> ProcessPushAsync(PushRequest pushRequest, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(1000, cancellationToken);

        var result = new PushResult
        {
            Success = true,
            BranchName = pushRequest.BranchName,
            CommitCount = Random.Shared.Next(1, 5),
            LatestCommitSha = "abc123def456",
            ProcessedAt = DateTime.UtcNow
        };

        return result;
    }

    public async Task<PullResult> ProcessPullAsync(PullRequest pullRequest, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(1500, cancellationToken);

        var hasConflicts = Random.Shared.Next(0, 10) < 2; // 20% 機率有衝突

        var result = new PullResult
        {
            Success = true,
            BranchName = pullRequest.BranchName,
            CommitCount = Random.Shared.Next(0, 3),
            LatestCommitSha = "def456ghi789",
            HasConflicts = hasConflicts,
            ConflictFiles = hasConflicts ? new List<string> { "src/Program.cs", "README.md" } : new List<string>(),
            ProcessedAt = DateTime.UtcNow
        };

        return result;
    }

    public Task<RepositoryStatus> GetRepositoryStatusAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var status = new RepositoryStatus
        {
            CurrentBranch = "main",
            HasUncommittedChanges = Random.Shared.Next(0, 10) < 3, // 30% 機率有未提交變更
            UntrackedFilesCount = Random.Shared.Next(0, 5),
            ModifiedFilesCount = Random.Shared.Next(0, 3),
            StagedFilesCount = Random.Shared.Next(0, 2),
            HasConflicts = false,
            ConflictFiles = new List<string>()
        };

        return Task.FromResult(status);
    }

    public Task<IEnumerable<BranchInfo>> GetLocalBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var branches = new[]
        {
            new BranchInfo { Name = "main", IsDefault = true },
            new BranchInfo { Name = "develop", IsDefault = false },
            new BranchInfo { Name = "feature/new-feature", IsDefault = false },
            new BranchInfo { Name = "hotfix/critical-fix", IsDefault = false }
        };

        return Task.FromResult<IEnumerable<BranchInfo>>(branches);
    }

    public async Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(500, cancellationToken);

        // 模擬成功率 90%
        var success = Random.Shared.Next(0, 10) < 9;
        return success;
    }

    public async Task<bool> ResolveConflictsAsync(string repositoryPath, IEnumerable<string> conflictFiles, string strategy, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(2000, cancellationToken);

        // 模擬成功率 80%
        var success = Random.Shared.Next(0, 10) < 8;
        return success;
    }
}

/// <summary>
/// MCP 客戶端模擬實作 (開發測試用)
/// </summary>
public class MockMcpClient : IMcpClient
{
    public async Task<ApiResponse<TResponse>> CallApiAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(100, cancellationToken);
        
        var response = new ApiResponse<TResponse>
        {
            Success = true,
            Data = default(TResponse),
            Message = "模擬成功回應",
            Timestamp = DateTime.UtcNow
        };
        
        return response;
    }

    public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(100, cancellationToken);
        
        var response = new ApiResponse<TResponse>
        {
            Success = true,
            Data = default(TResponse),
            Message = "GET 請求成功",
            Timestamp = DateTime.UtcNow
        };
        
        return response;
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(150, cancellationToken);
        
        var response = new ApiResponse<TResponse>
        {
            Success = true,
            Data = default(TResponse),
            Message = "POST 請求成功",
            Timestamp = DateTime.UtcNow
        };
        
        return response;
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(120, cancellationToken);
        
        var response = new ApiResponse<TResponse>
        {
            Success = true,
            Data = default(TResponse),
            Message = "PUT 請求成功",
            Timestamp = DateTime.UtcNow
        };
        
        return response;
    }

    public async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        // 模擬處理延遲
        await Task.Delay(80, cancellationToken);
        
        var response = new ApiResponse<TResponse>
        {
            Success = true,
            Data = default(TResponse),
            Message = "DELETE 請求成功",
            Timestamp = DateTime.UtcNow
        };
        
        return response;
    }
}
