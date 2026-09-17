using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
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
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadShortVideoThumbnailHandler _handler;

    public AdminUploadShortVideoThumbnailHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        _handler = new AdminUploadShortVideoThumbnailHandler(
            _shortVideoRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);

        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasExistingThumbnail_ShouldOverwriteInPlaceWithoutDelete()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithThumbnail();
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.ThumbnailFileId.Should().Be(uploadedFile.Id);
        result.ThumbnailUrl.Should().Be(uploadedFile.StorageUrl);
        result.ThumbnailStorageKey.Should().Be(uploadedFile.StorageKey);

        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyExecutedInTransaction();
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
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
