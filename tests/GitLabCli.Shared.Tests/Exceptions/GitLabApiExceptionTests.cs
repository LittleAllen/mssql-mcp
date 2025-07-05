using FluentAssertions;
using GitLabCli.Shared.Exceptions;

namespace GitLabCli.Shared.Tests.Exceptions;

/// <summary>
/// GitLabApiException 測試
/// </summary>
public class GitLabApiExceptionTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const int statusCode = 404;
        const string errorCode = "NOT_FOUND";
        const string message = "專案不存在";

        // Act
        var exception = new GitLabApiException(statusCode, errorCode, message);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.ErrorCode.Should().Be(errorCode);
        exception.Message.Should().Be(message);
    }

    [Theory]
    [InlineData(400, "BAD_REQUEST", "錯誤的請求")]
    [InlineData(401, "UNAUTHORIZED", "未授權")]
    [InlineData(403, "FORBIDDEN", "禁止存取")]
    [InlineData(404, "NOT_FOUND", "找不到資源")]
    [InlineData(500, "INTERNAL_ERROR", "內部伺服器錯誤")]
    public void Constructor_WithVariousStatusCodes_ShouldSetCorrectValues(int statusCode, string errorCode, string message)
    {
        // Act
        var exception = new GitLabApiException(statusCode, errorCode, message);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.ErrorCode.Should().Be(errorCode);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new GitLabApiException(400, "TEST", "測試例外");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ToString_ShouldIncludeAllInformation()
    {
        // Arrange
        const int statusCode = 404;
        const string errorCode = "NOT_FOUND";
        const string message = "專案不存在";
        var exception = new GitLabApiException(statusCode, errorCode, message);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain(message);
        result.Should().Contain("GitLabApiException");
        
        // 檢查屬性可以正確存取
        exception.StatusCode.Should().Be(statusCode);
        exception.ErrorCode.Should().Be(errorCode);
        exception.Message.Should().Be(message);
    }
}
