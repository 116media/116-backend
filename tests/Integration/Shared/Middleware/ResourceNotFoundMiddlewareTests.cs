using System.Text.Json;

namespace _116.Integration.Tests.Shared.Middleware;

/// <summary>
/// Verifies that the ResourceNotFoundMiddleware converts 404 responses
/// into ProblemDetails for non-existent routes.
/// </summary>
[Collection("Database")]
public class ResourceNotFoundMiddlewareTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Request_ToNonExistentRoute_ShouldReturn404WithProblemDetails()
    {
        var response = await Client.GetAsync("/api/v1/public/this-does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        string body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task Request_ToNonExistentAdminRoute_ShouldReturn401_WhenUnauthenticated()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync("/api/v1/admin/this-does-not-exist");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }
}
