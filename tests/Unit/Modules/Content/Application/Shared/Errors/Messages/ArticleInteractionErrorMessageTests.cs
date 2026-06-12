using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ArticleInteractionErrorMessage"/>.
/// </summary>
public class ArticleInteractionErrorMessageTests
{
    private readonly ArticleInteractionErrorMessage _message =
        LocalizerFactory.CreateMessage<ArticleInteractionErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void AlreadyLiked_ShouldReturnCorrectMessage()
    {
        string message = _message.AlreadyLiked();

        message.Should().Be("You have already liked this article.");
    }

    [Fact]
    public void LikeNotFound_ShouldReturnCorrectMessage()
    {
        string message = _message.LikeNotFound();

        message.Should().Be("Like not found for this article.");
    }

    [Fact]
    public void AlreadyBookmarked_ShouldReturnCorrectMessage()
    {
        string message = _message.AlreadyBookmarked();

        message.Should().Be("You have already bookmarked this article.");
    }

    [Fact]
    public void BookmarkNotFound_ShouldReturnCorrectMessage()
    {
        string message = _message.BookmarkNotFound();

        message.Should().Be("Bookmark not found for this article.");
    }

    [Fact]
    public void CommentNotFound_WithCommentId_ShouldReturnFormattedMessage()
    {
        Guid commentId = Guid.NewGuid();

        string message = _message.CommentNotFound(commentId);

        message.Should().Be($"Comment '{commentId}' not found.");
    }

    [Fact]
    public void NotCommentOwner_ShouldReturnCorrectMessage()
    {
        string message = _message.NotCommentOwner();

        message.Should().Be("You can only modify your own comments.");
    }

    [Fact]
    public void CommentBodyRequired_ShouldReturnCorrectMessage()
    {
        string message = _message.CommentBodyRequired();

        message.Should().Be("Comment body is required.");
    }

    [Fact]
    public void CommentBodyTooLong_WithMax_ShouldReturnFormattedMessage()
    {
        int max = 500;

        string message = _message.CommentBodyTooLong(max);

        message.Should().Be($"Comment body must not exceed {max} characters.");
    }

    [Fact]
    public void InvalidStarRating_ShouldReturnCorrectMessage()
    {
        string message = _message.InvalidStarRating();

        message.Should().Be("Star rating must be between 1 and 5.");
    }
}
