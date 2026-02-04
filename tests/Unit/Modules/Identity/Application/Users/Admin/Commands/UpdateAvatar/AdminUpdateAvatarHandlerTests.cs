using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Users.Admin.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="AdminUpdateAvatarHandler"/>.
/// </summary>
public class AdminUpdateAvatarHandlerTests
{
    private readonly Mock<IAdminUpdateAvatarAuthFactory> _authFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUpdateAvatarHandler _handler;

    public AdminUpdateAvatarHandlerTests()
    {
        _authFactoryMock = new Mock<IAdminUpdateAvatarAuthFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new AdminUpdateAvatarHandler(_authFactoryMock.Object, _fileRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnUpdatedProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [new RoleDto(Guid.NewGuid(), "Admin", "Admin role", true, false, null)];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        AdminUpdateAvatarResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldGetUserForAvatarUpdate()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldUploadAvatarFile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarFromFileAsync(
                    user.AvatarFileId,
                    avatarFile,
                    user.Id.ToString(),
                    avatarFile.FileName,
                    avatarFile.ContentType,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserAvatar()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchUpdatedAvatarFile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x => x.GetAvatarFileAsync(user.AvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        IFormFile avatarFile = CreateMockFormFile();

        AdminUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotUploadFile()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        IFormFile avatarFile = CreateMockFormFile();

        AdminUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);
        using CancellationTokenSource cts = new();

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        Guid newAvatarFileId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        IFormFile avatarFile = CreateMockFormFile();
        FileEntity fileEntity = CreateMockFileEntity(newAvatarFileId);
        using CancellationTokenSource cts = new();

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    avatarFile,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(fileEntity);
        _authFactoryMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarFromFileAsync(
                    user.AvatarFileId,
                    avatarFile,
                    user.Id.ToString(),
                    avatarFile.FileName,
                    avatarFile.ContentType,
                    cts.Token
                ),
            Times.Once
        );
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFormFile()
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.Length).Returns(1024);
        return fileMock.Object;
    }

    private static FileEntity CreateMockFileEntity(Guid fileId)
    {
        return FileEntity.Create(
            id: fileId,
            fileName: "avatar.jpg",
            originalFileName: "avatar.jpg",
            mimeType: "image/jpeg",
            storageUrl: "https://example.com/avatar.jpg",
            sizeInBytes: 1024
        );
    }

    #endregion
}
