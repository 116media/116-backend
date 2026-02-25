using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Unit tests for <see cref="AdminExportSessionDataHandler"/>.
/// </summary>
public class AdminExportSessionDataHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly AdminExportSessionDataHandler _handler;

    public AdminExportSessionDataHandlerTests()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        IMapper mapper = new Mapper(config);

        _sessionRepositoryMock = MockSessionRepository.Create();
        _handler = new AdminExportSessionDataHandler(_sessionRepositoryMock.Object, mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllSessions()
    {
        // Arrange
        List<SessionEntity> sessions = SessionFactory.CreateMany(5);

        AdminExportSessionDataQuery query = new();

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        AdminExportSessionDataResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SessionData.Should().NotBeNull();
        result.SessionData.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ShouldFetchSessionsForExport()
    {
        // Arrange
        List<SessionEntity> sessions = SessionFactory.CreateMany(3);

        AdminExportSessionDataQuery query = new();

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetSessionsForExportAsync(null, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassToRepository()
    {
        // Arrange
        List<SessionEntity> sessions = [];
        string status = "active";

        AdminExportSessionDataQuery query = new(Status: status);

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetSessionsForExportAsync(status, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithDateFilters_ShouldPassToRepository()
    {
        // Arrange
        List<SessionEntity> sessions = [];
        DateTime fromDate = DateTime.UtcNow.AddDays(-7);
        DateTime toDate = DateTime.UtcNow;

        AdminExportSessionDataQuery query = new(FromDate: fromDate, ToDate: toDate);

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetSessionsForExportAsync(null, fromDate, toDate, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllToRepository()
    {
        // Arrange
        List<SessionEntity> sessions = [];
        string status = "expired";
        DateTime fromDate = DateTime.UtcNow.AddDays(-30);
        DateTime toDate = DateTime.UtcNow.AddDays(-1);

        AdminExportSessionDataQuery query = new(Status: status, FromDate: fromDate, ToDate: toDate);

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetSessionsForExportAsync(status, fromDate, toDate, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoSessions_ShouldReturnEmptyList()
    {
        // Arrange
        List<SessionEntity> sessions = [];

        AdminExportSessionDataQuery query = new();

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        AdminExportSessionDataResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.SessionData.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapSessionsToExportDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        List<SessionEntity> sessions = SessionFactory.CreateMany(userId, 2);

        AdminExportSessionDataQuery query = new();

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        AdminExportSessionDataResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.SessionData.Should().HaveCount(2);
        result.SessionData.Should().AllSatisfy(dto => dto.UserId.Should().Be(userId));
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionRepository()
    {
        // Arrange
        List<SessionEntity> sessions = [];
        using CancellationTokenSource cts = new();

        AdminExportSessionDataQuery query = new();

        _sessionRepositoryMock.SetupGetSessionsForExport(sessions);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetSessionsForExportAsync(null, null, null, cts.Token), Times.Once);
    }

    #endregion
}
