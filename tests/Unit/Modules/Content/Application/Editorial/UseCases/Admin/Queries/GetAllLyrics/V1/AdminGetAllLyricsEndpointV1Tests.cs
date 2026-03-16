using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllLyricsResponse"/>.
/// </summary>
public class AdminGetAllLyricsEndpointV1Tests
{
    [Fact]
    public void AdminGetAllLyricsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<LyricsDto>(1, 10, 1, [CreateLyricsDto()]);

        // Act
        var response = new AdminGetAllLyricsResponse(Lyrics: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Lyrics.Should().Be(paginated);
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
            MetaKeywords: null,
            CreatedAt: null,
            UpdatedAt: null
        );
}
