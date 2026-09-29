using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;
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
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoFileHandler"/>.
/// </summary>
public class AdminUploadShortVideoFileHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadShortVideoFileHandler _handler;

    public AdminUploadShortVideoFileHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateVideo();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        _handler = new AdminUploadShortVideoFileHandler(
            _shortVideoRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenDraftHasNoVideoFile_ShouldUploadAndAttachFile()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateDraft();
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateVideo();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoFileResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.VideoFileId.Should().Be(uploadedFile.Id);
        result.VideoUrl.Should().Be(uploadedFile.StorageUrl);
        result.VideoStorageKey.Should().Be(uploadedFile.StorageKey);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenShortVideoAlreadyHasFile_ShouldReplaceIt()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateVideo();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoFileResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.VideoFileId.Should().Be(uploadedFile.Id);
        result.VideoUrl.Should().Be(uploadedFile.StorageUrl);
        result.VideoStorageKey.Should().Be(uploadedFile.StorageKey);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: nonExistentId.ToString(), File: fileMock);
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
