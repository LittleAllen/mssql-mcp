using System.CommandLine;

namespace GitLabCli.MCP.Client;

/// <summary>
/// CLI 程式進入點
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("GitLab CLI MCP Client - 基於 MCP 的 GitLab CLI 工具");
        
        // TODO: 加入各種 CLI 命令
        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("GitLab CLI MCP Client");
            Console.WriteLine("使用 --help 查看可用命令");
        });
        
        return await rootCommand.InvokeAsync(args);
    }
}
