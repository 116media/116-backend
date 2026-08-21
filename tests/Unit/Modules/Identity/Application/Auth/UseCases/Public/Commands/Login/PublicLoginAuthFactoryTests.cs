using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginAuthFactory"/>.
/// </summary>
public class PublicLoginAuthFactoryTests
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
    private readonly PublicLoginAuthFactory _factory;

    public PublicLoginAuthFactoryTests()
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

        _factory = new PublicLoginAuthFactory(
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
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.User.Should().BeSameAs(user);
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByCredentials()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsAsync(credentials, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyPassword()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.VerifyOrDummy(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUsername_ShouldWork()
    {
        // Arrange
        string credentials = "testuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.User.Should().BeSameAs(user);
        result.User.UserName.Should().Be(user.UserName);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldClearFailedLogins()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

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
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        user.InitializePasswordHash(newPasswordHash: LegacyPasswordHash);

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _passwordServiceMock.SetupNeedsRehash(needsRehash: true);
        _passwordServiceMock.SetupHashReturns(CurrentPasswordHash);

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be(CurrentPasswordHash);
        _passwordServiceMock.VerifyHashCalled(password);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenStoredHashIsAtTheCurrentWorkFactor_ShouldLeaveItUntouched()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        user.InitializePasswordHash(newPasswordHash: CurrentPasswordHash);

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);
        _passwordServiceMock.SetupNeedsRehash(needsRehash: false);

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

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
        string credentials = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsyncReturnsNull(credentials);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>().WithMessage(_userErrors.InvalidCredentials().Message);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldStillSpendTheVerificationWork()
    {
        // Arrange
        string credentials = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsyncReturnsNull(credentials);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _passwordServiceMock.Verify(x => x.VerifyOrDummy(password, null), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldNotRegisterAFailedLogin()
    {
        // Arrange
        string credentials = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsyncReturnsNull(credentials);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

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
        string credentials = "user@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>().WithMessage(_userErrors.InvalidCredentials().Message);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldRegisterAFailedLogin()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedLoginAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WhenAccountIsLocked_ShouldThrowWithoutVerifyingThePassword()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
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
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

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
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsAsync(user);
        _passwordServiceMock.SetupVerifyOrDummySuccess(password, user.PasswordHash);

        // Act
        await _factory.AuthenticateAsync(credentials, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsAsync(credentials, cts.Token),
            Times.Once
        );
    }

    #endregion
}
