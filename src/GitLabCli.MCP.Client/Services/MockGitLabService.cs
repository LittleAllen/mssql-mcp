using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.MCP.Client.Services;

/// <summary>
/// GitLab 服務模擬實作 (開發測試用)
/// </summary>
public class MockGitLabService : IGitLabService
{
    public Task<ProjectInfo> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = new ProjectInfo
        {
            Id = projectId,
            Name = $"測試專案 {projectId}",
            Description = "這是一個測試專案",
            DefaultBranch = "main",
            IsPrivate = true,
            CreatedAt = DateTime.Now.AddDays(-30),
            LastActivityAt = DateTime.Now.AddHours(-2),
            HttpUrlToRepo = $"https://gitlab.example.com/group/test-project-{projectId}.git",
            SshUrlToRepo = $"git@gitlab.example.com:group/test-project-{projectId}.git"
        };

        return Task.FromResult(project);
    }

    public Task<IEnumerable<ProjectInfo>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var projects = new[]
        {
            new ProjectInfo
            {
                Id = 1,
                Name = "網站前端",
                Description = "公司官網前端專案",
                DefaultBranch = "main",
                IsPrivate = true,
                CreatedAt = DateTime.Now.AddMonths(-6),
                LastActivityAt = DateTime.Now.AddHours(-1),
                HttpUrlToRepo = "https://gitlab.example.com/web/frontend.git",
                SshUrlToRepo = "git@gitlab.example.com:web/frontend.git"
            },
            new ProjectInfo
            {
                Id = 2,
                Name = "API 後端",
                Description = "REST API 後端服務",
                DefaultBranch = "develop",
                IsPrivate = false,
                CreatedAt = DateTime.Now.AddMonths(-4),
                LastActivityAt = DateTime.Now.AddDays(-1),
                HttpUrlToRepo = "https://gitlab.example.com/api/backend.git",
                SshUrlToRepo = "git@gitlab.example.com:api/backend.git"
            },
            new ProjectInfo
            {
                Id = 3,
                Name = "行動應用",
                Description = "iOS 和 Android 應用程式",
                DefaultBranch = "master",
                IsPrivate = false,
                CreatedAt = DateTime.Now.AddMonths(-2),
                LastActivityAt = DateTime.Now.AddHours(-3),
                HttpUrlToRepo = "https://gitlab.example.com/mobile/app.git",
                SshUrlToRepo = "git@gitlab.example.com:mobile/app.git"
            }
        };

        return Task.FromResult<IEnumerable<ProjectInfo>>(projects);
    }

    public Task<IEnumerable<BranchInfo>> GetBranchesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var branches = new[]
        {
            new BranchInfo
            {
                Name = "main",
                IsDefault = true,
                IsProtected = true,
                CommitSha = "abc123def456",
                CommitMessage = "修復登入功能的錯誤",
                CommitAuthor = "張小明",
                CommitDate = DateTime.Now.AddHours(-2)
            },
            new BranchInfo
            {
                Name = "develop",
                IsDefault = false,
                IsProtected = true,
                CommitSha = "def456ghi789",
                CommitMessage = "新增使用者管理功能",
                CommitAuthor = "李小華",
                CommitDate = DateTime.Now.AddHours(-5)
            },
            new BranchInfo
            {
                Name = "feature/user-profile",
                IsDefault = false,
                IsProtected = false,
                CommitSha = "ghi789jkl012",
                CommitMessage = "實作使用者個人資料頁面",
                CommitAuthor = "王小美",
                CommitDate = DateTime.Now.AddHours(-8)
            }
        };

        return Task.FromResult<IEnumerable<BranchInfo>>(branches);
    }

    public Task<BranchInfo> GetBranchAsync(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        var branch = new BranchInfo
        {
            Name = branchName,
            IsDefault = branchName == "main",
            IsProtected = branchName is "main" or "develop",
            CommitSha = "abc123def456ghi789",
            CommitMessage = $"在分支 {branchName} 的最新提交",
            CommitAuthor = "系統管理員",
            CommitDate = DateTime.Now.AddHours(-1)
        };

        return Task.FromResult(branch);
    }

    public Task<BranchInfo> CreateBranchAsync(int projectId, string branchName, string sourceBranch, CancellationToken cancellationToken = default)
    {
        var branch = new BranchInfo
        {
            Name = branchName,
            IsDefault = false,
            IsProtected = false,
            CommitSha = "new123branch456",
            CommitMessage = $"從 {sourceBranch} 建立新分支",
            CommitAuthor = "開發者",
            CommitDate = DateTime.Now
        };

        return Task.FromResult(branch);
    }

    public Task<IEnumerable<CommitInfo>> GetCommitsAsync(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        var commits = new[]
        {
            new CommitInfo
            {
                Sha = "commit123abc",
                Message = "修復重要錯誤",
                AuthorName = "張開發",
                AuthorEmail = "dev1@example.com",
                AuthorDate = DateTime.Now.AddHours(-1)
            },
            new CommitInfo
            {
                Sha = "commit456def",
                Message = "新增新功能",
                AuthorName = "李程式",
                AuthorEmail = "dev2@example.com",
                AuthorDate = DateTime.Now.AddHours(-3)
            },
            new CommitInfo
            {
                Sha = "commit789ghi",
                Message = "更新文件",
                AuthorName = "王文件",
                AuthorEmail = "doc@example.com",
                AuthorDate = DateTime.Now.AddHours(-5)
            }
        };

        return Task.FromResult<IEnumerable<CommitInfo>>(commits);
    }
}
