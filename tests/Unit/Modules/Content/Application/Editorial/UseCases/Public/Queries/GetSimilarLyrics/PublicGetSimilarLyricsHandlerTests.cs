using _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;

/// <summary>
/// Unit tests for <see cref="PublicGetSimilarLyricsHandler"/>.
/// </summary>
public class PublicGetSimilarLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly PublicGetSimilarLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetSimilarLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetSimilarLyricsHandler(_lyricsRepositoryMock.Object, fileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsMatches_ShouldMapToSummaryDtos()
    {
        // Arrange
        LyricsEntity source = LyricsFactory.CreatePublished(CategoryId);
        List<LyricsEntity> matches =
        [
            LyricsFactory.CreatePublished(CategoryId),
            LyricsFactory.CreatePublished(CategoryId),
        ];
        var query = new PublicGetSimilarLyricsQuery(LyricsId: source.Id);

        _lyricsRepositoryMock.SetupGetSimilarAsync(source.Id, matches);

        // Act
        PublicGetSimilarLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Should().HaveCount(2);
        result.Lyrics.Select(l => l.Id).Should().BeEquivalentTo(matches.Select(m => m.Id));
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNoMatches_ShouldReturnEmptyListNotThrow()
    {
        // Arrange
        LyricsEntity source = LyricsFactory.CreatePublished(CategoryId);
        var query = new PublicGetSimilarLyricsQuery(LyricsId: source.Id);

        _lyricsRepositoryMock.SetupGetSimilarAsync(source.Id, []);

        // Act
        PublicGetSimilarLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserLikedOneOfTheMatches_ShouldStampIsLikedPerItem()
    {
        // Arrange
        LyricsEntity source = LyricsFactory.CreatePublished(CategoryId);
        LyricsEntity likedMatch = LyricsFactory.CreatePublished(CategoryId);
        LyricsEntity notLikedMatch = LyricsFactory.CreatePublished(CategoryId);
        var query = new PublicGetSimilarLyricsQuery(LyricsId: source.Id, CurrentUserId: Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetSimilarAsync(source.Id, [likedMatch, notLikedMatch]);
        _lyricsRepositoryMock.SetupGetLikedIdsAsync(new HashSet<Guid> { likedMatch.Id });

        // Act
        PublicGetSimilarLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Single(l => l.Id == likedMatch.Id).IsLiked.Should().BeTrue();
        result.Lyrics.Single(l => l.Id == notLikedMatch.Id).IsLiked.Should().BeFalse();
    }
}
