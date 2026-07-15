using _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// Unit tests for <see cref="PublicGetShortsFeedHandler"/> covering cursor decoding, next-cursor
/// construction, per-user flag stamping, and seed reuse across pages.
/// </summary>
public class PublicGetShortsFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetShortsFeedHandler _handler;

    public PublicGetShortsFeedHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileRepositoryMock = MockFileRepository.Create();

        FileEntity videoFile = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupGetById(videoFile);

        _handler = new PublicGetShortsFeedHandler(
            _shortVideoRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenPageIsFull_ShouldReturnDecodableNextCursor()
    {
        // Arrange
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(2);
        var query = new PublicGetShortsFeedQuery(Cursor: null, PageSize: 2);

        long capturedSeed = 0;
        _shortVideoRepositoryMock.SetupCaptureRandomizedFeedArgs(shorts, (seed, _, _) => capturedSeed = seed);

        // Act
        PublicGetShortsFeedResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.NextCursor.Should().NotBeNull();

        ShortVideoFeedCursor.TryDecode(result.NextCursor, out ShortVideoFeedCursor cursor).Should().BeTrue();
        cursor.Seed.Should().Be(capturedSeed);
        cursor.AfterKey.Should().Be(shorts[^1].FeedRank ^ capturedSeed);
    }

    [Fact]
    public async Task Handle_WhenPageIsNotFull_ShouldReturnNullCursor()
    {
        // Arrange — one item for a page size of two: the feed is exhausted
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(1);
        var query = new PublicGetShortsFeedQuery(Cursor: null, PageSize: 2);

        _shortVideoRepositoryMock.SetupGetRandomizedFeedAsync(shorts);

        // Act
        PublicGetShortsFeedResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenFeedEmpty_ShouldReturnEmptyItemsAndNullCursor()
    {
        // Arrange
        var query = new PublicGetShortsFeedQuery(Cursor: null, PageSize: 5);

        _shortVideoRepositoryMock.SetupGetRandomizedFeedAsync(new List<ShortVideoEntity>());

        // Act
        PublicGetShortsFeedResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCursorProvided_ShouldReuseSeedAndKeyset()
    {
        // Arrange
        var incoming = new ShortVideoFeedCursor(Seed: 4242L, AfterKey: 987654321L);
        var query = new PublicGetShortsFeedQuery(Cursor: incoming.Encode(), PageSize: 3);

        long seedArg = 0;
        long? sortKeyArg = null;
        _shortVideoRepositoryMock.SetupCaptureRandomizedFeedArgs(
            ShortVideoFactory.CreateMany(1),
            (seed, sortKey, _) =>
            {
                seedArg = seed;
                sortKeyArg = sortKey;
            }
        );

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        seedArg.Should().Be(4242);
        sortKeyArg.Should().Be(987654321L);
    }

    [Fact]
    public async Task Handle_WhenUserLikedAndBookmarked_ShouldStampFlags()
    {
        // Arrange
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(2);
        ShortVideoEntity liked = shorts[0];
        var query = new PublicGetShortsFeedQuery(Cursor: null, PageSize: 5, CurrentUserId: Guid.NewGuid());

        _shortVideoRepositoryMock.SetupGetRandomizedFeedAsync(shorts);
        _shortVideoRepositoryMock.SetupGetLikedAndBookmarkedIdsAsync(
            new HashSet<Guid> { liked.Id },
            new HashSet<Guid> { liked.Id }
        );

        // Act
        PublicGetShortsFeedResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Single(dto => dto.Id == liked.Id).IsLiked.Should().BeTrue();
        result.Items.Single(dto => dto.Id == liked.Id).IsBookmarked.Should().BeTrue();
        result.Items.Single(dto => dto.Id == shorts[1].Id).IsLiked.Should().BeFalse();
    }
}
