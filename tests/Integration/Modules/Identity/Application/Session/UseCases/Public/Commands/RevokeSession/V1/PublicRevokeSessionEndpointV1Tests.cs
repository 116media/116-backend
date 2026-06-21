using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;

/// <summary>
/// Integration tests for the PublicRevokeSession endpoint.
/// </summary>
[Collection("Database")]
public class PublicRevokeSessionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string RevokeSessionBaseUrl = $"{ApiRoutes.Public.Me}/sessions/revoke";

    [Fact]
    public async Task PublicRevokeSession_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that a Visitor can revoke their own session successfully.
    /// </summary>
    [Fact]
    public async Task RevokeSession_AsVisitor_WithOwnSession_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, TestConstants.User.VisitorId);

        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{sessionId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that trying to revoke another user's session returns 404 Not Found.
    /// The handler treats mismatched ownership as session not found for security.
    /// </summary>
    [Fact]
    public async Task RevokeSession_WithOtherUsersSession_ReturnsNotFound()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var otherUser = UserFactory.CreateVerifiedActive();

        var otherSessionId = Guid.NewGuid();
        var otherSession = SessionFactory.CreateWithId(otherSessionId, otherUser.Id);

        seedContext.Users.Add(otherUser);
        seedContext.Sessions.Add(otherSession);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{otherSessionId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that revoking a non-existent session returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task RevokeSession_WithNonExistentSession_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
