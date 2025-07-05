using System.CommandLine;
using GitLabCli.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace GitLabCli.MCP.Client.Commands;

/// <summary>
/// 基礎命令類別
/// </summary>
public abstract class BaseCommand : Command
{
    protected readonly ILogger Logger;
    protected readonly IGitLabService GitLabService;
    protected readonly IGitService GitService;

    protected BaseCommand(
        string name, 
        string description,
        ILogger logger,
        IGitLabService gitLabService,
        IGitService gitService) 
        : base(name, description)
    {
        Logger = logger;
        GitLabService = gitLabService;
        GitService = gitService;
    }

    /// <summary>
    /// 處理一般例外
    /// </summary>
    /// <param name="ex">例外物件</param>
    /// <param name="operation">操作名稱</param>
    /// <returns>錯誤碼</returns>
    protected int HandleException(Exception ex, string operation)
    {
        Logger.LogError(ex, "執行 {Operation} 時發生錯誤: {Message}", operation, ex.Message);
        Console.WriteLine($"❌ 錯誤: {ex.Message}");
        return 1;
    }

    /// <summary>
    /// 顯示成功訊息
    /// </summary>
    /// <param name="message">訊息</param>
    protected void ShowSuccess(string message)
    {
        Console.WriteLine($"✅ {message}");
    }

    /// <summary>
    /// 顯示資訊訊息
    /// </summary>
    /// <param name="message">訊息</param>
    protected void ShowInfo(string message)
    {
        Console.WriteLine($"ℹ️  {message}");
    }

    /// <summary>
    /// 顯示警告訊息
    /// </summary>
    /// <param name="message">訊息</param>
    protected void ShowWarning(string message)
    {
        Console.WriteLine($"⚠️  {message}");
    }
}
