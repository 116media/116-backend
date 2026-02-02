using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;
using _116.Identity.Application.Shared.Persistence;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Sessions.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Unit tests for <see cref="AdminForceLogoutUserHandler"/>.
/// </summary>
public class AdminForceLogoutUserHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminForceLogoutUserHandler _handler;

    public AdminForceLogoutUserHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminForceLogoutUserHandler(_sessionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnSuccess()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());

        // Act
        AdminForceLogoutUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAllByUserIdAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.VerifyDeleteAllByUserIdCalled(userId);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteBeforeCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());
        int callOrder = 0;
        int deleteCallOrder = 0;
        int commitCallOrder = 0;

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
    public async Task Handle_WithInvalidGuid_ShouldThrowFormatException()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: "invalid-guid");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldCallDeleteWithEmptyGuid()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: Guid.Empty.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.VerifyDeleteAllByUserIdCalled(Guid.Empty);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(userId, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminForceLogoutUserCommand command = new(UserId: userId.ToString());
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
