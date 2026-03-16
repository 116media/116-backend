using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnSessionsHandler"/>.
/// </summary>
public class PublicGetOwnSessionsHandlerTests : BaseHandlerTest
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly PublicGetOwnSessionsHandler _handler;

    public PublicGetOwnSessionsHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();

        _handler = new PublicGetOwnSessionsHandler(_sessionRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithSessions_ShouldReturnSessionDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId);

        List<SessionEntity> sessions = SessionFactory.CreateMany(3);
        _sessionRepositoryMock.SetupGetUserSessions(userId, sessions);

        // Act
        PublicGetOwnSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Sessions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNoSessions_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId);
        _sessionRepositoryMock.SetupGetUserSessionsEmpty(userId);

        // Act
        PublicGetOwnSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId);
        _sessionRepositoryMock.SetupGetUserSessionsEmpty(userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetUserSessionsAsync(userId, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId, IsActive: true);
        _sessionRepositoryMock.SetupGetUserSessionsEmpty(userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetUserSessionsAsync(userId, true, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithInactiveFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId, IsActive: false);
        _sessionRepositoryMock.SetupGetUserSessionsEmpty(userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetUserSessionsAsync(userId, false, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId);

        List<SessionEntity> sessions = SessionFactory.CreateMany(2);
        _sessionRepositoryMock.SetupGetUserSessions(userId, sessions);

        // Act
        PublicGetOwnSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result
            .Sessions.Should()
            .BeAssignableTo<IReadOnlyCollection<_116.Identity.Application.Shared.DTOs.SessionDto>>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PublicGetOwnSessionsQuery query = new(UserId: userId);
        using CancellationTokenSource cts = new();
        _sessionRepositoryMock.SetupGetUserSessionsEmpty(userId);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetUserSessionsAsync(userId, null, cts.Token), Times.Once);
    }

    #endregion
}
