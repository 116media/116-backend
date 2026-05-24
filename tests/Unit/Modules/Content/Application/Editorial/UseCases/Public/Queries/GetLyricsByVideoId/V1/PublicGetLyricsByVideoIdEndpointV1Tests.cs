using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsByVideoIdResponse"/>.
/// </summary>
public class PublicGetLyricsByVideoIdEndpointV1Tests
{
    [Fact]
    public void PublicGetLyricsByVideoIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        LyricsDto dto = CreateLyricsDto();

        // Act
        var response = new PublicGetLyricsByVideoIdResponse(Lyrics: dto);

        // Assert
        response.Should().NotBeNull();
        response.Lyrics.Should().Be(dto);
    }

    private static LyricsDto CreateLyricsDto() =>
        new(
            Id: Guid.NewGuid(),
            SongTitle: "Test",
            ArtistName: "Test",
            LyricsText: "Test",
            Language: "fr",
            VideoId: Guid.NewGuid(),
            MetaTitle: null,
            MetaDescription: null,
            AuthorId: Guid.NewGuid().ToString()
        );
}
