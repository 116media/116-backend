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
    /// Seeds a session whose <c>CreatedAt</c> is a chosen instant. The audit interceptor stamps
    /// <c>CreatedAt</c> from the clock on insert and leaves it alone on update, so the instant is
    /// applied by a second save rather than by the seeded entity.
    /// </summary>
    /// <param name="createdAt">The creation instant the date filters should see.</param>
    /// <returns>The identifier of the seeded session.</returns>
    private async Task<Guid> SeedSessionCreatedAtAsync(DateTime createdAt)
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity tracked = ctx.Sessions.Single(s => s.Id == session.Id);
            tracked.CreatedAt = createdAt;
        });

        return session.Id;
    }

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

    [Fact]
    public async Task GetAllSessions_FilterByFromDate_ReturnsFilteredResults()
    {
        Guid inWindow = await SeedSessionCreatedAtAsync(new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc));
        Guid outOfWindow = await SeedSessionCreatedAtAsync(new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&fromDate=2026-06-01T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == inWindow);
        body.Sessions.Items.Should().NotContain(s => s.Id == outOfWindow);
    }

    [Fact]
    public async Task GetAllSessions_FilterByToDate_ReturnsFilteredResults()
    {
        Guid inWindow = await SeedSessionCreatedAtAsync(new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        Guid outOfWindow = await SeedSessionCreatedAtAsync(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2026-03-02T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
        body.Sessions.Items.Should().Contain(s => s.Id == inWindow);
        body.Sessions.Items.Should().NotContain(s => s.Id == outOfWindow);
    }

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
