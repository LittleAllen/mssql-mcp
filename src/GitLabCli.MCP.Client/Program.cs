using System.CommandLine;
using GitLabCli.MCP.Client.Commands;
using GitLabCli.MCP.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitLabCli.MCP.Client;

/// <summary>
/// CLI 程式進入點
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // 建立配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 建立服務容器
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLoggingServices();
            services.AddApplicationServices(configuration);

            // 註冊命令
            services.AddTransient<ProjectCommand>();
            services.AddTransient<BranchCommand>();
            services.AddTransient<SyncCommand>();
            services.AddTransient<StatusCommand>();

            var serviceProvider = services.BuildServiceProvider();

            // 建立根命令
            var rootCommand = new RootCommand("GitLab CLI MCP Client - 基於 MCP 的 GitLab CLI 工具");

            // 加入詳細輸出選項
            var verboseOption = new Option<bool>(
                new[] { "--verbose", "--debug" },
                "顯示詳細輸出"
            );
            rootCommand.AddGlobalOption(verboseOption);

            // 加入配置檔案選項
            var configOption = new Option<string?>(
                new[] { "--config", "-c" },
                "指定配置檔案路徑"
            );
            rootCommand.AddGlobalOption(configOption);

            // 加入主要命令
            rootCommand.AddCommand(serviceProvider.GetRequiredService<ProjectCommand>());
            rootCommand.AddCommand(serviceProvider.GetRequiredService<BranchCommand>());
            rootCommand.AddCommand(serviceProvider.GetRequiredService<SyncCommand>());
            rootCommand.AddCommand(serviceProvider.GetRequiredService<StatusCommand>());

            // 設定根命令處理器
            rootCommand.SetHandler((bool verbose) =>
            {
                Console.WriteLine("GitLab CLI MCP Client");
                Console.WriteLine("=====================================");
                Console.WriteLine();
                Console.WriteLine("可用命令:");
                Console.WriteLine("  project    專案相關操作");
                Console.WriteLine("  branch     分支相關操作");
                Console.WriteLine("  sync       Git 和 GitLab 同步操作");
                Console.WriteLine("  status     取得倉庫和專案狀態");
                Console.WriteLine();
                Console.WriteLine("使用 'gitlab-cli <命令> --help' 查看命令的詳細說明");
                Console.WriteLine();
                Console.WriteLine("範例:");
                Console.WriteLine("  gitlab-cli project list");
                Console.WriteLine("  gitlab-cli branch list --project-id 123");
                Console.WriteLine("  gitlab-cli sync push");
                Console.WriteLine("  gitlab-cli status repo");
                
            }, verboseOption);

            // 執行命令
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 應用程式啟動失敗: {ex.Message}");
            return 1;
        }
    }
}
