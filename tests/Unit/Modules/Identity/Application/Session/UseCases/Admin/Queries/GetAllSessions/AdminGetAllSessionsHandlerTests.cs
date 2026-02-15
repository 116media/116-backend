using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Unit tests for <see cref="AdminGetAllSessionsHandler"/>.
/// </summary>
public class AdminGetAllSessionsHandlerTests : BaseHandlerTest
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly AdminGetAllSessionsHandler _handler;

    public AdminGetAllSessionsHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _handler = new AdminGetAllSessionsHandler(_sessionRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithSessions_ShouldReturnPaginatedResult()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest);

        List<SessionEntity> sessions = SessionFactory.CreateMany(5);
        _sessionRepositoryMock.SetupGetAllWithPagination(sessions, 5);

        // Act
        AdminGetAllSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Sessions.Should().NotBeNull();
        result.Sessions.Items.Should().HaveCount(5);
        result.Sessions.Count.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithNoSessions_ShouldReturnEmptyResult()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest);
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        AdminGetAllSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Sessions.Items.Should().BeEmpty();
        result.Sessions.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPaginationInfo()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 2, PageSize: 5);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest);

        List<SessionEntity> sessions = SessionFactory.CreateMany(5);
        _sessionRepositoryMock.SetupGetAllWithPagination(sessions, 25);

        // Act
        AdminGetAllSessionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Sessions.PageIndex.Should().Be(2);
        result.Sessions.PageSize.Should().Be(5);
        result.Sessions.Count.Should().Be(25);
    }

    [Fact]
    public async Task Handle_WithUserIdFilter_ShouldCallRepositoryWithParsedGuid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest, UserId: userId.ToString());
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    userId,
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithInvalidUserIdFilter_ShouldCallRepositoryWithNullUserId()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest, UserId: "invalid-guid");
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    null,
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyUserIdFilter_ShouldCallRepositoryWithNullUserId()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest, UserId: "");
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    null,
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime fromDate = DateTime.UtcNow.AddDays(-7);
        DateTime toDate = DateTime.UtcNow;
        string status = "Active";
        string ipAddress = "192.168.1.1";

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: paginatedRequest,
            Status: status,
            UserId: userId.ToString(),
            IpAddress: ipAddress,
            FromDate: fromDate,
            ToDate: toDate
        );
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    1, // pageIndex + 1
                    10,
                    status,
                    userId,
                    ipAddress,
                    fromDate,
                    toDate,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldAdjustPageIndexForRepository()
    {
        // Arrange - PageIndex is 0-based in query but 1-based for repository
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest);
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Repository should receive pageIndex + 1
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    1, // 0 + 1
                    10,
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllSessionsQuery query = new(PaginatedRequest: paginatedRequest);
        using CancellationTokenSource cts = new();
        _sessionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    cts.Token
                ),
            Times.Once
        );
    }

    #endregion
}
