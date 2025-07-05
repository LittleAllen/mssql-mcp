using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Interfaces;
using GitLabCli.MCP.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GitLabCli.MCP.Client.Extensions;

/// <summary>
/// 服務註冊擴展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 註冊應用程式服務
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 註冊配置選項
        services.Configure<GitLabOptions>(configuration.GetSection(GitLabOptions.SectionName));
        services.Configure<GitOptions>(configuration.GetSection(GitOptions.SectionName));
        services.Configure<McpOptions>(configuration.GetSection(McpOptions.SectionName));

        // 註冊 HTTP 客戶端
        services.AddHttpClient();

        // 註冊應用程式服務
        services.AddSingleton<IGitLabService, MockGitLabService>();
        services.AddSingleton<IGitService, MockGitService>();
        services.AddSingleton<IMcpClient, MockMcpClient>();

        return services;
    }

    /// <summary>
    /// 註冊日誌服務
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return services;
    }
}
