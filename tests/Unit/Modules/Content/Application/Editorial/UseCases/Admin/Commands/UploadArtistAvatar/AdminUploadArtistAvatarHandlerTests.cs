using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;
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
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadArtistAvatarHandler _handler;

    public AdminUploadArtistAvatarHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadArtistAvatarHandler(
            _artistRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReplaceAvatarAndReturnUrl()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        FileEntity fileEntity = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

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

        _artistRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
    }
}
