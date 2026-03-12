using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="LyricsErrors"/>.
/// </summary>
public class LyricsErrorsTests
{
    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = LyricsErrors.NotFound(id);

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
        ConflictException exception = LyricsErrors.AlreadyExists(songTitle, artistName);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SongTitleRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = LyricsErrors.SongTitleRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ArtistNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = LyricsErrors.ArtistNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LyricsTextRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = LyricsErrors.LyricsTextRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
