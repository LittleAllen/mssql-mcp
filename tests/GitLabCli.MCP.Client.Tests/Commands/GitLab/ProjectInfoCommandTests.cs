using FluentAssertions;
using GitLabCli.MCP.Client.Commands.GitLab;
using GitLabCli.Shared.Exceptions;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;

namespace GitLabCli.MCP.Client.Tests.Commands.GitLab;

/// <summary>
/// ProjectInfoCommand 測試
/// </summary>
public class ProjectInfoCommandTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IGitLabService> _mockGitLabService;
    private readonly Mock<IGitService> _mockGitService;
    private readonly ProjectInfoCommand _command;

    public ProjectInfoCommandTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockGitLabService = new Mock<IGitLabService>();
        _mockGitService = new Mock<IGitService>();
        _command = new ProjectInfoCommand(_mockLogger.Object, _mockGitLabService.Object, _mockGitService.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCommand()
    {
        // Assert
        _command.Should().NotBeNull();
        _command.Name.Should().Be("info");
        _command.Description.Should().Be("取得專案詳細資訊");
    }

    [Fact]
    public void Constructor_ShouldHaveRequiredProjectIdOption()
    {
        // Assert
        var projectIdOption = _command.Options.FirstOrDefault(o => o.Name == "project-id");
        projectIdOption.Should().NotBeNull();
        projectIdOption!.IsRequired.Should().BeTrue();
        projectIdOption.ValueType.Should().Be(typeof(int));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldCreateCommand()
    {
        // Act
        var command = new ProjectInfoCommand(null!, _mockGitLabService.Object, _mockGitService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("info");
        command.Description.Should().Contain("專案詳細資訊");
    }

    [Fact]
    public void Constructor_WithNullGitLabService_ShouldCreateCommand()
    {
        // Act
        var command = new ProjectInfoCommand(_mockLogger.Object, null!, _mockGitService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("info");
        command.Description.Should().Contain("專案詳細資訊");
    }

    [Fact]
    public void Constructor_WithNullGitService_ShouldCreateCommand()
    {
        // Act
        var command = new ProjectInfoCommand(_mockLogger.Object, _mockGitLabService.Object, null!);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("info");
        command.Description.Should().Contain("專案詳細資訊");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidProjectId_ShouldReturnSuccessCode()
    {
        // Arrange
        const int projectId = 123;
        var project = new ProjectInfo
        {
            Id = projectId,
            Name = "測試專案",
            Description = "測試描述",
            DefaultBranch = "main",
            IsPrivate = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastActivityAt = DateTime.UtcNow.AddHours(-1),
            HttpUrlToRepo = "https://gitlab.example.com/test/project.git",
            SshUrlToRepo = "git@gitlab.example.com:test/project.git"
        };

        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await InvokeCommandAsync(projectId);

        // Assert
        result.Should().Be(0);
        _mockGitLabService.Verify(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ProjectNotFound_ShouldReturnErrorCode()
    {
        // Arrange
        const int projectId = 999;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GitLabApiException(404, "NOT_FOUND", "專案不存在"));

        // Act
        var result = await InvokeCommandAsync(projectId);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ShouldReturnErrorCode()
    {
        // Arrange
        const int projectId = 123;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("服務錯誤"));

        // Act
        var result = await InvokeCommandAsync(projectId);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsGitLabApiException_ShouldReturnErrorCode()
    {
        // Arrange
        const int projectId = 123;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GitLabApiException(500, "INTERNAL_ERROR", "內部錯誤"));

        // Act
        var result = await InvokeCommandAsync(projectId);

        // Assert
        result.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(123)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public async Task ExecuteAsync_WithVariousProjectIds_ShouldCallServiceWithCorrectId(int projectId)
    {
        // Arrange
        var project = new ProjectInfo { Id = projectId, Name = $"專案{projectId}" };
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        await InvokeCommandAsync(projectId);

        // Assert
        _mockGitLabService.Verify(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_OperationCancelled_ShouldReturnErrorCode()
    {
        // Arrange
        const int projectId = 123;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("操作已取消"));

        // Act
        var result = await InvokeCommandAsync(projectId);

        // Assert
        result.Should().Be(1);
    }

    private async Task<int> InvokeCommandAsync(int projectId)
    {
        // 使用反射來呼叫私有的 ExecuteAsync 方法
        var method = typeof(ProjectInfoCommand).GetMethod("ExecuteAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new[] { typeof(int) },
            null);
        
        if (method == null)
            throw new InvalidOperationException("找不到 ExecuteAsync 方法");

        var task = (Task<int>)method.Invoke(_command, new object[] { projectId })!;
        return await task;
    }
}
