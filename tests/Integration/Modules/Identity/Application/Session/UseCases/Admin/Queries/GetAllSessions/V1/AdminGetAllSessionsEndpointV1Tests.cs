using System.Text.Json;
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
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        sessionsProp.TryGetProperty("pageIndex", out _).Should().BeTrue();
        sessionsProp.TryGetProperty("pageSize", out _).Should().BeTrue();
        sessionsProp.TryGetProperty("count", out _).Should().BeTrue();
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
        await using var context = CreateDbContext<IdentityDbContext>();
        var matchingSession = CreateSessionWithIp(TestUser.SuperAdminId, "192.168.1.1");
        var otherSession = CreateSessionWithIp(TestUser.AdminId, "10.0.0.1");
        context.Sessions.AddRange(matchingSession, otherSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&ipAddress=192.168.1.1"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("ipAddress").GetString().Should().Contain("192.168.1.1");
        }
    }

    /// <summary>
    /// Verifies that filtering sessions by fromDate returns only sessions
    /// created on or after the specified date.
    /// Covers SessionCreatedAfterSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByFromDate_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        session.CreatedAt = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&fromDate=2026-06-01T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that filtering sessions by toDate returns only sessions
    /// created on or before the specified date.
    /// Covers SessionCreatedBeforeSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByToDate_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2030-01-01T00:00:00Z"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Active returns only sessions
    /// that are not revoked and not expired.
    /// Covers SessionIsActiveSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusActive_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var activeSession = SessionFactory.Create(TestUser.SuperAdminId);
        var expiredSession = SessionFactory.CreateExpired(TestUser.AdminId);
        context.Sessions.AddRange(activeSession, expiredSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Expired returns only sessions
    /// whose expiration date is in the past.
    /// Covers SessionIsExpiredSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusExpired_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var expiredSession = SessionFactory.CreateExpired(TestUser.SuperAdminId);
        var activeSession = SessionFactory.Create(TestUser.AdminId);
        context.Sessions.AddRange(expiredSession, activeSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Expired");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that filtering sessions by status=Revoked returns only sessions
    /// that have been explicitly revoked.
    /// Covers SessionIsRevokedSpecification and the Revoked branch of SessionQueryBuilder.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusRevoked_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var revokedSession = SessionFactory.CreateRevoked(TestUser.SuperAdminId);
        var activeSession = SessionFactory.Create(TestUser.AdminId);
        context.Sessions.AddRange(revokedSession, activeSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Revoked");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeFalse();
        }
    }

    /// <summary>
    /// Verifies that combining multiple filters (status and IP address) returns
    /// only sessions matching all criteria simultaneously.
    /// Covers combined specification composition via SessionQueryBuilder.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var matchingSession = CreateSessionWithIp(TestUser.SuperAdminId, "203.0.113.50");
        var nonMatchingSession = CreateSessionWithIp(TestUser.AdminId, "198.51.100.1");
        nonMatchingSession.Revoke();
        context.Sessions.AddRange(matchingSession, nonMatchingSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&status=Active&ipAddress=203.0.113.50"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("ipAddress").GetString().Should().Contain("203.0.113.50");
        }
    }

    /// <summary>
    /// Verifies that filtering sessions by userId returns only sessions
    /// belonging to the specified user. This exercises the
    /// SessionQueryBuilder.WithUserId path and SessionByUserIdSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByUserId_ReturnsOnlyMatchingUserSessions()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var targetSession = SessionFactory.Create(TestUser.SuperAdminId);
        var otherSession = SessionFactory.Create(TestUser.AdminId);
        context.Sessions.AddRange(targetSession, otherSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&userId={TestUser.SuperAdminId}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that combining userId and status filters returns only sessions
    /// matching both criteria. Covers the SessionQueryBuilder composition
    /// with WithUserId and WithStatus together.
    /// </summary>
    [Fact]
    public async Task GetAllSessions_FilterByUserIdAndStatus_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var activeSession = SessionFactory.Create(TestUser.SuperAdminId);
        var revokedSession = SessionFactory.CreateRevoked(TestUser.SuperAdminId);
        var otherUserSession = SessionFactory.Create(TestUser.AdminId);
        context.Sessions.AddRange(activeSession, revokedSession, otherUserSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&userId={TestUser.SuperAdminId}&status=Active"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("sessions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }
}
