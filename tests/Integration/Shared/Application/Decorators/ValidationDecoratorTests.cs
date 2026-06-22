using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Shared.Application.Decorators;

/// <summary>
/// Verifies that the ValidationDecorator intercepts invalid requests
/// and returns 400 with validation error details.
/// </summary>
[Collection("Database")]
public class ValidationDecoratorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Post_WithInvalidPayload_ShouldReturn400()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithValidPayload_ShouldPassThroughToHandler()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Roles,
            new { Name = "ValidTestRole", Description = "A valid test role" }
        );

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithMultipleValidationErrors_ShouldReturn400WithErrors()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Status.Should().Be(400);

        problem.Extensions.Should().ContainKey("errors", "the validation problem should enumerate the failed fields");

        JsonElement errors = (JsonElement)problem.Extensions["errors"]!;
        string serializedErrors = errors.GetRawText();
        serializedErrors.Should().Contain("Name", "the empty Name field should be reported as a validation failure");
    }
}
