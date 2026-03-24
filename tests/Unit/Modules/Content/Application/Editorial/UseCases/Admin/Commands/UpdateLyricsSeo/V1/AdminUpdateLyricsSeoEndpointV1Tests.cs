using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsSeoResponse"/>.
/// </summary>
public class AdminUpdateLyricsSeoEndpointV1Tests
{
    [Fact]
    public void AdminUpdateLyricsSeoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        LyricsDto dto = CreateLyricsDto();

        // Act
        var response = new AdminUpdateLyricsSeoResponse(Lyrics: dto);

        // Assert
        response.Should().NotBeNull();
        response.Lyrics.Should().Be(dto);
    }

    [Fact]
    public void AdminUpdateLyricsSeoRequest_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminUpdateLyricsSeoRequest(
            MetaTitle: "SEO Title",
            MetaDescription: "SEO Description",
            MetaKeywords: "keyword1, keyword2",
            StructuredData: "{\"@type\": \"MusicRecording\"}"
        );

        // Assert
        request.Should().NotBeNull();
        request.MetaTitle.Should().Be("SEO Title");
        request.MetaDescription.Should().Be("SEO Description");
        request.MetaKeywords.Should().Be("keyword1, keyword2");
        request.StructuredData.Should().Be("{\"@type\": \"MusicRecording\"}");
    }

    [Fact]
    public void AdminUpdateLyricsSeoRequest_WithAllNullFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminUpdateLyricsSeoRequest(
            MetaTitle: null,
            MetaDescription: null,
            MetaKeywords: null,
            StructuredData: null
        );

        // Assert
        request.MetaTitle.Should().BeNull();
        request.MetaDescription.Should().BeNull();
        request.MetaKeywords.Should().BeNull();
        request.StructuredData.Should().BeNull();
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
