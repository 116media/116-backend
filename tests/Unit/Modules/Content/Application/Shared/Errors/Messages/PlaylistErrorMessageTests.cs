using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="PlaylistErrorMessage"/>.
/// </summary>
public class PlaylistErrorMessageTests
{
    private readonly PlaylistErrorMessage _message = LocalizerFactory.CreateMessage<PlaylistErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnFormattedMessage()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        string message = _message.NotFound(id);

        // Assert
        message.Should().Be($"Playlist '{id}' not found.");
    }

    [Fact]
    public void NotOwner_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.NotOwner();

        // Assert
        message.Should().Be("You can only manage your own playlists.");
    }

    [Fact]
    public void VideoAlreadyInPlaylist_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.VideoAlreadyInPlaylist();

        // Assert
        message.Should().Be("This video is already in the playlist.");
    }

    [Fact]
    public void NameRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.NameRequired();

        // Assert
        message.Should().Be("Playlist name is required.");
    }

    [Fact]
    public void NameTooLong_WithMax_ShouldReturnFormattedMessage()
    {
        // Arrange
        int max = 100;

        // Act
        string message = _message.NameTooLong(max);

        // Assert
        message.Should().Be($"Playlist name must not exceed {max} characters.");
    }
}
