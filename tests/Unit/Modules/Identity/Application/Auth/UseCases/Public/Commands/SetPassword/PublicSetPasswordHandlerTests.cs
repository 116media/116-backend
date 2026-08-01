using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Unit tests for <see cref="PublicSetPasswordHandler"/>.
/// </summary>
public class PublicSetPasswordHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSetPasswordHandler _handler;

    public PublicSetPasswordHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new PublicSetPasswordHandler(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        PublicSetPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(password), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetPasswordForExternalUser()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.SetPasswordForExternalUser(user, hashedPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsActive()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(It.IsAny<UserEntity>()), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicSetPasswordCommand command = new(UserId: userId, SessionId: Guid.NewGuid(), Password: "Password123!");

        _authRepositoryMock.SetupFindUserByIdOrThrowNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCommit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicSetPasswordCommand command = new(UserId: userId, SessionId: Guid.NewGuid(), Password: "Password123!");

        _authRepositoryMock.SetupFindUserByIdOrThrowNotFound(userId);

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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";
        using CancellationTokenSource cts = new();

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

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
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";
        using CancellationTokenSource cts = new();

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion

    #region Session Invalidation

    [Fact]
    public async Task Handle_ShouldRevokeEverySessionExceptTheActingOneBeforeCommitting()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        var sessionId = Guid.NewGuid();
        string password = "NewPassword123!";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: sessionId, Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.Setup(x => x.Hash(password)).Returns("hashed-password");

        var callOrder = new List<string>();
        _sessionRepositoryMock
            .Setup(x =>
                x.DeleteAllByUserIdAsync(
                    user.Id,
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
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("revoke", "commit");
    }

    #endregion
}
