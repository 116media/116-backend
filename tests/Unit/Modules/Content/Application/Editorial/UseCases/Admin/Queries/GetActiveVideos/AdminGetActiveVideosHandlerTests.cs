using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Unit tests for <see cref="AdminGetActiveVideosHandler"/>.
/// </summary>
public class AdminGetActiveVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetActiveVideosHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetActiveVideosHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetActiveVideosHandler(_videoRepositoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenActiveVideosExist_ShouldReturnAllActiveVideos()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(CategoryId, category, 3);
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(videos);

        // Act
        AdminGetActiveVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenNoActiveVideosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(new List<VideoEntity>());

        // Act
        AdminGetActiveVideosResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Videos.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryGetActiveAsync()
    {
        // Arrange
        var query = new AdminGetActiveVideosQuery();

        _videoRepositoryMock.SetupGetActiveAsync(new List<VideoEntity>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
