using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoThumbnailHandler"/>.
/// </summary>
public class AdminUploadShortVideoThumbnailHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadShortVideoThumbnailHandler _handler;

    public AdminUploadShortVideoThumbnailHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileEntity fileEntity = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        _handler = new AdminUploadShortVideoThumbnailHandler(
            _shortVideoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        FileEntity uploadedFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(uploadedFile);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasExistingThumbnail_ShouldOverwriteInPlaceWithoutDelete()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithThumbnail();
        FileEntity uploadedFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(uploadedFile);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: nonExistentId.ToString(), File: fileMock);
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
