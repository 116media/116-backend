using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Unit tests for <see cref="AdminGetVideoByIdHandler"/>.
/// </summary>
public class AdminGetVideoByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly AdminGetVideoByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetVideoByIdHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _handler = new AdminGetVideoByIdHandler(_videoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenVideoExists_ShouldReturnVideoDetail()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        result.Video.Id.Should().Be(video.Id);
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetVideoByIdQuery(Id: nonExistentId);
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
