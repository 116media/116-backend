using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="SubmissionErrors" /> factory methods and the
/// <see cref="SubmissionErrorMessage" /> localized strings backing them.
/// </summary>
public class SubmissionErrorsTests
{
    private readonly SubmissionErrors _errors = TestErrorsFactory.CreateSubmissionErrors();
    private readonly SubmissionErrorMessage _message = LocalizerFactory.CreateMessage<SubmissionErrorMessage>("en");

    #region SubmissionErrors

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        SubmissionErrorMessage msg = _errors.Msg;

        // Assert
        msg.Should().NotBeNull();
        msg.NotPending().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NotFound_ShouldReturnNotFoundExceptionCarryingTheIdentifier()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException ex = _errors.NotFound(id);

        // Assert
        ex.Should().BeOfType<NotFoundException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(id.ToString());
        ex.KeyValue.Should().Be(id);
    }

    [Fact]
    public void NotPending_ShouldReturnConflictExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        ConflictException ex = _errors.NotPending();

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.NotPending());
    }

    #endregion

    #region SubmissionErrorMessage

    [Fact]
    public void Localizer_ShouldBeUsableForValidationExtensions()
    {
        // Arrange & Act
        string resolved = _message.Localizer["NotPending"];

        // Assert
        _message.Localizer.Should().NotBeNull();
        resolved.Should().NotBeNullOrWhiteSpace().And.NotBe("NotPending");
    }

    [Fact]
    public void NotPendingMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.NotPending();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("NotPending");
    }

    #endregion
}
