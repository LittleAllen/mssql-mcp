using System.CommandLine;
using GitLabCli.MCP.Client.Commands.Git;
using GitLabCli.MCP.Client.Commands.GitLab;
using GitLabCli.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace GitLabCli.MCP.Client.Commands;

/// <summary>
/// 專案相關命令
/// </summary>
public class ProjectCommand : BaseCommand
{
    public ProjectCommand(
        ILogger<ProjectCommand> logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("project", "專案相關操作", logger, gitLabService, gitService)
    {
        // 加入子命令
        AddCommand(new ProjectListCommand(logger, gitLabService, gitService));
        AddCommand(new ProjectInfoCommand(logger, gitLabService, gitService));
    }
}

/// <summary>
/// 分支相關命令
/// </summary>
public class BranchCommand : BaseCommand
{
    public BranchCommand(
        ILogger<BranchCommand> logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("branch", "分支相關操作", logger, gitLabService, gitService)
    {
        // 加入子命令
        AddCommand(new BranchListCommand(logger, gitLabService, gitService));
        AddCommand(new BranchCreateCommand(logger, gitLabService, gitService));
        AddCommand(new BranchCheckoutCommand(logger, gitLabService, gitService));
    }
}

/// <summary>
/// Git 同步相關命令
/// </summary>
public class SyncCommand : BaseCommand
{
    public SyncCommand(
        ILogger<SyncCommand> logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("sync", "Git 和 GitLab 同步操作", logger, gitLabService, gitService)
    {
        // 加入子命令
        AddCommand(new PushCommand(logger, gitLabService, gitService));
        AddCommand(new PullCommand(logger, gitLabService, gitService));
    }
}

/// <summary>
/// 狀態相關命令
/// </summary>
public class StatusCommand : BaseCommand
{
    public StatusCommand(
        ILogger<StatusCommand> logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("status", "取得倉庫和專案狀態", logger, gitLabService, gitService)
    {
        // 加入子命令
        AddCommand(new RepoStatusCommand(logger, gitLabService, gitService));
    }
}
