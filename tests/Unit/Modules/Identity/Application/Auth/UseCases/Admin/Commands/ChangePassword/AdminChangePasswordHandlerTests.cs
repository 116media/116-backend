using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Unit tests for <see cref="AdminChangePasswordHandler"/>.
/// </summary>
public class AdminChangePasswordHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminChangePasswordHandler _handler;

    public AdminChangePasswordHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminChangePasswordHandler(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidOldPassword_ShouldReturnSuccess()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        string newPasswordHash = "new-hashed-password";

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Old password matches, new password doesn't match old hash
        _passwordServiceMock.Setup(x => x.Verify(oldPassword, user.PasswordHash)).Returns(true);
        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);
        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns(newPasswordHash);

        // Act
        AdminChangePasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        AdminChangePasswordCommand command = new(
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
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldVerifyOldPassword()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify is called for old password
        _passwordServiceMock.Verify(x => x.Verify(oldPassword, It.IsAny<string?>()), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        AdminChangePasswordCommand command = new(
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
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "WrongPassword!";
        string newPassword = "NewPassword456!";

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Old password doesn't match
        _passwordServiceMock.Setup(x => x.Verify(oldPassword, user.PasswordHash)).Returns(false);

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
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "SamePassword123!";
        string newPassword = "SamePassword123!";

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        // Both old and new password match the hash (they're the same)
        _passwordServiceMock.Setup(x => x.Verify(oldPassword, user.PasswordHash)).Returns(true);
        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(true);

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
        Guid sessionId = Guid.NewGuid();
        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: "wrong",
            NewPassword: "new"
        );

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);
        _passwordServiceMock.Setup(x => x.Verify("wrong", user.PasswordHash)).Returns(false);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        using CancellationTokenSource cts = new();

        AdminChangePasswordCommand command = new(
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
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string oldPassword = "OldPassword123!";
        string newPassword = "NewPassword456!";
        using CancellationTokenSource cts = new();

        AdminChangePasswordCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            OldPassword: oldPassword,
            NewPassword: newPassword
        );

        SetupSuccessfulChangePassword(user, sessionId, oldPassword, newPassword);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulChangePassword(UserEntity user, Guid sessionId, string oldPassword, string newPassword)
    {
        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _passwordServiceMock.Setup(x => x.Verify(oldPassword, user.PasswordHash)).Returns(true);
        _passwordServiceMock.Setup(x => x.Verify(newPassword, user.PasswordHash)).Returns(false);
        _passwordServiceMock.Setup(x => x.Hash(newPassword)).Returns("new-hashed-password");
    }

    #endregion
}
