using FluentAssertions;
using GitLabCli.Shared.Models.Entities;
using GitLabCli.Shared.Tests.TestHelpers;

namespace GitLabCli.Shared.Tests.Models.Entities;

/// <summary>
/// ProjectInfo 測試
/// </summary>
public class ProjectInfoTests : TestBase
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var projectInfo = new ProjectInfo();

        // Assert
        projectInfo.Id.Should().Be(0);
        projectInfo.Name.Should().Be(string.Empty);
        projectInfo.Description.Should().Be(string.Empty);
        projectInfo.HttpUrlToRepo.Should().Be(string.Empty);
        projectInfo.SshUrlToRepo.Should().Be(string.Empty);
        projectInfo.DefaultBranch.Should().Be(string.Empty);
        projectInfo.IsPrivate.Should().BeFalse();
        projectInfo.CreatedAt.Should().Be(default);
        projectInfo.LastActivityAt.Should().Be(default);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var projectInfo = new ProjectInfo();
        const int id = 123;
        const string name = "測試專案";
        const string description = "這是測試專案";
        const string httpUrl = "https://gitlab.com/test/repo.git";
        const string sshUrl = "git@gitlab.com:test/repo.git";
        const string defaultBranch = "main";
        const bool isPrivate = true;
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var lastActivityAt = DateTime.UtcNow.AddHours(-1);

        // Act
        projectInfo.Id = id;
        projectInfo.Name = name;
        projectInfo.Description = description;
        projectInfo.HttpUrlToRepo = httpUrl;
        projectInfo.SshUrlToRepo = sshUrl;
        projectInfo.DefaultBranch = defaultBranch;
        projectInfo.IsPrivate = isPrivate;
        projectInfo.CreatedAt = createdAt;
        projectInfo.LastActivityAt = lastActivityAt;

        // Assert
        projectInfo.Id.Should().Be(id);
        projectInfo.Name.Should().Be(name);
        projectInfo.Description.Should().Be(description);
        projectInfo.HttpUrlToRepo.Should().Be(httpUrl);
        projectInfo.SshUrlToRepo.Should().Be(sshUrl);
        projectInfo.DefaultBranch.Should().Be(defaultBranch);
        projectInfo.IsPrivate.Should().Be(isPrivate);
        projectInfo.CreatedAt.Should().Be(createdAt);
        projectInfo.LastActivityAt.Should().Be(lastActivityAt);
    }

    [Fact]
    public void Builder_ShouldCreateValidProjectInfo()
    {
        // Act
        var projectInfo = TestData.ProjectInfo()
            .WithId(456)
            .WithName("建構器測試專案")
            .WithDescription("使用建構器建立的測試專案")
            .AsPrivate()
            .WithDefaultBranch("develop")
            .Build();

        // Assert
        projectInfo.Id.Should().Be(456);
        projectInfo.Name.Should().Be("建構器測試專案");
        projectInfo.Description.Should().Be("使用建構器建立的測試專案");
        projectInfo.IsPrivate.Should().BeTrue();
        projectInfo.DefaultBranch.Should().Be("develop");
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("測試專案", true)]
    [InlineData("Test Project", true)]
    [InlineData("專案名稱包含特殊字元!@#$", true)]
    public void Name_ShouldHandleVariousValues(string name, bool hasValue)
    {
        // Arrange
        var projectInfo = new ProjectInfo { Name = name };

        // Act & Assert
        projectInfo.Name.Should().Be(name);
        (!string.IsNullOrEmpty(projectInfo.Name)).Should().Be(hasValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsPrivate_ShouldToggleCorrectly(bool isPrivate)
    {
        // Arrange
        var projectInfo = TestData.ProjectInfo()
            .Build();

        // Act
        projectInfo.IsPrivate = isPrivate;

        // Assert
        projectInfo.IsPrivate.Should().Be(isPrivate);
    }

    [Fact]
    public void CreatedAt_ShouldBeBeforeLastActivityAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var lastActivityAt = DateTime.UtcNow.AddHours(-1);

        // Act
        var projectInfo = TestData.ProjectInfo()
            .WithCreatedAt(createdAt)
            .WithLastActivityAt(lastActivityAt)
            .Build();

        // Assert
        projectInfo.CreatedAt.Should().BeBefore(projectInfo.LastActivityAt);
    }
}
