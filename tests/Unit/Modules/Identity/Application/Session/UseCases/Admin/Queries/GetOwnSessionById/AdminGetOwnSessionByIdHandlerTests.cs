using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById;

/// <summary>
/// Unit tests for <see cref="AdminGetOwnSessionByIdHandler"/>.
/// </summary>
public class AdminGetOwnSessionByIdHandlerTests : BaseHandlerTest
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly AdminGetOwnSessionByIdHandler _handler;

    public AdminGetOwnSessionByIdHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();

        _handler = new AdminGetOwnSessionByIdHandler(_sessionRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidSessionId_ShouldReturnSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid(), userId);

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: session.Id);

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        AdminGetOwnSessionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Session.Should().NotBeNull();
        result.Session.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task Handle_ShouldFetchSessionById()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid(), userId);

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: session.Id);

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSessionDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid(), userId);

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: session.Id);

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        AdminGetOwnSessionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Session.Id.Should().Be(session.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: sessionId);

        _sessionRepositoryMock.SetupGetByIdReturnsNull(sessionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSessionBelongsToDifferentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid(), differentUserId);

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: session.Id);

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid(), userId);
        using CancellationTokenSource cts = new();

        AdminGetOwnSessionByIdQuery query = new(UserId: userId, SessionId: session.Id);

        _sessionRepositoryMock.SetupGetById(session);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetByIdAsync(session.Id, cts.Token), Times.Once);
    }

    #endregion
}
