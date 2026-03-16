using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetFeaturedVideosHandler"/>.
/// </summary>
public class PublicGetFeaturedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly PublicGetFeaturedVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetFeaturedVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _handler = new PublicGetFeaturedVideosHandler(_videoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenFeaturedVideosExist_ShouldReturnVideoList()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> featured = VideoFactory.CreateManyWithCategory(CategoryId, category, 2);
        var query = new PublicGetFeaturedVideosQuery();

        _videoRepositoryMock.SetupGetFeaturedAsync(featured);

        // Act
        PublicGetFeaturedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Videos.Should().NotBeNull();
        result.Videos.Count.Should().Be(featured.Count);
    }

    [Fact]
    public async Task Handle_WhenNoFeaturedVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetFeaturedVideosQuery();

        _videoRepositoryMock.SetupGetFeaturedAsync(new List<VideoEntity>());

        // Act
        PublicGetFeaturedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }
}
