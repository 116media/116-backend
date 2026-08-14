using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.Public.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="PublicUpdateProfileAuthFactory"/>.
/// </summary>
public class PublicUpdateProfileAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly UserErrors _userErrors;
    private readonly PublicUpdateProfileAuthFactory _factory;

    public PublicUpdateProfileAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _userErrors = TestErrorsFactory.CreateUserErrors();
        _factory = new PublicUpdateProfileAuthFactory(
            _authRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userErrors
        );
    }

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string userName = "newusername";
        string countryName = "Rwanda";
        string countryIsoCode = "RW";
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        PublicUpdateProfileAuthData result = await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            userName,
            countryName,
            countryIsoCode,
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateUserIsActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateUserIsVerified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountVerified(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewEmail_ShouldCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string newEmail = "changed.profile@example.com";
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            newEmail,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewUsername_ShouldCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string newUserName = "newusername";
        UserEntity user = UserFactory.CreateWithId(userId);
        user.UpdateUserName("oldusername", _userErrors);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            newUserName,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithSameUsername_ShouldNotCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string userName = "sameusername";
        UserEntity user = UserFactory.CreateWithId(userId);
        user.UpdateUserName(userName, _userErrors);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            userName,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneNumber_ShouldCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        PublicUpdateProfileAuthData result = await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );

        // Assert
        result.User.FullPhoneNumber.Should().Be(fullPhoneNumber);
        result.User.CountryDialCode.Should().Be(countryDialCode);
        result.User.PartialPhoneNumber.Should().Be(partialPhoneNumber);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldCommitTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock.Setup(x => x.IsSessionValidAsync(sessionId, cancellationToken)).ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(userId, sessionId, null, null, null, null, null, null, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken),
            Times.Once
        );
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion

    #region Session Invalidation

    [Fact]
    public async Task UpdateProfileAsync_WhenEmailChanges_ShouldRevokeEverySessionExceptTheActingOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string newEmail = "fresh@example.com";
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var callOrder = new List<string>();
        _sessionRepositoryMock
            .Setup(x =>
                x.DeleteAllByUserIdAsync(
                    userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    sessionId,
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(() => callOrder.Add("revoke"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("commit"))
            .ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            newEmail,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        callOrder.Should().Equal("revoke", "commit");
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmailUnchanged_ShouldNotRevokeSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));
        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync("newusername", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            "newusername",
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.DeleteAllByUserIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumSessionRevokeReason>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion
}
