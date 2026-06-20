using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById.V1;

/// <summary>
/// Integration tests for the PublicGetOwnSessionById endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnSessionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetOwnSessionById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnSessionById_AsVisitor_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOwnSessionById_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
