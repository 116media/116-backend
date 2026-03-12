using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="PublicUpdateAvatarHandler"/>.
/// </summary>
public class PublicUpdateAvatarHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicUpdateAvatarAuthFactory> _authFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicUpdateAvatarHandler _handler;

    public PublicUpdateAvatarHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicUpdateAvatarAuthFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new PublicUpdateAvatarHandler(_authFactoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnUpdatedProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileEntity fileEntity = FileFactory.CreateWithId(newAvatarFileId);

        PublicUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        PublicUpdateAvatarAuthData authData = new(User: user);

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
        PublicUpdateAvatarResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldGetUserForAvatarUpdate()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileEntity fileEntity = FileFactory.CreateWithId(newAvatarFileId);

        PublicUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        PublicUpdateAvatarAuthData authData = new(User: user);

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
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileEntity fileEntity = FileFactory.CreateWithId(newAvatarFileId);

        PublicUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        PublicUpdateAvatarAuthData authData = new(User: user);

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
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileEntity fileEntity = FileFactory.CreateWithId(newAvatarFileId);

        PublicUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        PublicUpdateAvatarAuthData authData = new(User: user);

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

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();

        PublicUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

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
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();

        PublicUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

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
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileEntity fileEntity = FileFactory.CreateWithId(newAvatarFileId);
        using CancellationTokenSource cts = new();

        PublicUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        PublicUpdateAvatarAuthData authData = new(User: user);

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

    #endregion
}
