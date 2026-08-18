using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginAuthFactory"/>.
/// </summary>
public class AdminLoginAuthFactoryTests
{
    /// <summary>
    /// A stored hash written at the superseded work factor, which login replaces in place.
    /// </summary>
    private const string LegacyPasswordHash = "v1:hash-written-at-the-legacy-work-factor";

    /// <summary>
    /// A stored hash written at the current work factor, which login leaves alone.
    /// </summary>
    private const string CurrentPasswordHash = "v2:hash-written-at-the-current-work-factor";

    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IAccountLockoutRepository> _lockoutRepositoryMock;
    private readonly AdminLoginAuthFactory _factory;

    public AdminLoginAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _lockoutRepositoryMock = new Mock<IAccountLockoutRepository>();

        _lockoutRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AccountLockoutState(
                    FailedLoginAttempts: 0,
                    LockedUntil: null,
                    OtpFailedAttempts: 0,
                    OtpLockedUntil: null
                )
            );

        _factory = new AdminLoginAuthFactory(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _lockoutRepositoryMock.Object,
            _userErrors
        );
    }

    #region Success Cases

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAuthData()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        AdminLoginAuthData result = await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        result.User.Should().BeSameAs(user);
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByEmail()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetUserWithRolesAndPermissionsByEmailAsync(
                    It.Is<Email>(e => e.Value == email),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyPassword()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.VerifyOrDummy(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyUserIsAdmin()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldClearFailedLogins()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _lockoutRepositoryMock.Verify(
            x => x.ClearFailedLoginsAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WhenStoredHashIsAtTheLegacyWorkFactor_ShouldRewriteItAtTheCurrentOne()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        user.InitializePasswordHash(newPasswordHash: LegacyPasswordHash, errors: _userErrors);

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _passwordServiceMock.SetupNeedsRehash(needsRehash: true);
        _passwordServiceMock.SetupHashReturns(CurrentPasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be(CurrentPasswordHash);
        _passwordServiceMock.VerifyHashCalled(password);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenStoredHashIsAtTheCurrentWorkFactor_ShouldLeaveItUntouched()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        user.InitializePasswordHash(newPasswordHash: CurrentPasswordHash, errors: _userErrors);

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _passwordServiceMock.SetupNeedsRehash(needsRehash: false);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be(CurrentPasswordHash);
        _passwordServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldThrowTheSameInvalidCredentialsAsAWrongPassword()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsyncReturnsNull(new Email(email));

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>().WithMessage(_userErrors.InvalidCredentials().Message);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldStillSpendTheVerificationWork()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsyncReturnsNull(new Email(email));

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _passwordServiceMock.Verify(x => x.VerifyOrDummy(password, null), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldNotRegisterAFailedLogin()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsyncReturnsNull(new Email(email));

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedLoginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>().WithMessage(_userErrors.InvalidCredentials().Message);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldRegisterAFailedLogin()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedLoginAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldNotCheckAdminStatus()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotAdmin_ShouldThrowAuthorizationException()
    {
        // Arrange
        string email = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminThrowsAuthorizationException();

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthorizationException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenAccountIsLocked_ShouldThrowWithoutVerifyingThePassword()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _lockoutRepositoryMock
            .Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AccountLockoutState(
                    FailedLoginAttempts: 5,
                    LockedUntil: DateTime.UtcNow.AddMinutes(15),
                    OtpFailedAttempts: 0,
                    OtpLockedUntil: null
                )
            );

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>().WithMessage(_userErrors.InvalidCredentials().Message);
        _passwordServiceMock.Verify(x => x.VerifyOrDummy(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task AuthenticateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByEmailAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByEmailAsync(It.IsAny<Email>(), cts.Token),
            Times.Once
        );
    }

    #endregion
}
