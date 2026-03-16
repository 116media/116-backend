using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsBySlugHandler"/>.
/// </summary>
public class PublicGetLyricsBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly PublicGetLyricsBySlugHandler _handler;

    public PublicGetLyricsBySlugHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _handler = new PublicGetLyricsBySlugHandler(_lyricsRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenLyricsExist_ShouldReturnLyrics()
    {
        // Arrange
        string songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
        string artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
        LyricsEntity lyrics = LyricsFactory.Create(songTitle, artistName);
        var query = new PublicGetLyricsBySlugQuery(SongTitle: songTitle, ArtistName: artistName);

        _lyricsRepositoryMock.SetupGetBySongTitleAndArtist(songTitle, artistName, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string songTitle = "nonexistent-song";
        string artistName = "nonexistent-artist";
        var query = new PublicGetLyricsBySlugQuery(SongTitle: songTitle, ArtistName: artistName);

        _lyricsRepositoryMock.SetupGetBySongTitleAndArtist(songTitle, artistName, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
