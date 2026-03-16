using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoSeoHandler"/>.
/// </summary>
public class AdminUpdateVideoSeoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateVideoSeoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateVideoSeoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateVideoSeoHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenVideoExists_ShouldUpdateSeoAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var command = new AdminUpdateVideoSeoCommand(
            Id: video.Id.ToString(),
            MetaTitle: "Updated SEO Title",
            MetaDescription: "Updated SEO Description"
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUpdateVideoSeoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateVideoSeoCommand(
            Id: nonExistentId.ToString(),
            MetaTitle: null,
            MetaDescription: null
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
