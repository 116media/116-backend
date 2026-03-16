using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;

/// <summary>
/// Unit tests for <see cref="PublicSignOutFromAllDevicesHandler"/>.
/// </summary>
public class PublicSignOutFromAllDevicesHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSignOutFromAllDevicesHandler _handler;

    public PublicSignOutFromAllDevicesHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new PublicSignOutFromAllDevicesHandler(
            _authRepositoryMock.Object,
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
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        PublicSignOutFromAllDevicesResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldDeleteAllUserSessions()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.VerifyDeleteAllByUserIdCalled(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldFindUserFirst()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

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
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

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
        PublicSignOutFromAllDevicesCommand command = new(UserId: userId);

        _authRepositoryMock.SetupFindUserByIdOrThrowNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotDeleteSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicSignOutFromAllDevicesCommand command = new(UserId: userId);

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
        _sessionRepositoryMock.Verify(
            x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCommit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicSignOutFromAllDevicesCommand command = new(UserId: userId);

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
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.FindUserByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSignOutFromAllDevicesCommand command = new(UserId: user.Id);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
