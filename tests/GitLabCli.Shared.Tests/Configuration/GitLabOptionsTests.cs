using FluentAssertions;
using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Tests.TestHelpers;

namespace GitLabCli.Shared.Tests.Configuration;

/// <summary>
/// GitLabOptions 測試
/// </summary>
public class GitLabOptionsTests : TestBase
{
    [Fact]
    public void SectionName_ShouldBeGitLab()
    {
        // Act & Assert
        GitLabOptions.SectionName.Should().Be("GitLab");
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var options = new GitLabOptions();

        // Assert
        options.BaseUrl.Should().Be("https://gitlab.com");
        options.AccessToken.Should().Be(string.Empty);
        options.ProjectId.Should().Be(0);
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new GitLabOptions();
        const string baseUrl = "https://gitlab.example.com";
        const string accessToken = "test-token-123";
        const int projectId = 456;
        var timeout = TimeSpan.FromMinutes(2);

        // Act
        options.BaseUrl = baseUrl;
        options.AccessToken = accessToken;
        options.ProjectId = projectId;
        options.Timeout = timeout;

        // Assert
        options.BaseUrl.Should().Be(baseUrl);
        options.AccessToken.Should().Be(accessToken);
        options.ProjectId.Should().Be(projectId);
        options.Timeout.Should().Be(timeout);
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://gitlab.com")]
    [InlineData("https://gitlab.example.com")]
    [InlineData("http://localhost:8080")]
    public void BaseUrl_ShouldAcceptVariousUrls(string url)
    {
        // Arrange
        var options = new GitLabOptions();

        // Act
        options.BaseUrl = url;

        // Assert
        options.BaseUrl.Should().Be(url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("token123")]
    [InlineData("glpat-xxxxxxxxxxxxxxxxxxxx")]
    public void AccessToken_ShouldAcceptVariousTokenFormats(string token)
    {
        // Arrange
        var options = new GitLabOptions();

        // Act
        options.AccessToken = token;

        // Assert
        options.AccessToken.Should().Be(token);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(123456)]
    [InlineData(int.MaxValue)]
    public void ProjectId_ShouldAcceptValidIds(int projectId)
    {
        // Arrange
        var options = new GitLabOptions();

        // Act
        options.ProjectId = projectId;

        // Assert
        options.ProjectId.Should().Be(projectId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(300)]
    public void Timeout_ShouldAcceptValidTimeouts(int seconds)
    {
        // Arrange
        var options = new GitLabOptions();
        var timeout = TimeSpan.FromSeconds(seconds);

        // Act
        options.Timeout = timeout;

        // Assert
        options.Timeout.Should().Be(timeout);
    }
}
