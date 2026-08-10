using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedLyricsHandler"/>.
/// </summary>
public class PublicGetPublishedLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly PublicGetPublishedLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPublishedLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetPublishedLyricsHandler(_lyricsRepositoryMock.Object, fileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPublishedLyricsExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<LyricsEntity> lyricsList = LyricsFactory.CreateManyPublished(CategoryId, 3);
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(lyricsList, lyricsList.Count);

        // Act
        PublicGetPublishedLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Items.Should().HaveCount(lyricsList.Count);
        result.Lyrics.Count.Should().Be((long)lyricsList.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPublishedLyricsExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        PublicGetPublishedLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Items.Should().BeEmpty();
        result.Lyrics.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldMapPageIndexToOneBasedRepositoryPage()
    {
        // Arrange
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(2, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    3,
                    10,
                    It.IsAny<string?>(),
                    EnumContentStatus.Published,
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithSearchLanguageAndCategoryFilters_ShouldPassThemToRepository()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: "fally",
            Language: "fr",
            CategoryId: categoryId,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    1,
                    10,
                    "fally",
                    EnumContentStatus.Published,
                    categoryId,
                    "fr",
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldAlwaysFilterByPublishedStatus()
    {
        // Arrange
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    EnumContentStatus.Published,
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNoSortParam_ShouldPassNullSortToRepositoryForNewestDefault()
    {
        // Arrange
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    EnumContentStatus.Published,
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithExplicitNewestSort_ShouldPassSortValueToRepository()
    {
        // Arrange
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: "newest"
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    EnumContentStatus.Published,
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    "newest",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenCurrentUserLikedOneOfTheResults_ShouldStampIsLikedPerItem()
    {
        // Arrange
        LyricsEntity likedLyrics = LyricsFactory.CreatePublished(CategoryId);
        LyricsEntity notLikedLyrics = LyricsFactory.CreatePublished(CategoryId);
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null,
            CurrentUserId: Guid.NewGuid()
        );

        _lyricsRepositoryMock.SetupGetAllAsync([likedLyrics, notLikedLyrics], 2);
        _lyricsRepositoryMock.SetupGetLikedIdsAsync(new HashSet<Guid> { likedLyrics.Id });

        // Act
        PublicGetPublishedLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Items.Single(l => l.Id == likedLyrics.Id).IsLiked.Should().BeTrue();
        result.Lyrics.Items.Single(l => l.Id == notLikedLyrics.Id).IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnIsLikedFalseForAllItems()
    {
        // Arrange
        List<LyricsEntity> lyricsList = LyricsFactory.CreateManyPublished(CategoryId, 2);
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null,
            CurrentUserId: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(lyricsList, lyricsList.Count);

        // Act
        PublicGetPublishedLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Items.Should().OnlyContain(l => l.IsLiked == false);
    }

    [Fact]
    public async Task Handle_ShouldPassThroughViewLikeAndShareCounts()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        lyrics.IncrementViewCount();
        lyrics.IncrementViewCount();
        lyrics.IncrementViewCount();
        lyrics.IncrementLikeCount();
        lyrics.IncrementShareCount();
        var query = new PublicGetPublishedLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Language: null,
            CategoryId: null,
            Sort: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync([lyrics], 1);

        // Act
        PublicGetPublishedLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        LyricsSummaryDto dto = result.Lyrics.Items.Single();
        dto.ViewCount.Should().Be(3);
        dto.LikeCount.Should().Be(1);
        dto.ShareCount.Should().Be(1);
    }

    [Fact]
    public void Handle_SortSwitchGuardTests_ShouldNotReferenceIsPromoted()
    {
        // Arrange / Act / Assert — this is a documentation-style sanity check: none of the sort
        // guard tests above assert on IsPromoted, and PublicGetPublishedLyricsQuery does not
        // expose an IsPromoted filter parameter.
        typeof(PublicGetPublishedLyricsQuery).GetProperties().Select(p => p.Name).Should().NotContain("IsPromoted");
    }
}
