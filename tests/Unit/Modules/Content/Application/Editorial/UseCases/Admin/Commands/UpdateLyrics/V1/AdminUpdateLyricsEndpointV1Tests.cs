using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsResponse"/>.
/// </summary>
public class AdminUpdateLyricsEndpointV1Tests
{
    [Fact]
    public void AdminUpdateLyricsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        LyricsDto dto = CreateLyricsDto();

        // Act
        var response = new AdminUpdateLyricsResponse(Lyrics: dto);

        // Assert
        response.Should().NotBeNull();
        response.Lyrics.Should().Be(dto);
    }

    [Fact]
    public void AdminUpdateLyricsRequest_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminUpdateLyricsRequest(
            SongTitle: "Eloko Oyo",
            ArtistName: "Fally Ipupa",
            LyricsText: "Eloko oyo...",
            Language: "fr",
            VideoId: null
        );

        // Assert
        request.Should().NotBeNull();
        request.SongTitle.Should().Be("Eloko Oyo");
        request.ArtistName.Should().Be("Fally Ipupa");
        request.LyricsText.Should().Be("Eloko oyo...");
        request.Language.Should().Be("fr");
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
