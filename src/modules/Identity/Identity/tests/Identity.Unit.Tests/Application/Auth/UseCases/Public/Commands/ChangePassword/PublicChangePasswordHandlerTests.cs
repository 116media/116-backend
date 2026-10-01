using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.UseCases.Public.Commands.ChangePassword;

/// <summary>
/// Unit tests for <see cref="PublicChangePasswordHandler"/>.
/// </summary>
public class PublicChangePasswordHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ICredentialInvalidationService> _credentialInvalidationServiceMock;
    private readonly PublicChangePasswordHandler _handler;

    public PublicChangePasswordHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _credentialInvalidationServiceMock = new Mock<ICredentialInvalidationService>();

        _handler = new PublicChangePasswordHandler(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _credentialInvalidationServiceMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidOldPassword_ShouldUpdateUserPasswordHash()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        string newPasswordHash = "new-hashed-password";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _passwordServiceMock.SetupVerifySuccess(oldPassword, user.PasswordHash);
        _passwordServiceMock.SetupVerifyFailure(newPassword, user.PasswordHash);
        _passwordServiceMock.SetupHash(newPassword, newPasswordHash);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCommitTheCredentialChange()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _credentialInvalidationServiceMock.Verify(
            x => x.CommitCredentialChangeAsync(user.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldVerifyOldPassword()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Verify(oldPassword, It.IsAny<string?>()), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        PublicChangePasswordCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            OldPassword: "old",
            NewPassword: "new"
        );

        _authRepositoryMock.SetupFindUserByIdOrThrowNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOldPasswordIncorrect_ShouldThrowBadRequestException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "WrongPassword!";
        string newPassword = "NewPassword456!";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _passwordServiceMock.SetupVerifyFailure(oldPassword, user.PasswordHash);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenNewPasswordSameAsOld_ShouldThrowConflictException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "SamePassword123!";
        string newPassword = "SamePassword123!";

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _passwordServiceMock.SetupVerifySuccess(oldPassword, user.PasswordHash);
        _passwordServiceMock.SetupVerifySuccess(newPassword, user.PasswordHash);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenOldPasswordIncorrect_ShouldNotCommit()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: "wrong",
            NewPassword: "new"
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);
        _passwordServiceMock.SetupVerifyFailure("wrong", user.PasswordHash);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _credentialInvalidationServiceMock.Verify(
            x => x.CommitCredentialChangeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        using CancellationTokenSource cts = new();

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.FindUserByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheCredentialInvalidationService()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        using CancellationTokenSource cts = new();

        PublicChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _credentialInvalidationServiceMock.Verify(
            x => x.CommitCredentialChangeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), cts.Token),
            Times.Once
        );
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulChangePassword(UserEntity user, Guid sessionId, string oldPassword, string newPassword)
    {
        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _passwordServiceMock.SetupVerifySuccess(oldPassword, user.PasswordHash);
        _passwordServiceMock.SetupVerifyFailure(newPassword, user.PasswordHash);
        _passwordServiceMock.SetupHash(newPassword, "new-hashed-password");
    }

    #endregion
}
