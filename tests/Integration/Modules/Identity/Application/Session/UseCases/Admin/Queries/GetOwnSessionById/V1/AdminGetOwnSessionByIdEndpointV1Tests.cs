using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById.V1;

/// <summary>
/// Integration tests for the AdminGetOwnSessionById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnSessionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeSessions = $"{ApiRoutes.Admin.Base}/me/sessions";

    [Fact]
    public async Task GetOwnSessionById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{AdminMeSessions}/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnSessionById_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{AdminMeSessions}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOwnSessionById_AsSuperAdmin_WithExistingSession_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{AdminMeSessions}/{session.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
