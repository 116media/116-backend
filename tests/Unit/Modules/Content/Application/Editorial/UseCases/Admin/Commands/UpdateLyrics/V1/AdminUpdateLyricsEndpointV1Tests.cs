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

    private static LyricsDto CreateLyricsDto() =>
        new(
            Id: Guid.NewGuid(),
            SongTitle: "Test",
            ArtistName: "Test",
            LyricsText: "Test",
            Language: "fr",
            VideoId: null,
            ArticleId: null,
            MetaTitle: null,
            MetaDescription: null,
            MetaKeywords: null
        );
}
