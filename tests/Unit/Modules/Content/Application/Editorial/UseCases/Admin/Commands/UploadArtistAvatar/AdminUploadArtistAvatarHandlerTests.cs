using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Unit tests for <see cref="AdminUploadArtistAvatarHandler"/>.
/// </summary>
public class AdminUploadArtistAvatarHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadArtistAvatarHandler _handler;

    public AdminUploadArtistAvatarHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadArtistAvatarHandler(
            _artistRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReplaceAvatarAndReturnUrl()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("avatar.png");
        fileMock.Setup(f => f.ContentType).Returns("image/png");

        var command = new AdminUploadArtistAvatarCommand(artist.Id, fileMock.Object);

        // Act
        AdminUploadArtistAvatarResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AvatarUrl.Should().Be(fileEntity.StorageUrl);
        result.AvatarStorageKey.Should().Be(fileEntity.StorageKey);
        artist.AvatarFileId.Should().Be(fileEntity.Id);

        _artistRepositoryMock.VerifyUpdateCalled(artist);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        Mock<IFormFile> fileMock = new();
        var command = new AdminUploadArtistAvatarCommand(nonExistentId, fileMock.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }
}
