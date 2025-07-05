using System.CommandLine;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Requests;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitLabCli.MCP.Client.Commands.Git;

/// <summary>
/// Push 命令
/// </summary>
public class PushCommand : BaseCommand
{
    public PushCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("push", "推送本地變更到 GitLab", logger, gitLabService, gitService)
    {
        var repositoryPathOption = new Option<string>(
            "--repo",
            "倉庫路徑 (預設: 目前目錄)"
        );
        repositoryPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

        var branchOption = new Option<string?>(
            "--branch",
            "指定分支 (預設: 目前分支)"
        );

        var forceOption = new Option<bool>(
            "--force",
            "強制推送"
        );

        AddOption(repositoryPathOption);
        AddOption(branchOption);
        AddOption(forceOption);
        this.SetHandler(ExecuteAsync, repositoryPathOption, branchOption, forceOption);
    }

    private async Task<int> ExecuteAsync(string repositoryPath, string? branch, bool force)
    {
        try
        {
            ShowInfo("正在準備推送...");

            // 取得倉庫狀態
            var status = await GitService.GetRepositoryStatusAsync(repositoryPath);
            
            if (status.HasUncommittedChanges)
            {
                ShowWarning("倉庫有未提交的變更，請先提交變更");
                return 1;
            }

            var targetBranch = branch ?? status.CurrentBranch;
            ShowInfo($"正在推送分支 '{targetBranch}' 到 GitLab...");

            var pushRequest = new PushRequest
            {
                BranchName = targetBranch,
                Force = force
            };

            var result = await GitService.ProcessPushAsync(pushRequest);

            if (result.Success)
            {
                var panel = new Panel(new Markup($"""
                    [bold]分支:[/] {result.BranchName}
                    [bold]提交數:[/] {result.CommitCount}
                    [bold]推送時間:[/] {result.ProcessedAt:yyyy-MM-dd HH:mm:ss}
                    [bold]最新提交:[/] {result.LatestCommitSha}
                    """))
                {
                    Header = new PanelHeader("推送成功"),
                    Border = BoxBorder.Rounded
                };

                AnsiConsole.Write(panel);
                ShowSuccess("推送完成");
                return 0;
            }
            else
            {
                ShowWarning($"推送失敗: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            return HandleException(ex, "推送");
        }
    }
}

/// <summary>
/// Pull 命令
/// </summary>
public class PullCommand : BaseCommand
{
    public PullCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("pull", "從 GitLab 拉取最新變更", logger, gitLabService, gitService)
    {
        var repositoryPathOption = new Option<string>(
            "--repo",
            "倉庫路徑 (預設: 目前目錄)"
        );
        repositoryPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

        var branchOption = new Option<string?>(
            "--branch",
            "指定分支 (預設: 目前分支)"
        );

        var forceOption = new Option<bool>(
            "--force",
            "強制拉取"
        );

        AddOption(repositoryPathOption);
        AddOption(branchOption);
        AddOption(forceOption);
        this.SetHandler(ExecuteAsync, repositoryPathOption, branchOption, forceOption);
    }

    private async Task<int> ExecuteAsync(string repositoryPath, string? branch, bool force)
    {
        try
        {
            ShowInfo("正在準備拉取...");

            // 取得倉庫狀態
            var status = await GitService.GetRepositoryStatusAsync(repositoryPath);
            
            if (status.HasUncommittedChanges && !force)
            {
                ShowWarning("倉庫有未提交的變更，請先提交變更或使用 --force 選項");
                return 1;
            }

            var targetBranch = branch ?? status.CurrentBranch;
            ShowInfo($"正在從 GitLab 拉取分支 '{targetBranch}'...");

            var pullRequest = new PullRequest
            {
                BranchName = targetBranch,
                LocalPath = repositoryPath,
                Force = force
            };

            var result = await GitService.ProcessPullAsync(pullRequest);

            if (result.Success)
            {
                var panel = new Panel(new Markup($"""
                    [bold]分支:[/] {result.BranchName}
                    [bold]新增的提交數:[/] {result.CommitCount}
                    [bold]拉取時間:[/] {result.ProcessedAt:yyyy-MM-dd HH:mm:ss}
                    [bold]最新提交:[/] {result.LatestCommitSha}
                    """))
                {
                    Header = new PanelHeader(result.HasConflicts ? "拉取完成 (有衝突)" : "拉取成功"),
                    Border = BoxBorder.Rounded
                };

                AnsiConsole.Write(panel);

                if (result.HasConflicts)
                {
                    ShowWarning("發現衝突，請手動解決後再次提交");
                    if (result.ConflictFiles.Any())
                    {
                        ShowInfo("衝突檔案:");
                        foreach (var file in result.ConflictFiles)
                        {
                            Console.WriteLine($"  - {file}");
                        }
                    }
                }
                else
                {
                    ShowSuccess("拉取完成");
                }
                
                return result.HasConflicts ? 2 : 0;
            }
            else
            {
                ShowWarning($"拉取失敗: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            return HandleException(ex, "拉取");
        }
    }
}
