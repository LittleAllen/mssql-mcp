using GitLabCli.Shared.Models.Entities;

namespace GitLabCli.Shared.Tests.TestHelpers;

/// <summary>
/// 專案資訊建構器
/// </summary>
public class ProjectInfoBuilder
{
    private readonly ProjectInfo _projectInfo = new()
    {
        Id = 123,
        Name = "測試專案",
        Description = "這是一個測試專案",
        DefaultBranch = "main",
        IsPrivate = false,
        CreatedAt = DateTime.UtcNow.AddMonths(-1),
        LastActivityAt = DateTime.UtcNow.AddHours(-1),
        HttpUrlToRepo = "https://gitlab.example.com/test/project.git",
        SshUrlToRepo = "git@gitlab.example.com:test/project.git"
    };

    public ProjectInfoBuilder WithId(int id)
    {
        _projectInfo.Id = id;
        return this;
    }

    public ProjectInfoBuilder WithName(string name)
    {
        _projectInfo.Name = name;
        return this;
    }

    public ProjectInfoBuilder WithDescription(string description)
    {
        _projectInfo.Description = description;
        return this;
    }

    public ProjectInfoBuilder WithDefaultBranch(string branch)
    {
        _projectInfo.DefaultBranch = branch;
        return this;
    }

    public ProjectInfoBuilder AsPrivate()
    {
        _projectInfo.IsPrivate = true;
        return this;
    }

    public ProjectInfoBuilder AsPublic()
    {
        _projectInfo.IsPrivate = false;
        return this;
    }

    public ProjectInfoBuilder WithCreatedAt(DateTime createdAt)
    {
        _projectInfo.CreatedAt = createdAt;
        return this;
    }

    public ProjectInfoBuilder WithLastActivityAt(DateTime lastActivityAt)
    {
        _projectInfo.LastActivityAt = lastActivityAt;
        return this;
    }

    public ProjectInfo Build() => _projectInfo;
}

/// <summary>
/// 分支資訊建構器
/// </summary>
public class BranchInfoBuilder
{
    private readonly BranchInfo _branchInfo = new()
    {
        Name = "main",
        IsDefault = true,
        IsProtected = true,
        CommitSha = "abc123def456",
        CommitMessage = "測試提交",
        CommitAuthor = "測試者",
        CommitDate = DateTime.UtcNow.AddHours(-1)
    };

    public BranchInfoBuilder WithName(string name)
    {
        _branchInfo.Name = name;
        return this;
    }

    public BranchInfoBuilder AsDefault()
    {
        _branchInfo.IsDefault = true;
        return this;
    }

    public BranchInfoBuilder AsNonDefault()
    {
        _branchInfo.IsDefault = false;
        return this;
    }

    public BranchInfoBuilder AsProtected()
    {
        _branchInfo.IsProtected = true;
        return this;
    }

    public BranchInfoBuilder AsUnprotected()
    {
        _branchInfo.IsProtected = false;
        return this;
    }

    public BranchInfoBuilder WithCommit(string sha, string message, string author)
    {
        _branchInfo.CommitSha = sha;
        _branchInfo.CommitMessage = message;
        _branchInfo.CommitAuthor = author;
        return this;
    }

    public BranchInfo Build() => _branchInfo;
}

/// <summary>
/// 提交資訊建構器
/// </summary>
public class CommitInfoBuilder
{
    private readonly CommitInfo _commitInfo = new()
    {
        Sha = "abc123def456",
        Message = "測試提交",
        AuthorName = "測試者",
        AuthorEmail = "test@example.com",
        AuthorDate = DateTime.UtcNow.AddHours(-1)
    };

    public CommitInfoBuilder WithSha(string sha)
    {
        _commitInfo.Sha = sha;
        return this;
    }

    public CommitInfoBuilder WithMessage(string message)
    {
        _commitInfo.Message = message;
        return this;
    }

    public CommitInfoBuilder WithAuthor(string name, string email)
    {
        _commitInfo.AuthorName = name;
        _commitInfo.AuthorEmail = email;
        return this;
    }

    public CommitInfoBuilder WithDate(DateTime date)
    {
        _commitInfo.AuthorDate = date;
        return this;
    }

    public CommitInfo Build() => _commitInfo;
}
