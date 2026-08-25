using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetArtistBySlugHandler"/>.
/// </summary>
public class PublicGetArtistBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly PublicGetArtistBySlugHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetArtistBySlugHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetArtistBySlugHandler(
            _artistRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object,
            Mapper,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenArtistFoundBySlug_ShouldReturnArtistPage()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);

        var query = new PublicGetArtistBySlugQuery(
            "fally-ipupa",
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetArtistBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artist.Id.Should().Be(artist.Id);
        result.Artist.Slug.Should().Be("fally-ipupa");
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _artistRepositoryMock.SetupGetBySlug("non-existent-slug", null);
        var query = new PublicGetArtistBySlugQuery(
            "non-existent-slug",
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThreadPaginationToBothLyricsAndVideosCalls()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);

        var query = new PublicGetArtistBySlugQuery(
            "fally-ipupa",
            new PaginatedRequest(2, 5),
            new PaginatedRequest(1, 20)
        );

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x => x.GetPublishedByArtistAsync(artist.Id, 3, 5, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _videoRepositoryMock.Verify(
            x => x.GetPublishedByArtistAsync(artist.Id, 2, 20, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedLyricsAndVideos()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);

        List<LyricsEntity> lyrics = LyricsFactory.CreateManyPublished(CategoryId, 2);
        List<VideoEntity> videos = VideoFactory.CreateManyPublished(CategoryId, 3);

        _lyricsRepositoryMock
            .Setup(x =>
                x.GetPublishedByArtistAsync(artist.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((lyrics, 2));
        _videoRepositoryMock
            .Setup(x =>
                x.GetPublishedByArtistAsync(artist.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((videos, 3));

        var query = new PublicGetArtistBySlugQuery(
            "fally-ipupa",
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetArtistBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Items.Should().HaveCount(2);
        result.Lyrics.Count.Should().Be(2);
        result.Videos.Items.Should().HaveCount(3);
        result.Videos.Count.Should().Be(3);
    }

    #region Zero-Content 404 Rule

    [Fact]
    public async Task Handle_WhenEveryTotalIsZero_ShouldThrowNotFound()
    {
        // Arrange — a bio and an avatar are not content; a stub must not become a page.
        ArtistEntity artist = ArtistFactory.CreateWithSlug("empty-artist");
        _artistRepositoryMock.SetupGetBySlug("empty-artist", artist);
        _artistRepositoryMock.SetupGetTotals(new ArtistTotals(0, 0, 0, 0, 0));

        var query = new PublicGetArtistBySlugQuery(
            "empty-artist",
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(1, 0, 0, 0, 0)]
    [InlineData(0, 1, 0, 0, 0)]
    [InlineData(0, 0, 1, 0, 0)]
    [InlineData(0, 0, 0, 1, 0)]
    [InlineData(0, 0, 0, 0, 1)]
    public async Task Handle_WhenAnySingleSurfaceHasContent_ShouldReturnProfile(
        int songs,
        int videos,
        int albums,
        int mixtapes,
        int news
    )
    {
        // Arrange — each of the five surfaces alone keeps the profile alive.
        ArtistEntity artist = ArtistFactory.CreateWithSlug("one-surface");
        _artistRepositoryMock.SetupGetBySlug("one-surface", artist);
        _artistRepositoryMock.SetupGetTotals(new ArtistTotals(songs, videos, albums, mixtapes, news));

        var query = new PublicGetArtistBySlugQuery(
            "one-surface",
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetArtistBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Totals.Should().Be(new ArtistTotalsDto(songs, videos, albums, mixtapes, news));
    }

    [Fact]
    public async Task Handle_ShouldExposeVerificationWithoutTheClaimingUser()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateClaimed(Guid.NewGuid());
        _artistRepositoryMock.SetupGetBySlug(artist.Slug, artist);

        var query = new PublicGetArtistBySlugQuery(
            artist.Slug,
            new PaginatedRequest(0, 10),
            new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetArtistBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert — the derived flag ships; the claiming user's identity never does.
        result.Artist.IsVerified.Should().BeTrue();
        result.Artist.GetType().GetProperty(nameof(ArtistEntity.UserId)).Should().BeNull();
    }

    #endregion
}
