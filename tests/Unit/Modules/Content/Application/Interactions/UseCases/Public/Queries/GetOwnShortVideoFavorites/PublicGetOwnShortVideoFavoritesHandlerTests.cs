using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnBookmarkedShortVideos;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedShortVideos;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedShortVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnShortVideoFavorites;

/// <summary>Unit tests for the short-video favorite query handlers.</summary>
public class PublicGetOwnShortVideoFavoritesHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly Mock<IShortVideoRepository> _repository = MockShortVideoRepository.Create();
    private readonly Mock<IFileRepository> _files = MockFileRepository.Create();

    public PublicGetOwnShortVideoFavoritesHandlerTests()
    {
        _files
            .Setup(repository =>
                repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Dictionary<Guid, FileEntity>());
    }

    [Fact]
    public async Task GetLiked_Handle_ReturnsTimestampCountPaginationAndLikedFlag()
    {
        DateTime interactedAt = DateTime.UtcNow.AddMinutes(-5);
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _repository
            .Setup(repository => repository.GetLikedShortVideosAsync(UserId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ShortVideoActivity> { new(shortVideo, interactedAt) }, 8));
        _repository.SetupGetLikedAndBookmarkedIdsAsync(new HashSet<Guid> { shortVideo.Id }, new HashSet<Guid>());
        var handler = new PublicGetOwnLikedShortVideosHandler(_repository.Object, _files.Object, Mapper);

        PublicGetOwnLikedShortVideosResult result = await handler.Handle(
            new PublicGetOwnLikedShortVideosQuery(UserId, new PaginatedRequest(1, 5)),
            CancellationToken.None
        );

        result.ShortVideos.PageIndex.Should().Be(1);
        result.ShortVideos.PageSize.Should().Be(5);
        result.ShortVideos.Count.Should().Be(8);
        result.ShortVideos.Items.Single().LastInteractedAt.Should().Be(interactedAt);
        result.ShortVideos.Items.Single().ShortVideo.IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task GetBookmarked_Handle_ReturnsBookmarkFlagAndInteractionTimestamp()
    {
        DateTime interactedAt = DateTime.UtcNow.AddDays(-1);
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _repository
            .Setup(repository => repository.GetBookmarkedShortVideosAsync(UserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ShortVideoActivity> { new(shortVideo, interactedAt) }, 1));
        _repository.SetupGetLikedAndBookmarkedIdsAsync(new HashSet<Guid>(), new HashSet<Guid> { shortVideo.Id });
        var handler = new PublicGetOwnBookmarkedShortVideosHandler(_repository.Object, _files.Object, Mapper);

        PublicGetOwnBookmarkedShortVideosResult result = await handler.Handle(
            new PublicGetOwnBookmarkedShortVideosQuery(UserId, new PaginatedRequest()),
            CancellationToken.None
        );

        result.ShortVideos.Items.Single().LastInteractedAt.Should().Be(interactedAt);
        result.ShortVideos.Items.Single().InteractionCount.Should().Be(1);
        result.ShortVideos.Items.Single().ShortVideo.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task GetShared_Handle_ReturnsOwnGroupedShareCount()
    {
        DateTime interactedAt = DateTime.UtcNow;
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _repository
            .Setup(repository => repository.GetSharedShortVideosAsync(UserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ShortVideoActivity> { new(shortVideo, interactedAt, 4) }, 1));
        var handler = new PublicGetOwnSharedShortVideosHandler(_repository.Object, _files.Object, Mapper);

        PublicGetOwnSharedShortVideosResult result = await handler.Handle(
            new PublicGetOwnSharedShortVideosQuery(UserId, new PaginatedRequest()),
            CancellationToken.None
        );

        result.ShortVideos.Items.Single().InteractionCount.Should().Be(4);
        result.ShortVideos.Items.Single().LastInteractedAt.Should().Be(interactedAt);
    }

    [Fact]
    public async Task Handlers_WhenRepositoryIsEmpty_ReturnEmptyPages()
    {
        _repository
            .Setup(repository => repository.GetLikedShortVideosAsync(UserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ShortVideoActivity>(), 0));
        var handler = new PublicGetOwnLikedShortVideosHandler(_repository.Object, _files.Object, Mapper);

        PublicGetOwnLikedShortVideosResult result = await handler.Handle(
            new PublicGetOwnLikedShortVideosQuery(UserId, new PaginatedRequest()),
            CancellationToken.None
        );

        result.ShortVideos.Count.Should().Be(0);
        result.ShortVideos.Items.Should().BeEmpty();
    }
}
