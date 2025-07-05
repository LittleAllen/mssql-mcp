using System.CommandLine;
using GitLabCli.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitLabCli.MCP.Client.Commands.GitLab;

/// <summary>
/// 取得專案清單命令
/// </summary>
public class ProjectListCommand : BaseCommand
{
    public ProjectListCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("list", "取得 GitLab 專案清單", logger, gitLabService, gitService)
    {
        this.SetHandler(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync()
    {
        try
        {
            ShowInfo("正在取得專案清單...");
            
            var projects = await GitLabService.GetProjectsAsync();
            var projectList = projects.ToList();

            if (!projectList.Any())
            {
                ShowWarning("找不到任何專案");
                return 0;
            }

            // 使用 Spectre.Console 建立表格
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("名稱");
            table.AddColumn("描述");
            table.AddColumn("預設分支");
            table.AddColumn("最後活動");

            foreach (var project in projectList)
            {
                table.AddRow(
                    project.Id.ToString(),
                    project.Name,
                    project.Description ?? "-",
                    project.DefaultBranch,
                    project.LastActivityAt.ToString("yyyy-MM-dd") ?? "-"
                );
            }

            AnsiConsole.Write(table);
            ShowSuccess($"共找到 {projectList.Count} 個專案");
            
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "取得專案清單");
        }
    }
}

/// <summary>
/// 取得專案詳細資訊命令
/// </summary>
public class ProjectInfoCommand : BaseCommand
{
    public ProjectInfoCommand(
        ILogger logger, 
        IGitLabService gitLabService, 
        IGitService gitService) 
        : base("info", "取得專案詳細資訊", logger, gitLabService, gitService)
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
            ShowInfo($"正在取得專案 {projectId} 的資訊...");
            
            var project = await GitLabService.GetProjectAsync(projectId);

            // 建立資訊面板
            var panel = new Panel(new Markup($"""
                [bold]專案名稱:[/] {project.Name}
                [bold]描述:[/] {project.Description ?? "無"}
                [bold]預設分支:[/] {project.DefaultBranch}
                [bold]私有專案:[/] {(project.IsPrivate ? "是" : "否")}
                [bold]建立時間:[/] {project.CreatedAt:yyyy-MM-dd HH:mm:ss}
                [bold]最後活動:[/] {project.LastActivityAt:yyyy-MM-dd HH:mm:ss}
                [bold]HTTP URL:[/] {project.HttpUrlToRepo}
                [bold]SSH URL:[/] {project.SshUrlToRepo}
                """))
            {
                Header = new PanelHeader($"專案資訊 - ID: {project.Id}"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
            ShowSuccess("專案資訊取得完成");
            
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "取得專案資訊");
        }
    }
}
