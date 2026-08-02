using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Unit tests for <see cref="AdminUploadLyricsCoverHandler"/>.
/// </summary>
public class AdminUploadLyricsCoverHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadLyricsCoverHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadLyricsCoverHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileEntity fileEntity = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        _handler = new AdminUploadLyricsCoverHandler(
            _lyricsRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsHasNoExistingCover_ShouldUploadAndReturnUrls()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: lyrics.Id, File: fileMock);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUploadLyricsCoverResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CoverImageUrl.Should().NotBeNullOrEmpty();
        result.CoverImageStorageKey.Should().NotBeNullOrEmpty();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsHasExistingCover_ShouldOverwriteInPlace()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.SetCoverImageFileId(Guid.NewGuid());
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: lyrics.Id, File: fileMock);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUploadLyricsCoverResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
    }
}
