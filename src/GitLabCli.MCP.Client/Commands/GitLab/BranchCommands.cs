using System.CommandLine;
using GitLabCli.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitLabCli.MCP.Client.Commands.GitLab;

/// <summary>
/// 取得分支清單命令
/// </summary>
public class BranchListCommand : BaseCommand
{
    public BranchListCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("list", "取得分支清單", logger, gitLabService, gitService)
    {
        var projectIdOption = new Option<int>(
            "--project-id",
            "專案 ID"
        ) { IsRequired = true };

        AddOption(projectIdOption);
        this.SetHandler(ExecuteAsync, projectIdOption);
    }

    private async Task<int> ExecuteAsync(int projectId)
    {
        try
        {
            ShowInfo($"正在取得專案 {projectId} 的分支清單...");
            
            var branches = await GitLabService.GetBranchesAsync(projectId);
            var branchList = branches.ToList();

            if (!branchList.Any())
            {
                ShowWarning("找不到任何分支");
                return 0;
            }

            // 使用 Spectre.Console 建立表格
            var table = new Table();
            table.AddColumn("分支名稱");
            table.AddColumn("是否預設");
            table.AddColumn("是否受保護");
            table.AddColumn("最後提交");
            table.AddColumn("提交者");

            foreach (var branch in branchList)
            {
                table.AddRow(
                    branch.Name,
                    branch.IsDefault ? "✅" : "-",
                    branch.IsProtected ? "🔒" : "-",
                    branch.CommitSha.Substring(0, Math.Min(8, branch.CommitSha.Length)),
                    branch.CommitAuthor ?? "-"
                );
            }

            AnsiConsole.Write(table);
            ShowSuccess($"共找到 {branchList.Count} 個分支");
            
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "取得分支清單");
        }
    }
}

/// <summary>
/// 建立分支命令
/// </summary>
public class BranchCreateCommand : BaseCommand
{
    public BranchCreateCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("create", "建立新分支", logger, gitLabService, gitService)
    {
        var projectIdOption = new Option<int>(
            "--project-id",
            "專案 ID"
        ) { IsRequired = true };

        var branchNameOption = new Option<string>(
            "--name",
            "分支名稱"
        ) { IsRequired = true };

        var sourceBranchOption = new Option<string>(
            "--source",
            "來源分支"
        ) { IsRequired = true };

        AddOption(projectIdOption);
        AddOption(branchNameOption);
        AddOption(sourceBranchOption);
        this.SetHandler(ExecuteAsync, projectIdOption, branchNameOption, sourceBranchOption);
    }

    private async Task<int> ExecuteAsync(int projectId, string branchName, string sourceBranch)
    {
        try
        {
            ShowInfo($"正在建立分支 '{branchName}' (來源: {sourceBranch})...");
            
            var branch = await GitLabService.CreateBranchAsync(projectId, branchName, sourceBranch);

            var panel = new Panel(new Markup($"""
                [bold]分支名稱:[/] {branch.Name}
                [bold]來源分支:[/] {sourceBranch}
                [bold]最後提交:[/] {branch.CommitSha.Substring(0, Math.Min(8, branch.CommitSha.Length))}
                [bold]提交訊息:[/] {branch.CommitMessage ?? "無"}
                """))
            {
                Header = new PanelHeader("分支建立成功"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
            ShowSuccess($"分支 '{branchName}' 建立完成");
            
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "建立分支");
        }
    }
}

/// <summary>
/// 切換分支命令
/// </summary>
public class BranchCheckoutCommand : BaseCommand
{
    public BranchCheckoutCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("checkout", "切換到指定分支", logger, gitLabService, gitService)
    {
        var branchNameOption = new Option<string>(
            "--name",
            "分支名稱"
        ) { IsRequired = true };

        var repositoryPathOption = new Option<string>(
            "--repo",
            "倉庫路徑 (預設: 目前目錄)"
        );
        repositoryPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

        AddOption(branchNameOption);
        AddOption(repositoryPathOption);
        this.SetHandler(ExecuteAsync, branchNameOption, repositoryPathOption);
    }

    private async Task<int> ExecuteAsync(string branchName, string repositoryPath)
    {
        try
        {
            ShowInfo($"正在切換到分支 '{branchName}'...");
            
            var success = await GitService.CheckoutBranchAsync(repositoryPath, branchName);

            if (success)
            {
                ShowSuccess($"已成功切換到分支 '{branchName}'");
                return 0;
            }
            else
            {
                ShowWarning($"無法切換到分支 '{branchName}'，請檢查分支是否存在");
                return 1;
            }
        }
        catch (Exception ex)
        {
            return HandleException(ex, "切換分支");
        }
    }
}
