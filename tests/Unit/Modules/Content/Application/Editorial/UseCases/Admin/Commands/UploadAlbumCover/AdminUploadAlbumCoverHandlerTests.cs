using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;
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
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Unit tests for <see cref="AdminUploadAlbumCoverHandler"/>.
/// </summary>
public class AdminUploadAlbumCoverHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadAlbumCoverHandler _handler;

    public AdminUploadAlbumCoverHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _albumRepositoryMock = MockAlbumRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadAlbumCoverHandler(
            _albumRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReplaceCoverAndReturnUrl()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("cover.png");
        fileMock.Setup(f => f.ContentType).Returns("image/png");

        var command = new AdminUploadAlbumCoverCommand(album.Id, fileMock.Object);

        // Act
        AdminUploadAlbumCoverResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CoverImageUrl.Should().Be(fileEntity.StorageUrl);
        result.CoverImageStorageKey.Should().Be(fileEntity.StorageKey);
        album.CoverImageFileId.Should().Be(fileEntity.Id);

        _albumRepositoryMock.VerifyUpdateCalled(album);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_ShouldPreserveExistingNameAndLabel()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        string originalName = album.Name;
        string? originalLabel = album.Label;
        _albumRepositoryMock.SetupGetByIdOrThrow(album);

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("cover.png");
        fileMock.Setup(f => f.ContentType).Returns("image/png");

        var command = new AdminUploadAlbumCoverCommand(album.Id, fileMock.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        album.Name.Should().Be(originalName);
        album.Label.Should().Be(originalLabel);
        album.CoverImageFileId.Should().Be(fileEntity.Id);
    }

    [Fact]
    public async Task Handle_WhenAlbumNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        Mock<IFormFile> fileMock = new();
        var command = new AdminUploadAlbumCoverCommand(nonExistentId, fileMock.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
