using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Admin.SignOutFromAllDevices;

/// <summary>
/// Unit tests for <see cref="AdminSignOutFromAllDevicesHandler"/>.
/// </summary>
public class AdminSignOutFromAllDevicesHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminSignOutFromAllDevicesHandler _handler;

    public AdminSignOutFromAllDevicesHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminSignOutFromAllDevicesHandler(
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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        AdminSignOutFromAllDevicesResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldDeleteAllUserSessions()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);

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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);

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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);

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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteSessionsBeforeCommit()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);
        int callOrder = 0;
        int deleteCallOrder = 0;
        int commitCallOrder = 0;

        _authRepositoryMock.SetupFindUserByIdOrThrow(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _sessionRepositoryMock
            .Setup(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback(() => deleteCallOrder = ++callOrder)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => commitCallOrder = ++callOrder)
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        deleteCallOrder.Should().Be(1);
        commitCallOrder.Should().Be(2);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminSignOutFromAllDevicesCommand command = new(UserId: userId);

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
        Guid userId = Guid.NewGuid();
        AdminSignOutFromAllDevicesCommand command = new(UserId: userId);

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
        Guid userId = Guid.NewGuid();
        AdminSignOutFromAllDevicesCommand command = new(UserId: userId);

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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);
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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);
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
        AdminSignOutFromAllDevicesCommand command = new(UserId: user.Id);
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
