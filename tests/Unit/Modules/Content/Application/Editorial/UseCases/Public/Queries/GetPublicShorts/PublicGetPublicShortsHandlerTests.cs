using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Unit tests for <see cref="PublicGetPublicShortsHandler"/>.
/// </summary>
public class PublicGetPublicShortsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly PublicGetPublicShortsHandler _handler;

    public PublicGetPublicShortsHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _handler = new PublicGetPublicShortsHandler(_shortVideoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenActiveShortVideosExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(3);
        var query = new PublicGetPublicShortsQuery(PaginatedRequest: new PaginatedRequest(0, 10), Search: null);

        _shortVideoRepositoryMock.SetupGetAllAsync(shorts, shorts.Count);

        // Act
        PublicGetPublicShortsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideos.Items.Count().Should().Be(shorts.Count);
        result.ShortVideos.Count.Should().Be((long)shorts.Count);
    }

    [Fact]
    public async Task Handle_WhenNoActiveShortVideosExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new PublicGetPublicShortsQuery(PaginatedRequest: new PaginatedRequest(0, 10), Search: null);

        _shortVideoRepositoryMock.SetupGetAllAsync(new List<ShortVideoEntity>(), 0);

        // Act
        PublicGetPublicShortsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideos.Items.Should().BeEmpty();
        result.ShortVideos.Count.Should().Be(0);
    }
}
