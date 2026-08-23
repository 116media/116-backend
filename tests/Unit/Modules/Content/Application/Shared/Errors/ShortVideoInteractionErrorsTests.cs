using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ShortVideoInteractionErrors"/> factory methods.
/// </summary>
public class ShortVideoInteractionErrorsTests
{
    private readonly ShortVideoInteractionErrors _errors = TestErrorsFactory.CreateShortVideoInteractionErrors();
    private readonly ShortVideoInteractionErrorMessage _message =
        LocalizerFactory.CreateMessage<ShortVideoInteractionErrorMessage>();

    [Fact]
    public void AlreadyLiked_ShouldReturnConflictException()
    {
        ConflictException ex = _errors.AlreadyLiked();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.AlreadyLiked());
    }

    [Fact]
    public void LikeNotFound_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.LikeNotFound();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.LikeNotFound());
    }

    [Fact]
    public void AlreadyBookmarked_ShouldReturnConflictException()
    {
        ConflictException ex = _errors.AlreadyBookmarked();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.AlreadyBookmarked());
    }

    [Fact]
    public void BookmarkNotFound_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.BookmarkNotFound();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.BookmarkNotFound());
    }

    [Fact]
    public void Msg_Localizer_AlreadyLiked_ShouldReturnLocalizedString()
    {
        _errors.Msg.Localizer["AlreadyLiked"].Value.Should().Be(_message.AlreadyLiked());
    }
}
