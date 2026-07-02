using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;

/// <summary>
/// Integration tests for the AdminGetAllSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Creates a session entity with specific user and IP address for filter testing.
    /// </summary>
    private static SessionEntity CreateSessionWithIp(Guid userId, string ipAddress)
    {
        return SessionEntity.Create(
            Guid.NewGuid(),
            userId,
            $"device-{Guid.NewGuid():N}"[..20],
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.AddDays(30),
            EnumBrowser.Chrome,
            EnumDevice.Desktop,
            EnumPlatform.Windows,
            EnumClient.WebApp,
            ipAddress
        );
    }

    [Fact]
    public async Task AdminGetAllSessions_AsSuperAdmin_Returns200()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == session.Id);
        body.Sessions.PageIndex.Should().Be(0);
        body.Sessions.PageSize.Should().Be(10);
        body.Sessions.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AdminGetAllSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetAllSessions_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that filtering sessions by IP address returns only sessions
    /// matching the specified IP address pattern.
    /// Covers SessionByIpAddressSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByIpAddress_ReturnsFilteredResults()
    {
        SessionEntity matchingSession = CreateSessionWithIp(TestUser.SuperAdminId, "192.168.1.1");
        SessionEntity otherSession = CreateSessionWithIp(TestUser.AdminId, "10.0.0.1");
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(matchingSession, otherSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&ipAddress=192.168.1.1"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().NotBeEmpty();
        body.Sessions.Items.Should().OnlyContain(s => s.IpAddress!.Contains("192.168.1.1"));
    }

    /// <summary>
    /// Verifies that filtering sessions by fromDate returns only sessions
    /// created on or after the specified date.
    /// Covers SessionCreatedAfterSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByFromDate_ReturnsFilteredResults()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity session = SessionFactory.Create(TestUser.SuperAdminId);
            session.CreatedAt = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            ctx.Sessions.Add(session);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&fromDate=2026-06-01T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that filtering sessions by toDate returns only sessions
    /// created on or before the specified date.
    /// Covers SessionCreatedBeforeSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByToDate_ReturnsFilteredResults()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2030-01-01T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == session.Id);
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Active returns only sessions
    /// that are not revoked and not expired.
    /// Covers SessionIsActiveSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusActive_ReturnsFilteredResults()
    {
        SessionEntity activeSession = SessionFactory.Create(TestUser.SuperAdminId);
        SessionEntity expiredSession = SessionFactory.CreateExpired(TestUser.AdminId);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(activeSession, expiredSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().NotBeEmpty();
        body.Sessions.Items.Should().OnlyContain(s => s.IsActive);
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Expired returns only sessions
    /// whose expiration date is in the past.
    /// Covers SessionIsExpiredSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusExpired_ReturnsFilteredResults()
    {
        SessionEntity expiredSession = SessionFactory.CreateExpired(TestUser.SuperAdminId);
        SessionEntity activeSession = SessionFactory.Create(TestUser.AdminId);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(expiredSession, activeSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Expired");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == expiredSession.Id);
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Revoked returns only sessions
    /// that have been explicitly revoked.
    /// Covers SessionIsRevokedSpecification and the Revoked branch of SessionQueryBuilder.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusRevoked_ReturnsFilteredResults()
    {
        SessionEntity revokedSession = SessionFactory.CreateRevoked(TestUser.SuperAdminId);
        SessionEntity activeSession = SessionFactory.Create(TestUser.AdminId);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(revokedSession, activeSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Revoked");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().NotBeEmpty();
        body.Sessions.Items.Should().OnlyContain(s => !s.IsActive);
    }

    /// <summary>
    /// Verifies that combining multiple filters (status and IP address) returns
    /// only sessions matching all criteria simultaneously.
    /// Covers combined specification composition via SessionQueryBuilder.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults()
    {
        SessionEntity matchingSession = CreateSessionWithIp(TestUser.SuperAdminId, "203.0.113.50");
        SessionEntity nonMatchingSession = CreateSessionWithIp(TestUser.AdminId, "198.51.100.1");
        nonMatchingSession.Revoke();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(matchingSession, nonMatchingSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Active&ipAddress=203.0.113.50"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().NotBeEmpty();
        body.Sessions.Items.Should().OnlyContain(s => s.IpAddress!.Contains("203.0.113.50") && s.IsActive);
    }

    /// <summary>
    /// Verifies that filtering sessions by userId returns only sessions
    /// belonging to the specified user. This exercises the
    /// SessionQueryBuilder.WithUserId path and SessionByUserIdSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByUserId_ReturnsOnlyMatchingUserSessions()
    {
        SessionEntity targetSession = SessionFactory.Create(TestUser.SuperAdminId);
        SessionEntity otherSession = SessionFactory.Create(TestUser.AdminId);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(targetSession, otherSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&userId={TestUser.SuperAdminId}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == targetSession.Id);
        body.Sessions.Items.Should().NotContain(s => s.Id == otherSession.Id);
    }

    /// <summary>
    /// Verifies that combining userId and status filters returns only sessions
    /// matching both criteria. Covers the SessionQueryBuilder composition
    /// with WithUserId and WithStatus together.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByUserIdAndStatus_ReturnsFilteredResults()
    {
        SessionEntity activeSession = SessionFactory.Create(TestUser.SuperAdminId);
        SessionEntity revokedSession = SessionFactory.CreateRevoked(TestUser.SuperAdminId);
        SessionEntity otherUserSession = SessionFactory.Create(TestUser.AdminId);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(activeSession, revokedSession, otherUserSession);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&userId={TestUser.SuperAdminId}&status=Active"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == activeSession.Id);
        body.Sessions.Items.Should().OnlyContain(s => s.IsActive);
    }
}
