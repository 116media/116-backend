using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateLyricsResponse"/>.
/// </summary>
public class AdminCreateLyricsEndpointV1Tests
{
    [Fact]
    public void AdminCreateLyricsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        LyricsDto dto = CreateLyricsDto();

        // Act
        var response = new AdminCreateLyricsResponse(Lyrics: dto);

        // Assert
        response.Should().NotBeNull();
        response.Lyrics.Should().Be(dto);
    }

    [Fact]
    public void AdminCreateLyricsRequest_ShouldConstructCorrectly()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();

        // Act
        var request = new AdminCreateLyricsRequest(
            SongTitle: "Test Song",
            ArtistName: "Test Artist",
            LyricsText: "Test lyrics text",
            Language: "fr",
            VideoId: videoId
        );

        // Assert
        request.Should().NotBeNull();
        request.SongTitle.Should().Be("Test Song");
        request.ArtistName.Should().Be("Test Artist");
        request.LyricsText.Should().Be("Test lyrics text");
        request.Language.Should().Be("fr");
        request.VideoId.Should().Be(videoId);
    }

    [Fact]
    public void AdminCreateLyricsRequest_WithAllNullOptionalFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateLyricsRequest(
            SongTitle: "Test Song",
            ArtistName: "Test Artist",
            LyricsText: "Test lyrics text",
            Language: "en",
            VideoId: null
        );

        // Assert
        request.VideoId.Should().BeNull();
    }

    private static LyricsDto CreateLyricsDto() =>
        new(
            Id: Guid.NewGuid(),
            SongTitle: "Test",
            ArtistName: "Test",
            LyricsText: "Test",
            Language: "fr",
            VideoId: null,
            MetaTitle: null,
            MetaDescription: null,
            AuthorId: Guid.NewGuid().ToString()
        );
}
