using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Exceptions;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLabCli.MCP.Server.Services.Implementations;

/// <summary>
/// GitLab 服務實作
/// </summary>
public class GitLabService : IGitLabService
{
    private readonly GitLabOptions _options;
    private readonly ILogger<GitLabService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public GitLabService(IOptions<GitLabOptions> options, ILogger<GitLabService> logger, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 建立配置好的 HTTP 客戶端
    /// </summary>
    /// <returns>配置好的 HttpClient</returns>
    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{_options.BaseUrl.TrimEnd('/')}/api/v4/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.Timeout = _options.Timeout;
        
        return client;
    }

    /// <summary>
    /// 處理 HTTP 回應並檢查錯誤
    /// </summary>
    /// <param name="response">HTTP 回應</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應內容</returns>
    private async Task<string> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("GitLab API 呼叫失敗：{StatusCode} - {Content}", response.StatusCode, content);
            throw new GitLabApiException((int)response.StatusCode, response.StatusCode.ToString(), content);
        }
        
        return content;
    }

    /// <summary>
    /// 取得專案資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案資訊</returns>
    public async Task<ProjectInfo> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在取得專案資訊，專案 ID: {ProjectId}", projectId);
        
        using var client = CreateHttpClient();
        
        try
        {
            var response = await client.GetAsync($"projects/{projectId}", cancellationToken).ConfigureAwait(false);
            var content = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabProject = JsonSerializer.Deserialize<GitLabProjectDto>(content, _jsonOptions);
            
            if (gitlabProject == null)
            {
                throw new GitLabApiException(404, "NOT_FOUND", $"專案 {projectId} 不存在");
            }
            
            return new ProjectInfo
            {
                Id = gitlabProject.Id,
                Name = gitlabProject.Name,
                Description = gitlabProject.Description ?? string.Empty,
                HttpUrlToRepo = gitlabProject.HttpUrlToRepo,
                SshUrlToRepo = gitlabProject.SshUrlToRepo,
                DefaultBranch = gitlabProject.DefaultBranch,
                IsPrivate = gitlabProject.Visibility == "private",
                CreatedAt = gitlabProject.CreatedAt,
                LastActivityAt = gitlabProject.LastActivityAt
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "取得專案資訊時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "取得專案資訊時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }

    /// <summary>
    /// 取得專案清單
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案清單</returns>
    public async Task<IEnumerable<ProjectInfo>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在取得專案清單");
        
        using var client = CreateHttpClient();
        
        try
        {
            var response = await client.GetAsync("projects?membership=true&simple=true&per_page=100", cancellationToken).ConfigureAwait(false);
            var content = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabProjects = JsonSerializer.Deserialize<GitLabProjectDto[]>(content, _jsonOptions);
            
            if (gitlabProjects == null)
            {
                return new List<ProjectInfo>();
            }
            
            return gitlabProjects.Select(p => new ProjectInfo
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                HttpUrlToRepo = p.HttpUrlToRepo,
                SshUrlToRepo = p.SshUrlToRepo,
                DefaultBranch = p.DefaultBranch,
                IsPrivate = p.Visibility == "private",
                CreatedAt = p.CreatedAt,
                LastActivityAt = p.LastActivityAt
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "取得專案清單時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "取得專案清單時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }

    /// <summary>
    /// 取得分支清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    public async Task<IEnumerable<BranchInfo>> GetBranchesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在取得分支清單，專案 ID: {ProjectId}", projectId);
        
        using var client = CreateHttpClient();
        
        try
        {
            var response = await client.GetAsync($"projects/{projectId}/repository/branches", cancellationToken).ConfigureAwait(false);
            var content = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabBranches = JsonSerializer.Deserialize<GitLabBranchDto[]>(content, _jsonOptions);
            
            if (gitlabBranches == null)
            {
                return new List<BranchInfo>();
            }
            
            return gitlabBranches.Select(b => new BranchInfo
            {
                Name = b.Name,
                CommitSha = b.Commit.Id,
                CommitMessage = b.Commit.Message,
                CommitAuthor = b.Commit.AuthorName,
                CommitDate = b.Commit.AuthoredDate,
                IsDefault = b.Default,
                IsProtected = b.Protected
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "取得分支清單時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "取得分支清單時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }

    /// <summary>
    /// 取得分支資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支資訊</returns>
    public async Task<BranchInfo> GetBranchAsync(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在取得分支資訊，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
        
        using var client = CreateHttpClient();
        
        try
        {
            var encodedBranchName = Uri.EscapeDataString(branchName);
            var response = await client.GetAsync($"projects/{projectId}/repository/branches/{encodedBranchName}", cancellationToken).ConfigureAwait(false);
            var content = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabBranch = JsonSerializer.Deserialize<GitLabBranchDto>(content, _jsonOptions);
            
            if (gitlabBranch == null)
            {
                throw new GitLabApiException(404, "NOT_FOUND", $"分支 {branchName} 不存在");
            }
            
            return new BranchInfo
            {
                Name = gitlabBranch.Name,
                CommitSha = gitlabBranch.Commit.Id,
                CommitMessage = gitlabBranch.Commit.Message,
                CommitAuthor = gitlabBranch.Commit.AuthorName,
                CommitDate = gitlabBranch.Commit.AuthoredDate,
                IsDefault = gitlabBranch.Default,
                IsProtected = gitlabBranch.Protected
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "取得分支資訊時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "取得分支資訊時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }

    /// <summary>
    /// 建立分支
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="sourceBranch">來源分支</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>建立的分支資訊</returns>
    public async Task<BranchInfo> CreateBranchAsync(int projectId, string branchName, string sourceBranch, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在建立分支，專案 ID: {ProjectId}，分支: {BranchName}，來源分支: {SourceBranch}", 
            projectId, branchName, sourceBranch);
        
        using var client = CreateHttpClient();
        
        try
        {
            var createRequest = new
            {
                branch = branchName,
                @ref = sourceBranch
            };
            
            var json = JsonSerializer.Serialize(createRequest, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync($"projects/{projectId}/repository/branches", content, cancellationToken).ConfigureAwait(false);
            var responseContent = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabBranch = JsonSerializer.Deserialize<GitLabBranchDto>(responseContent, _jsonOptions);
            
            if (gitlabBranch == null)
            {
                throw new GitLabApiException(500, "INTERNAL_ERROR", "建立分支失敗");
            }
            
            return new BranchInfo
            {
                Name = gitlabBranch.Name,
                CommitSha = gitlabBranch.Commit.Id,
                CommitMessage = gitlabBranch.Commit.Message,
                CommitAuthor = gitlabBranch.Commit.AuthorName,
                CommitDate = gitlabBranch.Commit.AuthoredDate,
                IsDefault = gitlabBranch.Default,
                IsProtected = gitlabBranch.Protected
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "建立分支時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "建立分支時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }

    /// <summary>
    /// 取得 Commit 清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Commit 清單</returns>
    public async Task<IEnumerable<CommitInfo>> GetCommitsAsync(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("正在取得 Commit 清單，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
        
        using var client = CreateHttpClient();
        
        try
        {
            var encodedBranchName = Uri.EscapeDataString(branchName);
            var response = await client.GetAsync($"projects/{projectId}/repository/commits?ref_name={encodedBranchName}&per_page=50", cancellationToken).ConfigureAwait(false);
            var content = await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            
            var gitlabCommits = JsonSerializer.Deserialize<GitLabCommitDto[]>(content, _jsonOptions);
            
            if (gitlabCommits == null)
            {
                return new List<CommitInfo>();
            }
            
            var commitInfos = new List<CommitInfo>();
            
            foreach (var commit in gitlabCommits)
            {
                // 取得每個 commit 的詳細資訊（包含檔案變更）
                var commitDetailResponse = await client.GetAsync($"projects/{projectId}/repository/commits/{commit.Id}", cancellationToken).ConfigureAwait(false);
                var commitDetailContent = await HandleResponseAsync(commitDetailResponse, cancellationToken).ConfigureAwait(false);
                
                var commitDetail = JsonSerializer.Deserialize<GitLabCommitDetailDto>(commitDetailContent, _jsonOptions);
                
                var changedFiles = new List<FileChange>();
                if (commitDetail?.Stats != null)
                {
                    // GitLab API 提供的是整體統計，需要進一步取得檔案差異詳情
                    var diffResponse = await client.GetAsync($"projects/{projectId}/repository/commits/{commit.Id}/diff", cancellationToken).ConfigureAwait(false);
                    var diffContent = await HandleResponseAsync(diffResponse, cancellationToken).ConfigureAwait(false);
                    
                    var diffs = JsonSerializer.Deserialize<GitLabDiffDto[]>(diffContent, _jsonOptions);
                    
                    if (diffs != null)
                    {
                        changedFiles = diffs.Select(d => new FileChange
                        {
                            FilePath = d.NewPath ?? d.OldPath,
                            OldFilePath = d.RenamedFile ? d.OldPath : null,
                            ChangeType = DetermineChangeType(d),
                            AddedLines = ParseLineCount(d.Diff, "+"),
                            DeletedLines = ParseLineCount(d.Diff, "-")
                        }).ToList();
                    }
                }
                
                commitInfos.Add(new CommitInfo
                {
                    Sha = commit.Id,
                    Message = commit.Message,
                    AuthorName = commit.AuthorName,
                    AuthorEmail = commit.AuthorEmail,
                    AuthorDate = commit.AuthoredDate,
                    CommitterName = commit.CommitterName,
                    CommitterEmail = commit.CommitterEmail,
                    CommitterDate = commit.CommittedDate,
                    ChangedFiles = changedFiles
                });
            }
            
            return commitInfos;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "取得 Commit 清單時發生網路錯誤");
            throw new GitLabApiException(500, "NETWORK_ERROR", "網路連線錯誤");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "取得 Commit 清單時請求逾時");
            throw new GitLabApiException(408, "TIMEOUT", "請求逾時");
        }
    }
    
    /// <summary>
    /// 判斷檔案變更類型
    /// </summary>
    /// <param name="diff">差異資訊</param>
    /// <returns>變更類型</returns>
    private static string DetermineChangeType(GitLabDiffDto diff)
    {
        if (diff.NewFile) return "Added";
        if (diff.DeletedFile) return "Deleted";
        if (diff.RenamedFile) return "Renamed";
        return "Modified";
    }
    
    /// <summary>
    /// 解析差異中的行數變更
    /// </summary>
    /// <param name="diffContent">差異內容</param>
    /// <param name="prefix">前綴符號（+ 或 -）</param>
    /// <returns>行數</returns>
    private static int ParseLineCount(string diffContent, string prefix)
    {
        if (string.IsNullOrEmpty(diffContent)) return 0;
        
        return diffContent.Split('\n')
            .Count(line => line.StartsWith(prefix) && line.Length > 1);
    }
}

// GitLab API DTO 類別
internal class GitLabProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string HttpUrlToRepo { get; set; } = string.Empty;
    public string SshUrlToRepo { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}

internal class GitLabBranchDto
{
    public string Name { get; set; } = string.Empty;
    public GitLabCommitDto Commit { get; set; } = new();
    public bool Default { get; set; }
    public bool Protected { get; set; }
}

internal class GitLabCommitDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime AuthoredDate { get; set; }
    public string CommitterName { get; set; } = string.Empty;
    public string CommitterEmail { get; set; } = string.Empty;
    public DateTime CommittedDate { get; set; }
}

internal class GitLabCommitDetailDto : GitLabCommitDto
{
    public GitLabStatsDto? Stats { get; set; }
}

internal class GitLabStatsDto
{
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int Total { get; set; }
}

internal class GitLabDiffDto
{
    public string? OldPath { get; set; }
    public string? NewPath { get; set; }
    public bool NewFile { get; set; }
    public bool DeletedFile { get; set; }
    public bool RenamedFile { get; set; }
    public string Diff { get; set; } = string.Empty;
}
