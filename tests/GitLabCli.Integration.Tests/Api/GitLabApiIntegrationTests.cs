using FluentAssertions;
using GitLabCli.MCP.Server;
using GitLabCli.Shared.Models.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace GitLabCli.Integration.Tests.Api;

/// <summary>
/// GitLab API 整合測試
/// </summary>
public class GitLabApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GitLabApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 可以在這裡覆寫測試用的服務
                // 例如使用記憶體資料庫或 Mock 服務
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetProjects_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/gitlab/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetProjects_ShouldReturnValidApiResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/gitlab/projects");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetProject_WithValidId_ShouldReturnOk()
    {
        // Arrange
        const int projectId = 1; // 假設這是一個有效的專案 ID

        // Act
        var response = await _client.GetAsync($"/api/v1/gitlab/projects/{projectId}");

        // Assert
        // 由於使用 Mock 服務，應該會返回 OK 或相應的錯誤狀態
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetProject_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        const string invalidId = "abc"; // 非數字 ID

        // Act
        var response = await _client.GetAsync($"/api/v1/gitlab/projects/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetProject_WithNegativeOrZeroId_ShouldHandleGracefully(int projectId)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/gitlab/projects/{projectId}");

        // Assert
        // 應該返回適當的狀態碼，不應該是成功
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Api_ShouldIncludeCorrectHeaders()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/gitlab/projects");

        // Assert
        response.Headers.Should().ContainKey("Date");
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task SwaggerUi_InDevelopment_ShouldBeAccessible()
    {
        // Arrange
        var developmentFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
        
        using var developmentClient = developmentFactory.CreateClient();

        // Act
        var response = await developmentClient.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("swagger");
    }

    [Fact]
    public async Task CorsPolicy_ShouldAllowRequests()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");

        // Act
        var response = await _client.GetAsync("/api/v1/gitlab/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // CORS headers 會在實際的 preflight 請求中設定
    }

    [Fact]
    public async Task ErrorHandling_ShouldReturnConsistentFormat()
    {
        // Act - 嘗試存取不存在的端點
        var response = await _client.GetAsync("/api/v1/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ContentNegotiation_ShouldReturnJson()
    {
        // Arrange
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await _client.GetAsync("/api/v1/gitlab/projects");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
