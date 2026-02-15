using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;

/// <summary>
/// Unit tests for <see cref="PublicRevokeSessionHandler"/>.
/// </summary>
public class PublicRevokeSessionHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicRevokeSessionHandler _handler;

    public PublicRevokeSessionHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new PublicRevokeSessionHandler(_sessionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidSessionBelongingToUser_ShouldReturnSuccess()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        PublicRevokeSessionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallRevokeAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.VerifyRevokeCalled(session.Id);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: sessionId.ToString());

        _sessionRepositoryMock.SetupGetByIdReturnsNull(sessionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSessionBelongsToAnotherUser_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid anotherUserId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(anotherUserId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldNotCallRevoke()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: sessionId.ToString());

        _sessionRepositoryMock.SetupGetByIdReturnsNull(sessionId);

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
        _sessionRepositoryMock.Verify(x => x.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldNotCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: sessionId.ToString());

        _sessionRepositoryMock.SetupGetByIdReturnsNull(sessionId);

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

    [Fact]
    public async Task Handle_WithInvalidGuid_ShouldThrowFormatException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: "invalid-guid");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FormatException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToGetByIdAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());
        using CancellationTokenSource cts = new();

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetByIdAsync(session.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRevokeAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());
        using CancellationTokenSource cts = new();

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.RevokeAsync(session.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        PublicRevokeSessionCommand command = new(UserId: userId, SessionId: session.Id.ToString());
        using CancellationTokenSource cts = new();

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
