using System.CommandLine;
using GitLabCli.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitLabCli.MCP.Client.Commands.Git;

/// <summary>
/// 倉庫狀態命令
/// </summary>
public class RepoStatusCommand : BaseCommand
{
    public RepoStatusCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("repo", "取得本地倉庫狀態", logger, gitLabService, gitService)
    {
        var repositoryPathOption = new Option<string>(
            "--repo",
            "倉庫路徑 (預設: 目前目錄)"
        );
        repositoryPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

        AddOption(repositoryPathOption);
        this.SetHandler(ExecuteAsync, repositoryPathOption);
    }

    private async Task<int> ExecuteAsync(string repositoryPath)
    {
        try
        {
            ShowInfo("正在取得倉庫狀態...");
            
            var status = await GitService.GetRepositoryStatusAsync(repositoryPath);

            // 建立狀態面板
            var statusText = new Markup($"""
                [bold]目前分支:[/] {status.CurrentBranch}
                [bold]未追蹤檔案:[/] {status.UntrackedFilesCount} 個
                [bold]已修改檔案:[/] {status.ModifiedFilesCount} 個  
                [bold]已暫存檔案:[/] {status.StagedFilesCount} 個
                [bold]有未提交變更:[/] {(status.HasUncommittedChanges ? "是" : "否")}
                [bold]有衝突:[/] {(status.HasConflicts ? "是" : "否")}
                """);

            var panel = new Panel(statusText)
            {
                Header = new PanelHeader("倉庫狀態"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);

            // 如果有衝突，顯示衝突檔案
            if (status.HasConflicts && status.ConflictFiles.Any())
            {
                ShowWarning("發現衝突檔案:");
                var conflictTable = new Table();
                conflictTable.AddColumn("衝突檔案");
                
                foreach (var file in status.ConflictFiles)
                {
                    conflictTable.AddRow(file);
                }
                
                AnsiConsole.Write(conflictTable);
            }

            // 取得本地分支清單
            ShowInfo("正在取得本地分支清單...");
            var branches = await GitService.GetLocalBranchesAsync(repositoryPath);
            var branchList = branches.ToList();

            if (branchList.Any())
            {
                var branchTable = new Table();
                branchTable.AddColumn("分支名稱");
                branchTable.AddColumn("是否目前分支");
                
                foreach (var branch in branchList)
                {
                    branchTable.AddRow(
                        branch.Name,
                        branch.Name == status.CurrentBranch ? "✅" : "-"
                    );
                }
                
                AnsiConsole.Write(new Panel(branchTable)
                {
                    Header = new PanelHeader("本地分支"),
                    Border = BoxBorder.Rounded
                });
            }

            ShowSuccess("倉庫狀態取得完成");
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "取得倉庫狀態");
        }
    }
}
