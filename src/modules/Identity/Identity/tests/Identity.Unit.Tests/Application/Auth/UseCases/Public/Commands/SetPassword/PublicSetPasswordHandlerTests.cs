using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Unit tests for <see cref="PublicSetPasswordHandler"/>.
/// </summary>
public class PublicSetPasswordHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ICredentialInvalidationService> _credentialInvalidationServiceMock;
    private readonly PublicSetPasswordHandler _handler;

    public PublicSetPasswordHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _credentialInvalidationServiceMock = new Mock<ICredentialInvalidationService>();

        _handler = new PublicSetPasswordHandler(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _credentialInvalidationServiceMock.Object
        );
    }

    #region Success Cases

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
        _passwordServiceMock.SetupHash(password, hashedPassword);

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
        _passwordServiceMock.SetupHash(password, hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.SetPasswordForExternalUser(user, hashedPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCommitTheCredentialChange()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.SetupHash(password, hashedPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _credentialInvalidationServiceMock.Verify(
            x => x.CommitCredentialChangeAsync(user.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
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
        _passwordServiceMock.SetupHash(password, hashedPassword);

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
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";
        using CancellationTokenSource cts = new();

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.SetupHash(password, hashedPassword);

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
        string password = "NewPassword123!";
        string hashedPassword = "hashed-password";
        using CancellationTokenSource cts = new();

        PublicSetPasswordCommand command = new(UserId: user.Id, SessionId: Guid.NewGuid(), Password: password);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _passwordServiceMock.SetupHash(password, hashedPassword);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _credentialInvalidationServiceMock.Verify(
            x => x.CommitCredentialChangeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), cts.Token),
            Times.Once
        );
    }

    #endregion
}
