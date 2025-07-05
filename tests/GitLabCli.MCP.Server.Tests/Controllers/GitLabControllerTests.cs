using FluentAssertions;
using GitLabCli.MCP.Server.Controllers;
using GitLabCli.Shared.Exceptions;
using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitLabCli.MCP.Server.Tests.Controllers;

/// <summary>
/// GitLabController 測試
/// </summary>
public class GitLabControllerTests
{
    private readonly Mock<IGitLabService> _mockGitLabService;
    private readonly Mock<ILogger<GitLabController>> _mockLogger;
    private readonly GitLabController _controller;

    public GitLabControllerTests()
    {
        _mockGitLabService = new Mock<IGitLabService>();
        _mockLogger = new Mock<ILogger<GitLabController>>();
        _controller = new GitLabController(_mockGitLabService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProjects_ValidRequest_ReturnsOkWithProjects()
    {
        // Arrange
        var expectedProjects = new List<ProjectInfo>
        {
            new()
            {
                Id = 1,
                Name = "專案1",
                Description = "描述1",
                DefaultBranch = "main",
                IsPrivate = false
            },
            new()
            {
                Id = 2,
                Name = "專案2",
                Description = "描述2",
                DefaultBranch = "develop",
                IsPrivate = true
            }
        };

        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProjects);

        // Act
        var result = await _controller.GetProjects();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var apiResponse = okResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(expectedProjects);
        apiResponse.Message.Should().Be("專案清單取得成功");
    }

    [Fact]
    public async Task GetProjects_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("服務錯誤"));

        // Act
        var result = await _controller.GetProjects();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();
        
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.ErrorCode.Should().Be("GET_PROJECTS_ERROR");
    }

    [Fact]
    public async Task GetProject_ValidProjectId_ReturnsOkWithProject()
    {
        // Arrange
        const int projectId = 123;
        var expectedProject = new ProjectInfo
        {
            Id = projectId,
            Name = "測試專案",
            Description = "測試描述",
            DefaultBranch = "main",
            IsPrivate = true
        };

        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var apiResponse = okResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(expectedProject);
        apiResponse.Message.Should().Be("專案資訊取得成功");
    }

    [Fact]
    public async Task GetProject_ProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        const int projectId = 999;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GitLabApiException(404, "NOT_FOUND", "專案不存在"));

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();
        
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Theory]
    [InlineData(400, "BAD_REQUEST")]
    [InlineData(401, "UNAUTHORIZED")]
    [InlineData(403, "FORBIDDEN")]
    [InlineData(500, "INTERNAL_SERVER_ERROR")]
    public async Task GetProject_GitLabApiException_ReturnsCorrectStatusCode(int statusCode, string errorCode)
    {
        // Arrange
        const int projectId = 123;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GitLabApiException(statusCode, errorCode, "測試錯誤"));

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();
        
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(statusCode);
        
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.ErrorCode.Should().Be(errorCode);
    }

    [Fact]
    public async Task GetProject_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        const int projectId = 123;
        
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("一般錯誤"));

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ObjectResult>();
        
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.ErrorCode.Should().Be("GET_PROJECT_ERROR");
    }

    [Fact]
    public void Constructor_WithNullGitLabService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new GitLabController(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new GitLabController(_mockGitLabService.Object, null!));
    }

    [Fact]
    public async Task GetProjects_VerifyServiceMethodCalled()
    {
        // Arrange
        _mockGitLabService.Setup(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectInfo>());

        // Act
        await _controller.GetProjects();

        // Assert
        _mockGitLabService.Verify(x => x.GetProjectsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProject_VerifyServiceMethodCalledWithCorrectId()
    {
        // Arrange
        const int projectId = 456;
        _mockGitLabService.Setup(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectInfo { Id = projectId });

        // Act
        await _controller.GetProject(projectId);

        // Assert
        _mockGitLabService.Verify(x => x.GetProjectAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
