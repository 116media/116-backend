using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;
using _116.Identity.Application.Shared.Persistence;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;

/// <summary>
/// Unit tests for <see cref="AdminCleanupExpiredSessionsHandler"/>.
/// </summary>
public class AdminCleanupExpiredSessionsHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminCleanupExpiredSessionsHandler _handler;

    public AdminCleanupExpiredSessionsHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminCleanupExpiredSessionsHandler(_sessionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenExpiredSessionsExist_ShouldReturnDeletedCount()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(10);

        // Act
        AdminCleanupExpiredSessionsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DeletedCount.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenNoExpiredSessions_ShouldReturnZero()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(0);

        // Act
        AdminCleanupExpiredSessionsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DeletedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteExpiredSessionsAsync()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(3);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(x => x.DeleteExpiredSessionsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        using CancellationTokenSource cts = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(1);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.DeleteExpiredSessionsAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        AdminCleanupExpiredSessionsCommand command = new();
        using CancellationTokenSource cts = new();
        _sessionRepositoryMock.SetupDeleteExpiredSessions(1);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
