using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="TranslationErrors" /> factory methods and the
/// <see cref="TranslationErrorMessage" /> localized strings backing them.
/// </summary>
public class TranslationErrorsTests
{
    private readonly TranslationErrors _errors = TestErrorsFactory.CreateTranslationErrors();
    private readonly TranslationErrorMessage _message = LocalizerFactory.CreateMessage<TranslationErrorMessage>();

    #region TranslationErrors

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        TranslationErrorMessage msg = _errors.Msg;

        // Assert
        msg.Should().NotBeNull();
        msg.AlreadyVoted().Should().NotBeNullOrWhiteSpace();
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
    public void RevisionNotFound_ShouldReturnNotFoundExceptionCarryingTheIdentifier()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException ex = _errors.RevisionNotFound(id);

        // Assert
        ex.Should().BeOfType<NotFoundException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(id.ToString());
        ex.EntityName.Should().Be("Translation revision");
    }

    [Fact]
    public void AlreadyVoted_ShouldReturnConflictExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        ConflictException ex = _errors.AlreadyVoted();

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.AlreadyVoted());
    }

    [Fact]
    public void ProposedTextRequired_ShouldReturnBadRequestExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        BadRequestException ex = _errors.ProposedTextRequired();

        // Assert
        ex.Should().BeOfType<BadRequestException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.ProposedTextRequired());
    }

    #endregion

    #region TranslationErrorMessage

    [Fact]
    public void Localizer_ShouldBeUsableForValidationExtensions()
    {
        // Arrange & Act
        string resolved = _message.Localizer["AlreadyVoted"];

        // Assert
        _message.Localizer.Should().NotBeNull();
        resolved.Should().NotBeNullOrWhiteSpace().And.NotBe("AlreadyVoted");
    }

    [Fact]
    public void AlreadyVotedMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.AlreadyVoted();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("AlreadyVoted");
    }

    [Fact]
    public void ProposedTextRequiredMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.ProposedTextRequired();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("ProposedTextRequired");
    }

    #endregion
}
