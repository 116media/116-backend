using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ShortVideoInteractionErrorMessage"/>.
/// </summary>
public class ShortVideoInteractionErrorMessageTests
{
    private readonly ShortVideoInteractionErrorMessage _message =
        LocalizerFactory.CreateMessage<ShortVideoInteractionErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void AlreadyLiked_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.AlreadyLiked();

        // Assert
        message.Should().Be("You have already liked this short video.");
    }

    [Fact]
    public void LikeNotFound_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.LikeNotFound();

        // Assert
        message.Should().Be("Like not found for this short video.");
    }

    [Fact]
    public void AlreadyBookmarked_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.AlreadyBookmarked();

        // Assert
        message.Should().Be("You have already bookmarked this short video.");
    }

    [Fact]
    public void BookmarkNotFound_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.BookmarkNotFound();

        // Assert
        message.Should().Be("Bookmark not found for this short video.");
    }
}
