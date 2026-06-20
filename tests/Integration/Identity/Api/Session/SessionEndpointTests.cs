using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Session;

/// <summary>
/// Integration tests for the session management endpoints covering both
/// admin (<c>/api/v1/admin/sessions</c>) and public (<c>/api/v1/public/me/sessions</c>) scopes.
/// </summary>
[Collection("Database")]
public class SessionEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminGetAllSessions_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(User.SuperAdminId);
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

    [Fact]
    public async Task AdminGetOwnSessions_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(User.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync("/api/v1/admin/me/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task AdminGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync("/api/v1/admin/me/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetSessionMetrics_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetSessionMetrics_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCleanup_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var expiredSession = SessionFactory.CreateExpired(User.SuperAdminId);
        context.Sessions.Add(expiredSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/cleanup", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("deletedCount", out var deletedCount).Should().BeTrue();
        deletedCount.GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AdminCleanup_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/cleanup", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminRevokeSession_WithNonExistentId_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"/api/v1/admin/me/sessions/revoke/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminExportSessions_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PublicGetOwnSessions_AsVisitor_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(User.VisitorId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task PublicGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnSessions_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicRevokeSession_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{ApiRoutes.Public.Me}/sessions/revoke/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
