using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Domain.Entities;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Unit tests for <see cref="AdminUploadLyricsCoverHandler"/>.
/// </summary>
public class AdminUploadLyricsCoverHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadLyricsCoverHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadLyricsCoverHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        _handler = new AdminUploadLyricsCoverHandler(
            _lyricsRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsHasNoExistingCover_ShouldUploadAndReturnUrls()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        FileReferenceDto uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(uploadedFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: lyrics.Id, File: fileMock);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUploadLyricsCoverResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.CoverImageFileId.Should().Be(uploadedFile.Id);
        result.CoverImageUrl.Should().Be(uploadedFile.StorageUrl);
        result.CoverImageStorageKey.Should().Be(uploadedFile.StorageKey);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenLyricsHasExistingCover_ShouldOverwriteInPlace()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.SetCoverImageFileId(Guid.NewGuid());
        FileReferenceDto replacementFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(replacementFile));
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: lyrics.Id, File: fileMock);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUploadLyricsCoverResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.CoverImageFileId.Should().Be(replacementFile.Id);
        result.CoverImageUrl.Should().Be(replacementFile.StorageUrl);
        result.CoverImageStorageKey.Should().Be(replacementFile.StorageKey);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: nonExistentId, File: fileMock);
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
