using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadVideoThumbnailHandler"/>.
/// </summary>
public class AdminUploadVideoThumbnailHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileUploadService> _fileUploadServiceMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadVideoThumbnailHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadVideoThumbnailHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileUploadServiceMock = MockFileUploadService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileEntity fileEntity = FileFactory.CreateImage();
        _fileUploadServiceMock.SetupUploadImage(fileEntity);

        _handler = new AdminUploadVideoThumbnailHandler(
            _videoRepositoryMock.Object,
            _fileUploadServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        FileEntity uploadedFile = FileFactory.CreateImage();
        _fileUploadServiceMock.SetupUploadImage(uploadedFile);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileUploadServiceMock.VerifyUploadImageCalled();
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldOverwriteInPlaceWithoutDelete()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithThumbnail(CategoryId);
        FileEntity uploadedFile = FileFactory.CreateImage();
        _fileUploadServiceMock.SetupUploadImage(uploadedFile);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileUploadServiceMock.VerifyUploadImageCalled();
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: nonExistentId.ToString(), File: fileMock);
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
