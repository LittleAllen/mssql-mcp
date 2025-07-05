using FluentAssertions;
using GitLabCli.MCP.Client.Commands.GitLab;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitLabCli.MCP.Client.Tests.Commands.GitLab;

/// <summary>
/// ProjectListCommand 測試
/// </summary>
public class ProjectListCommandTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IGitLabService> _mockGitLabService;
    private readonly Mock<IGitService> _mockGitService;
    private readonly ProjectListCommand _command;

    public ProjectListCommandTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockGitLabService = new Mock<IGitLabService>();
        _mockGitService = new Mock<IGitService>();
        _command = new ProjectListCommand(_mockLogger.Object, _mockGitLabService.Object, _mockGitService.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCommand()
    {
        // Assert
        _command.Should().NotBeNull();
        _command.Name.Should().Be("list");
        _command.Description.Should().Be("取得 GitLab 專案清單");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldCreateCommand()
    {
        // Act
        var command = new ProjectListCommand(null!, _mockGitLabService.Object, _mockGitService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("list");
        command.Description.Should().Contain("專案清單");
    }

    [Fact]
    public void Constructor_WithNullGitLabService_ShouldCreateCommand()
    {
        // Act  
        var command = new ProjectListCommand(_mockLogger.Object, null!, _mockGitService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("list");
        command.Description.Should().Contain("專案清單");
    }

    [Fact]
    public void Constructor_WithNullGitService_ShouldCreateCommand()
    {
        // Act
        var command = new ProjectListCommand(_mockLogger.Object, _mockGitLabService.Object, null!);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("list");
        command.Description.Should().Contain("專案清單");
    }

    [Fact]
    public async Task ExecuteAsync_WithProjects_ShouldReturnSuccessCode()
    {
        // Arrange
        var projects = new List<ProjectInfo>
        {
            new()
            {
                Id = 1,
                Name = "專案1",
                Description = "描述1",
                DefaultBranch = "main",
                LastActivityAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 2,
                Name = "專案2",
                Description = "描述2",
                DefaultBranch = "develop",
                LastActivityAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        // Act
        var result = await InvokeCommandAsync();

        // Assert
        result.Should().Be(0);
        _mockGitLabService.Verify(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoProjects_ShouldReturnSuccessCode()
    {
        // Arrange
        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectInfo>());

        // Act
        var result = await InvokeCommandAsync();

        // Assert
        result.Should().Be(0);
        _mockGitLabService.Verify(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ShouldReturnErrorCode()
    {
        // Arrange
        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("服務錯誤"));

        // Act
        var result = await InvokeCommandAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsOperationCanceledException_ShouldReturnErrorCode()
    {
        // Arrange
        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("操作已取消"));

        // Act
        var result = await InvokeCommandAsync();

        // Assert
        result.Should().Be(1);
    }

    private async Task<int> InvokeCommandAsync()
    {
        // 使用反射來呼叫私有的 ExecuteAsync 方法
        var method = typeof(ProjectListCommand).GetMethod("ExecuteAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method == null)
            throw new InvalidOperationException("找不到 ExecuteAsync 方法");

        var task = (Task<int>)method.Invoke(_command, Array.Empty<object>())!;
        return await task;
    }
}
