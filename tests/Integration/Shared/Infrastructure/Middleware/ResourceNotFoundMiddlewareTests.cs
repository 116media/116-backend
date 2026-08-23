using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
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

        await response.ShouldBeProblem<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.ResourceNotFound())
        );
    }

    [Fact]
    public async Task Request_ToNonExistentAdminRoute_ShouldReturn404_WhenUnauthenticated()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Base}/this-does-not-exist");

        await response.ShouldBeProblem<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.ResourceNotFound())
        );
    }

    [Fact]
    public async Task Request_ToNonExistentRoute_ShouldNotEchoRequestPathInDetail()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Base}/leaky-path-probe");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();

        problem.Detail.Should().NotContain("leaky-path-probe");
        problem.Detail.Should().NotContain("GET");
    }
}
