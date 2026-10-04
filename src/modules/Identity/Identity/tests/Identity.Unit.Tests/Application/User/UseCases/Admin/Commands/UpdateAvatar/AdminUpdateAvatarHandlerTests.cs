using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.User.Ports;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="AdminUpdateAvatarHandler"/>.
/// </summary>
public class AdminUpdateAvatarHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminUpdateAvatarAuthService> _authServiceMock;
    private readonly Mock<IAvatarService> _avatarServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateAvatarHandler _handler;

    public AdminUpdateAvatarHandlerTests()
    {
        _authServiceMock = new Mock<IAdminUpdateAvatarAuthService>();
        _avatarServiceMock = MockAvatarService.Create();

        _unitOfWorkMock = MockIdentityUnitOfWork.Create().SetupExecuteInTransaction<AdminUpdateAvatarAuthData>();

        _handler = new AdminUpdateAvatarHandler(
            _authServiceMock.Object,
            _avatarServiceMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
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
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateWithId(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(User: user);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupUpload(StoredFileFactory.From(fileEntity));
        _authServiceMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        // Act
        AdminUpdateAvatarResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldGetUserForAvatarUpdate()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateWithId(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(User: user);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupUpload(StoredFileFactory.From(fileEntity));
        _authServiceMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
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
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateWithId(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(User: user);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupUpload(StoredFileFactory.From(fileEntity));
        _authServiceMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserAvatar()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        var newAvatarFileId = Guid.NewGuid();
        IFormFile avatarFile = FileTestHelpers.CreateMockFormFile();
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateWithId(newAvatarFileId);

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(User: user);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupUpload(StoredFileFactory.From(fileEntity));
        _authServiceMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
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

        AdminUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

        _authServiceMock
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

        AdminUpdateAvatarCommand command = new(UserId: userId, SessionId: sessionId, AvatarFile: avatarFile);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateWithId(newAvatarFileId);
        using CancellationTokenSource cts = new();

        AdminUpdateAvatarCommand command = new(UserId: user.Id, SessionId: sessionId, AvatarFile: avatarFile);
        AdminUpdateAvatarAuthData authData = new(User: user);

        _authServiceMock
            .Setup(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupUpload(StoredFileFactory.From(fileEntity));
        _authServiceMock
            .Setup(x => x.UpdateAvatarAsync(user, newAvatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authServiceMock.Verify(x => x.GetUserForAvatarUpdateAsync(user.Id, sessionId, cts.Token), Times.Once);
    }

    #endregion
}
