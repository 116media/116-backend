using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularVideosHandler"/>.
/// </summary>
public class PublicGetPopularVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IPopularVideosCacheInvalidator> _cacheInvalidatorMock;
    private readonly IMemoryCache _cache;
    private readonly PublicGetPopularVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPopularVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _cacheInvalidatorMock = MockPopularVideosCacheInvalidator.Create();
        _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        FileEntity thumbnailFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(thumbnailFile);
        _handler = new PublicGetPopularVideosHandler(
            _videoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _cache,
            _cacheInvalidatorMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenPopularVideosExist_ShouldReturnMappedList()
    {
        // Arrange
        List<VideoEntity> videos = VideoFactory.CreateManyPublished(CategoryId, 3);
        _videoRepositoryMock.SetupGetPopularVideosAsync(videos);

        var query = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        PublicGetPopularVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Videos.Should().NotBeNull();
        result.Videos.Count.Should().Be(videos.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPopularVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        _videoRepositoryMock.SetupGetPopularVideosAsync(new List<VideoEntity>());

        var query = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        PublicGetPopularVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassArgumentsToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var excludeId = Guid.NewGuid();
        _videoRepositoryMock.SetupGetPopularVideosAsync(VideoFactory.CreateManyPublished(CategoryId, 1));

        var query = new PublicGetPopularVideosQuery(Limit: 7, CategoryId: categoryId, ExcludeId: excludeId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(
            x => x.GetPopularVideosAsync(7, categoryId, excludeId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameArgs_ShouldHitRepositoryOnce()
    {
        // Arrange
        _videoRepositoryMock.SetupGetPopularVideosAsync(VideoFactory.CreateManyPublished(CategoryId, 3));
        var query = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Act
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(
            x => x.GetPopularVideosAsync(5, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CalledWithDifferentExcludeId_ShouldHitRepositoryTwice()
    {
        // Arrange
        _videoRepositoryMock.SetupGetPopularVideosAsync(VideoFactory.CreateManyPublished(CategoryId, 3));

        // Act
        await _handler.Handle(
            new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: Guid.NewGuid()),
            CancellationToken.None
        );
        await _handler.Handle(
            new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: Guid.NewGuid()),
            CancellationToken.None
        );

        // Assert
        _videoRepositoryMock.Verify(
            x =>
                x.GetPopularVideosAsync(
                    It.IsAny<int>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallInvalidate()
    {
        // Arrange
        _videoRepositoryMock.SetupGetPopularVideosAsync(VideoFactory.CreateManyPublished(CategoryId, 3));

        // Act
        await _handler.Handle(
            new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null),
            CancellationToken.None
        );

        // Assert
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }
}
