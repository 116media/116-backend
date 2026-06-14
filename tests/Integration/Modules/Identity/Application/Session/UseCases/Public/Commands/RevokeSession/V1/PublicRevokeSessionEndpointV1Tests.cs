using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;

/// <summary>
/// Integration tests for the PublicRevokeSession endpoint.
/// </summary>
[Collection("Database")]
public class PublicRevokeSessionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublicRevokeSession_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{ApiRoutes.Public.Me}/sessions/revoke/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
