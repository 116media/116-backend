using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="PublicUpdateAvatarAuthService"/>.
/// </summary>
public class PublicUpdateAvatarAuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicUpdateAvatarAuthService _service;

    public PublicUpdateAvatarAuthServiceTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _service = new PublicUpdateAvatarAuthService(_authRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region GetUserForAvatarUpdateAsync Tests

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_WithValidUser_ShouldReturnAuthData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Act
        PublicUpdateAvatarAuthData result = await _service.GetUserForAvatarUpdateAsync(
            userId,
            sessionId,
            CancellationToken.None
        );

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateUserIsActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Act
        await _service.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateUserIsVerified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Act
        await _service.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountVerified(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Act
        await _service.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        CancellationToken cancellationToken = CancellationToken.None;

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken))
            .ReturnsAsync(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();

        _authRepositoryMock.Setup(x => x.IsSessionValidAsync(sessionId, cancellationToken)).ReturnsAsync(true);

        // Act
        await _service.GetUserForAvatarUpdateAsync(userId, sessionId, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken),
            Times.Once
        );
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, cancellationToken), Times.Once);
    }

    #endregion

    #region UpdateAvatarAsync Tests

    [Fact]
    public async Task UpdateAvatarAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        PublicUpdateAvatarAuthData result = await _service.UpdateAvatarAsync(
            user,
            avatarFileId,
            CancellationToken.None
        );

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ShouldSetAvatarSourceToManual()
    {
        // Arrange
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateAvatarAsync(user, avatarFileId, CancellationToken.None);

        // Assert
        user.AvatarSource.Should().Be(EnumAvatarSource.Manual);
        user.AvatarFileId.Should().Be(avatarFileId);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ShouldCommitTransaction()
    {
        // Arrange
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateAvatarAsync(user, avatarFileId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithCancellationToken_ShouldPassToCommit()
    {
        // Arrange
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();
        CancellationToken cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _service.UpdateAvatarAsync(user, avatarFileId, cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
