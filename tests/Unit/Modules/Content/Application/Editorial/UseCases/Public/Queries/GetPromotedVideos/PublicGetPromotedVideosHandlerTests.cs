using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedVideosHandler"/>.
/// </summary>
public class PublicGetPromotedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPromotedVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPromotedVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetPromotedVideosHandler(_videoRepositoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenPromotedVideosExist_ShouldReturnVideoList()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> promoted = VideoFactory.CreateManyWithCategory(CategoryId, category, 2);
        var query = new PublicGetPromotedVideosQuery();

        _videoRepositoryMock.SetupGetPromotedAsync(promoted);

        // Act
        PublicGetPromotedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Count.Should().Be(promoted.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPromotedVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetPromotedVideosQuery();

        _videoRepositoryMock.SetupGetPromotedAsync(new List<VideoEntity>());

        // Act
        PublicGetPromotedVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }
}
