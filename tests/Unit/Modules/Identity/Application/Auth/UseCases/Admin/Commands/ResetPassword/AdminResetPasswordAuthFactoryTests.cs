using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Unit tests for <see cref="AdminResetPasswordAuthFactory"/>.
/// </summary>
public class AdminResetPasswordAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminResetPasswordAuthFactory _factory;

    public AdminResetPasswordAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new AdminResetPasswordAuthFactory(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateUserErrors()
        );
    }

    #region GetUserForResetAsync Tests

    [Fact]
    public async Task GetUserForResetAsync_WithValidEmail_ShouldReturnAuthData()
    {
        // Arrange
        string email = "admin@example.com";
        var emailValue = new Email(email);
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAdmin(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        // Act
        AdminResetPasswordAuthData result = await _factory.GetUserForResetAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task GetUserForResetAsync_ShouldValidateUserIsAdmin()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAdmin(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        // Act
        await _factory.GetUserForResetAsync(email, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForResetAsync_ShouldValidateUserIsActive()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAdmin(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        // Act
        await _factory.GetUserForResetAsync(email, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForResetAsync_ShouldGetUserByEmail()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x =>
                x.GetUserWithRolesByEmailOrThrow(It.Is<Email>(e => e.Value == email), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAdmin(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        // Act
        await _factory.GetUserForResetAsync(email, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesByEmailOrThrow(It.Is<Email>(e => e.Value == email), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetUserForResetAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.Create(email);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), cancellationToken))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAdmin(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        // Act
        await _factory.GetUserForResetAsync(email, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), cancellationToken),
            Times.Once
        );
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidPassword_ShouldReturnUser()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        string hashedPassword = "hashed_new_password";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);

        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        UserEntity result = await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(user);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNewPasswordSameAsOld_ShouldThrowException()
    {
        // Arrange
        string newPassword = "SamePassword123!";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(true);

        // Act & Assert
        Func<Task> act = async () => await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);
        await act.Should().ThrowExactlyAsync<ConflictException>();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldVerifyOldPassword()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        string hashedPassword = "hashed_new_password";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);

        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldHashNewPassword()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        string hashedPassword = "hashed_new_password";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);

        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldCommitTransaction()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        string hashedPassword = "hashed_new_password";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);

        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithCancellationToken_ShouldPassToCommit()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        string hashedPassword = "hashed_new_password";
        UserEntity user = UserFactory.Create();
        CancellationToken cancellationToken = new();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);

        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _factory.ResetPasswordAsync(user, newPassword, cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion

    #region Session Invalidation

    [Fact]
    public async Task ResetPasswordAsync_ShouldRevokeEverySessionBeforeCommitting()
    {
        // Arrange
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.Create();

        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);
        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns("hashed_new_password");

        var callOrder = new List<string>();
        _sessionRepositoryMock
            .Setup(x =>
                x.DeleteAllByUserIdAsync(
                    user.Id,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    null,
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
        await _factory.ResetPasswordAsync(user, newPassword, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("revoke", "commit");
    }

    #endregion
}
