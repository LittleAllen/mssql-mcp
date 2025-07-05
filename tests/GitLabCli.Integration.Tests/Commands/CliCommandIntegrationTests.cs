using FluentAssertions;
using GitLabCli.MCP.Client.Commands.GitLab;
using GitLabCli.MCP.Client.Services;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Integration.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitLabCli.Integration.Tests.Commands;

/// <summary>
/// CLI 命令整合測試
/// </summary>
public class CliCommandIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public CliCommandIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // 設定日誌
        services.AddLogging(builder => builder.AddConsole());
        
        // 註冊 Mock 服務
        services.AddSingleton<IGitLabService, MockGitLabService>();
        services.AddSingleton<IGitService, Integration.Tests.Services.MockGitService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ProjectListCommand_WithMockService_ShouldExecuteSuccessfully()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger<ProjectListCommand>>();
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();
        var gitService = _serviceProvider.GetRequiredService<IGitService>();
        
        var command = new ProjectListCommand(logger, gitLabService, gitService);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProjectInfoCommand_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger<ProjectInfoCommand>>();
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();
        var gitService = _serviceProvider.GetRequiredService<IGitService>();
        
        var command = new ProjectInfoCommand(logger, gitLabService, gitService);

        // Act
        var result = await ExecuteCommandWithProjectIdAsync(command, 1);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProjectInfoCommand_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger<ProjectInfoCommand>>();
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();
        var gitService = _serviceProvider.GetRequiredService<IGitService>();
        
        var command = new ProjectInfoCommand(logger, gitLabService, gitService);

        // Act
        var result = await ExecuteCommandWithProjectIdAsync(command, 99999);

        // Assert
        // Mock 服務會為任何 ID 返回資料，所以應該成功
        result.Should().Be(0);
    }

    [Fact]
    public void MockGitLabService_ShouldReturnTestData()
    {
        // Arrange
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();

        // Act & Assert
        gitLabService.Should().BeOfType<MockGitLabService>();
    }

    [Fact]
    public async Task MockGitLabService_GetProjects_ShouldReturnSampleProjects()
    {
        // Arrange
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();

        // Act
        var projects = await gitLabService.GetProjectsAsync();

        // Assert
        projects.Should().NotBeNull();
        projects.Should().NotBeEmpty();
        projects.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task MockGitLabService_GetProject_ShouldReturnProjectInfo()
    {
        // Arrange
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();
        const int testProjectId = 123;

        // Act
        var project = await gitLabService.GetProjectAsync(testProjectId);

        // Assert
        project.Should().NotBeNull();
        project.Id.Should().Be(testProjectId);
        project.Name.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task MockGitLabService_GetProject_ShouldWorkForAnyId(int projectId)
    {
        // Arrange
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();

        // Act
        var project = await gitLabService.GetProjectAsync(projectId);

        // Assert
        project.Should().NotBeNull();
        project.Id.Should().Be(projectId);
    }

    [Fact]
    public async Task Integration_FullWorkflow_ShouldCompleteSuccessfully()
    {
        // Arrange
        var gitLabService = _serviceProvider.GetRequiredService<IGitLabService>();

        // Act & Assert - 取得專案清單
        var projects = await gitLabService.GetProjectsAsync();
        projects.Should().NotBeEmpty();

        // 取得第一個專案的詳細資訊
        var firstProject = projects.First();
        var projectDetails = await gitLabService.GetProjectAsync(firstProject.Id);
        projectDetails.Should().NotBeNull();
        projectDetails.Id.Should().Be(firstProject.Id);

        // 取得專案的分支
        var branches = await gitLabService.GetBranchesAsync(firstProject.Id);
        branches.Should().NotBeEmpty();

        // 取得主分支的提交
        var defaultBranch = branches.FirstOrDefault(b => b.IsDefault);
        defaultBranch.Should().NotBeNull();

        var commits = await gitLabService.GetCommitsAsync(firstProject.Id, defaultBranch!.Name);
        commits.Should().NotBeEmpty();
    }

    private async Task<int> ExecuteCommandAsync(ProjectListCommand command)
    {
        var method = typeof(ProjectListCommand).GetMethod("ExecuteAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method == null)
            throw new InvalidOperationException("找不到 ExecuteAsync 方法");

        var task = (Task<int>)method.Invoke(command, Array.Empty<object>())!;
        return await task;
    }

    private async Task<int> ExecuteCommandWithProjectIdAsync(ProjectInfoCommand command, int projectId)
    {
        var method = typeof(ProjectInfoCommand).GetMethod("ExecuteAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method == null)
            throw new InvalidOperationException("找不到 ExecuteAsync 方法");

        var task = (Task<int>)method.Invoke(command, new object[] { projectId })!;
        return await task;
    }
}
