using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="LyricsErrors"/>.
/// </summary>
public class LyricsErrorsTests
{
    private readonly LyricsErrors _errors = TestErrorsFactory.CreateLyricsErrors();
    private readonly LyricsErrorMessage _message = LocalizerFactory.CreateMessage<LyricsErrorMessage>("en");

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyExists_WithSongTitleAndArtistName_ShouldReturnConflictException()
    {
        // Arrange
        const string songTitle = "Eloko Oyo";
        const string artistName = "Fally Ipupa";

        // Act
        ConflictException exception = _errors.AlreadyExists(songTitle, artistName);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyExists(songTitle, artistName));
    }

    [Fact]
    public void SongTitleRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.SongTitleRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.SongTitleRequired());
    }

    [Fact]
    public void ArtistNameRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.ArtistNameRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.ArtistNameRequired());
    }

    [Fact]
    public void LyricsTextRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.LyricsTextRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.LyricsTextRequired());
    }
}
