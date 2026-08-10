using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="LyricsInteractionErrors" /> factory methods and the
/// <see cref="LyricsInteractionErrorMessage" /> localized strings backing them.
/// </summary>
public class LyricsInteractionErrorsTests
{
    private readonly LyricsInteractionErrors _errors = TestErrorsFactory.CreateLyricsInteractionErrors();
    private readonly LyricsInteractionErrorMessage _message =
        LocalizerFactory.CreateMessage<LyricsInteractionErrorMessage>();

    #region LyricsInteractionErrors

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        LyricsInteractionErrorMessage msg = _errors.Msg;

        // Assert
        msg.Should().NotBeNull();
        msg.AlreadyLiked().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AlreadyLiked_ShouldReturnConflictExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        ConflictException ex = _errors.AlreadyLiked();

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.AlreadyLiked());
    }

    [Fact]
    public void LikeNotFound_ShouldReturnBadRequestExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        BadRequestException ex = _errors.LikeNotFound();

        // Assert
        ex.Should().BeOfType<BadRequestException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.LikeNotFound());
    }

    #endregion

    #region LyricsInteractionErrorMessage

    [Fact]
    public void Localizer_ShouldBeUsableForValidationExtensions()
    {
        // Arrange & Act
        string resolved = _message.Localizer["AlreadyLiked"];

        // Assert
        _message.Localizer.Should().NotBeNull();
        resolved.Should().NotBeNullOrWhiteSpace().And.NotBe("AlreadyLiked");
    }

    [Fact]
    public void AlreadyLikedMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.AlreadyLiked();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("AlreadyLiked");
    }

    [Fact]
    public void LikeNotFoundMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.LikeNotFound();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("LikeNotFound");
    }

    #endregion
}
