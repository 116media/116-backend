using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ArticleInteractionErrors"/> factory methods.
/// </summary>
public class ArticleInteractionErrorsTests
{
    private readonly ArticleInteractionErrors _errors = TestErrorsFactory.CreateArticleInteractionErrors();
    private readonly ArticleInteractionErrorMessage _message =
        LocalizerFactory.CreateMessage<ArticleInteractionErrorMessage>();

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
    public void CommentNotFound_WithId_ShouldReturnNotFoundException()
    {
        var commentId = Guid.NewGuid();

        NotFoundException ex = _errors.CommentNotFound(commentId);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void NotCommentOwner_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.NotCommentOwner();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.NotCommentOwner());
    }

    [Fact]
    public void CannotReplyToReply_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.CannotReplyToReply();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.CannotReplyToReply());
    }
}
