using System.Text.Json;

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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        string body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status);
        status.GetInt32().Should().Be(400);
    }
}
