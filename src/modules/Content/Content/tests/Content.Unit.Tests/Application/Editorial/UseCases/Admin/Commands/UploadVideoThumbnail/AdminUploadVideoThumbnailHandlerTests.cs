using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadVideoThumbnailHandler"/>.
/// </summary>
public class AdminUploadVideoThumbnailHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadVideoThumbnailHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadVideoThumbnailHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        _handler = new AdminUploadVideoThumbnailHandler(
            _videoRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldOverwriteInPlaceWithoutDelete()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithThumbnail(CategoryId);
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);
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
