using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

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

        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
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

        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
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
