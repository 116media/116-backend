using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Unit tests for <see cref="AdminDeleteVideoHandler"/>.
/// </summary>
public class AdminDeleteVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly AdminDeleteVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminDeleteVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _fileServiceMock = MockFileService.Create();
        _handler = new AdminDeleteVideoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            _fileServiceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftVideoWithNoThumbnail_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var command = new AdminDeleteVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminDeleteVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyRemoveCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
        _fileServiceMock.VerifyDeleteFileNotCalled();
    }

    [Fact]
    public async Task Handle_WhenDraftVideoWithThumbnail_ShouldDeleteThumbnailAndVideo()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithThumbnail(CategoryId);
        var command = new AdminDeleteVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        FileEntity thumbnailFile = FileFactory.CreateWithStorageKey("test-storage-key");
        _fileRepositoryMock
            .Setup(x => x.GetByIdAsync(video.ThumbnailFileId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thumbnailFile);

        // Act
        AdminDeleteVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _fileServiceMock.VerifyDeleteFileCalled();
        _videoRepositoryMock.VerifyRemoveCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenRejectedVideo_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateRejected(CategoryId);
        var command = new AdminDeleteVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminDeleteVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyRemoveCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoIsPublished_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var command = new AdminDeleteVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
