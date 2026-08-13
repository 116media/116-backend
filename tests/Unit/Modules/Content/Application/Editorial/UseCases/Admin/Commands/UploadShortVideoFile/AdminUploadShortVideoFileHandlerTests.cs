using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoFileHandler"/>.
/// </summary>
public class AdminUploadShortVideoFileHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadShortVideoFileHandler _handler;

    public AdminUploadShortVideoFileHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        FileEntity fileEntity = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupReplaceVideoFile(fileEntity);

        _handler = new AdminUploadShortVideoFileHandler(
            _shortVideoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenDraftHasNoVideoFile_ShouldUploadAndAttachFile()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateDraft();
        FileEntity uploadedFile = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupReplaceVideoFile(uploadedFile);
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoFileResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.VideoFileId.Should().Be(uploadedFile.Id);
        result.VideoUrl.Should().Be(uploadedFile.StorageUrl);
        result.VideoStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileRepositoryMock.VerifyReplaceVideoFileCalled();
        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoAlreadyHasFile_ShouldReplaceIt()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        FileEntity uploadedFile = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupReplaceVideoFile(uploadedFile);
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoFileResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        shortVideo.VideoFileId.Should().Be(uploadedFile.Id);
        result.VideoUrl.Should().Be(uploadedFile.StorageUrl);
        result.VideoStorageKey.Should().Be(uploadedFile.StorageKey);
        _fileRepositoryMock.VerifyReplaceVideoFileCalled();
        _shortVideoRepositoryMock.VerifyUpdateCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
