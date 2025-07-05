using FluentAssertions;
using GitLabCli.Shared.Exceptions;

namespace GitLabCli.Shared.Tests.Exceptions;

/// <summary>
/// GitOperationException 測試
/// </summary>
public class GitOperationExceptionTests
{
    [Fact]
    public void Constructor_WithOperationTypeAndMessage_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string operationType = "push";
        const string message = "Git push 失敗";

        // Act
        var exception = new GitOperationException(operationType, message);

        // Assert
        exception.OperationType.Should().Be(operationType);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithOperationTypeMessageAndInnerException_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string operationType = "pull";
        const string message = "Git pull 失敗";
        var innerException = new InvalidOperationException("內部錯誤");

        // Act
        var exception = new GitOperationException(operationType, message, innerException);

        // Assert
        exception.OperationType.Should().Be(operationType);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Theory]
    [InlineData("push", "Git push 失敗")]
    [InlineData("pull", "Git pull 發生衝突")]
    [InlineData("clone", "無法存取 Git 儲存庫")]
    [InlineData("commit", "分支不存在")]
    public void Constructor_WithVariousOperationTypes_ShouldSetCorrectProperties(string operationType, string message)
    {
        // Act
        var exception = new GitOperationException(operationType, message);

        // Assert
        exception.OperationType.Should().Be(operationType);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new GitOperationException("test", "測試例外");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ToString_ShouldIncludeOperationTypeAndMessage()
    {
        // Arrange
        const string operationType = "push";
        const string message = "Git push 失敗";
        var exception = new GitOperationException(operationType, message);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain(operationType);
        result.Should().Contain(message);
        result.Should().Contain(nameof(GitOperationException));
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldPreserveInnerExceptionDetails()
    {
        // Arrange
        const string innerMessage = "原始錯誤";
        var innerException = new ArgumentException(innerMessage);
        const string operationType = "merge";
        const string outerMessage = "Git merge 包裝錯誤";

        // Act
        var exception = new GitOperationException(operationType, outerMessage, innerException);

        // Assert
        exception.InnerException.Should().NotBeNull();
        exception.InnerException!.Message.Should().Be(innerMessage);
        exception.InnerException.Should().BeOfType<ArgumentException>();
        exception.OperationType.Should().Be(operationType);
    }
}
