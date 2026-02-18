using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Unit tests for <see cref="PublicSignOutHandler"/>.
/// </summary>
public class PublicSignOutHandlerTests
{
    private readonly Mock<IPublicSignOutSessionFactory> _sessionFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly PublicSignOutHandler _handler;

    public PublicSignOutHandlerTests()
    {
        _sessionFactoryMock = new Mock<IPublicSignOutSessionFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new PublicSignOutHandler(_sessionFactoryMock.Object, _authRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        PublicSignOutResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallSessionFactorySignOut()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionFactoryMock.Verify(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFindUserFirst()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.FindUserByIdOrThrow(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsActive()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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
        Guid userId = Guid.NewGuid();
        PublicSignOutCommand command = new(UserId: userId, RefreshToken: "token");

        _authRepositoryMock.SetupFindUserByIdOrThrowNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCallSessionFactory()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        PublicSignOutCommand command = new(UserId: userId, RefreshToken: "token");

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
        _sessionFactoryMock.Verify(x => x.SignOutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.FindUserByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionFactory()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        string refreshToken = "valid-refresh-token";
        PublicSignOutCommand command = new(UserId: user.Id, RefreshToken: refreshToken);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _sessionFactoryMock
            .Setup(x => x.SignOutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionFactoryMock.Verify(x => x.SignOutAsync(refreshToken, cts.Token), Times.Once);
    }

    #endregion
}
