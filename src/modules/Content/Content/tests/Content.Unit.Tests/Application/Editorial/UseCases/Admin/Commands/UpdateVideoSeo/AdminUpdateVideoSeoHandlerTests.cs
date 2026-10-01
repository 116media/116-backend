using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoSeoHandler"/>.
/// </summary>
public class AdminUpdateVideoSeoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminUpdateVideoSeoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateVideoSeoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminUpdateVideoSeoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            )
        );
    }

    [Fact]
    public async Task Handle_WhenVideoExists_ShouldUpdateSeoAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var command = new AdminUpdateVideoSeoCommand(
            Id: video.Id.ToString(),
            MetaTitle: "Updated SEO Title",
            MetaDescription: "Updated SEO Description"
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUpdateVideoSeoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.MetaTitle.Should().Be(command.MetaTitle);
        video.MetaDescription.Should().Be(command.MetaDescription);
        result.Video.Id.Should().Be(video.Id);
        result.Video.MetaTitle.Should().Be(command.MetaTitle);
        result.Video.MetaDescription.Should().Be(command.MetaDescription);
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
