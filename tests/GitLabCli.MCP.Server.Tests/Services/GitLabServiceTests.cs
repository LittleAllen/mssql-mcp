using FluentAssertions;
using GitLabCli.MCP.Server.Services.Implementations;
using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace GitLabCli.MCP.Server.Tests.Services;

/// <summary>
/// GitLabService 測試
/// </summary>
public class GitLabServiceTests : IDisposable
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<GitLabService>> _mockLogger;
    private readonly Mock<IOptions<GitLabOptions>> _mockOptions;
    private readonly GitLabOptions _gitLabOptions;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly GitLabService _gitLabService;

    public GitLabServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<GitLabService>>();
        _mockOptions = new Mock<IOptions<GitLabOptions>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _gitLabOptions = new GitLabOptions
        {
            BaseUrl = "https://gitlab.example.com",
            AccessToken = "test-token",
            ProjectId = 123,
            Timeout = TimeSpan.FromSeconds(30)
        };

        _mockOptions.Setup(x => x.Value).Returns(_gitLabOptions);

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_gitLabOptions.BaseUrl)
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _gitLabService = new GitLabService(_mockOptions.Object, _mockLogger.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task GetProjectAsync_ValidProjectId_ReturnsProjectInfo()
    {
        // Arrange
        const int projectId = 123;
        var expectedProject = new
        {
            id = projectId,
            name = "測試專案",
            description = "測試描述",
            http_url_to_repo = "https://gitlab.example.com/test/project.git",
            ssh_url_to_repo = "git@gitlab.example.com:test/project.git",
            default_branch = "main",
            visibility = "private",
            created_at = DateTime.UtcNow.AddDays(-30),
            last_activity_at = DateTime.UtcNow.AddHours(-1)
        };

        var jsonResponse = JsonSerializer.Serialize(expectedProject);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"projects/{projectId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _gitLabService.GetProjectAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(projectId);
        result.Name.Should().Be("測試專案");
        result.Description.Should().Be("測試描述");
        result.IsPrivate.Should().BeTrue();
        result.DefaultBranch.Should().Be("main");
    }

    [Fact]
    public async Task GetProjectAsync_ProjectNotFound_ThrowsGitLabApiException()
    {
        // Arrange
        const int projectId = 999;
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GitLabApiException>(
            () => _gitLabService.GetProjectAsync(projectId));

        exception.StatusCode.Should().Be(404);
        exception.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task GetProjectsAsync_ValidRequest_ReturnsProjectList()
    {
        // Arrange
        var expectedProjects = new[]
        {
            new
            {
                id = 1,
                name = "專案1",
                description = "描述1",
                http_url_to_repo = "https://gitlab.example.com/test/project1.git",
                ssh_url_to_repo = "git@gitlab.example.com:test/project1.git",
                default_branch = "main",
                visibility = "public",
                created_at = DateTime.UtcNow.AddDays(-60),
                last_activity_at = DateTime.UtcNow.AddDays(-1)
            },
            new
            {
                id = 2,
                name = "專案2",
                description = "描述2",
                http_url_to_repo = "https://gitlab.example.com/test/project2.git",
                ssh_url_to_repo = "git@gitlab.example.com:test/project2.git",
                default_branch = "develop",
                visibility = "private",
                created_at = DateTime.UtcNow.AddDays(-30),
                last_activity_at = DateTime.UtcNow.AddHours(-2)
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedProjects);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("projects")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _gitLabService.GetProjectsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var projectList = result.ToList();
        projectList[0].Id.Should().Be(1);
        projectList[0].Name.Should().Be("專案1");
        projectList[0].IsPrivate.Should().BeFalse();
        
        projectList[1].Id.Should().Be(2);
        projectList[1].Name.Should().Be("專案2");
        projectList[1].IsPrivate.Should().BeTrue();
    }

    [Fact]
    public async Task GetProjectAsync_HttpRequestException_ThrowsGitLabApiException()
    {
        // Arrange
        const int projectId = 123;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("網路錯誤"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GitLabApiException>(
            () => _gitLabService.GetProjectAsync(projectId));

        exception.StatusCode.Should().Be(500);
        exception.ErrorCode.Should().Be("NETWORK_ERROR");
    }

    [Fact]
    public async Task GetProjectAsync_TaskCancelledException_ThrowsGitLabApiException()
    {
        // Arrange
        const int projectId = 123;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("請求逾時", new TimeoutException()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GitLabApiException>(
            () => _gitLabService.GetProjectAsync(projectId));

        exception.StatusCode.Should().Be(408);
        exception.ErrorCode.Should().Be("TIMEOUT");
    }

    [Theory]
    [InlineData(400, "BAD_REQUEST")]
    [InlineData(401, "UNAUTHORIZED")]
    [InlineData(403, "FORBIDDEN")]
    [InlineData(500, "INTERNAL_SERVER_ERROR")]
    public async Task GetProjectAsync_VariousHttpErrors_ThrowsCorrectGitLabApiException(int statusCode, string expectedErrorCode)
    {
        // Arrange
        const int projectId = 123;
        var httpResponse = new HttpResponseMessage((HttpStatusCode)statusCode);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GitLabApiException>(
            () => _gitLabService.GetProjectAsync(projectId));

        exception.StatusCode.Should().Be(statusCode);
        exception.ErrorCode.Should().Be(expectedErrorCode);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
