using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularVideosHandler"/>. Caching lives in the
/// CQRS caching decorator, so these cover the projection only; the cache-key contract
/// the decorator relies on is asserted on the query record.
/// </summary>
public class PublicGetPopularVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetPopularVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPopularVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto thumbnailFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupResolve(thumbnailFile);
        _handler = new PublicGetPopularVideosHandler(
            _videoRepositoryMock.Object,
            new VideoDtoFactory(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupFactory()
            )
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
    public void CacheKey_WithSameArguments_ShouldBeStable()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var first = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: categoryId, ExcludeId: null);
        var second = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: categoryId, ExcludeId: null);

        // Assert
        first.CacheKey.Should().Be(second.CacheKey);
    }

    [Fact]
    public void CacheKey_WithDifferentArguments_ShouldDiffer()
    {
        // Arrange
        var baseline = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Assert — every parameter participates in the key
        new PublicGetPopularVideosQuery(Limit: 7, CategoryId: null, ExcludeId: null)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularVideosQuery(Limit: 5, CategoryId: Guid.NewGuid(), ExcludeId: null)
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
        new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: Guid.NewGuid())
            .CacheKey.Should()
            .NotBe(baseline.CacheKey);
    }

    [Fact]
    public void CacheTags_ShouldCarryThePopularVideosTag()
    {
        // Arrange
        var query = new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null);

        // Assert
        query.CacheTags.Should().ContainSingle().Which.Should().Be(ContentCacheTags.PopularVideos);
    }
}
