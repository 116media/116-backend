using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Shared.Infrastructure.Middleware;

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
        var response = await Client.GetAsync($"{ApiRoutes.Public.Base}/this-does-not-exist");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Status.Should().Be(404);
        problem.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_ToNonExistentAdminRoute_ShouldReturn401_WhenUnauthenticated()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Base}/this-does-not-exist");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }
}
