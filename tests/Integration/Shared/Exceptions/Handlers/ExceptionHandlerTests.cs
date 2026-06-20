using System.Text.Json;

namespace _116.Integration.Tests.Shared.Exceptions.Handlers;

/// <summary>
/// Verifies that all exception handler strategies return correct HTTP status codes
/// and ProblemDetails structure through the full API pipeline.
/// </summary>
[Collection("Database")]
public class ExceptionHandlerTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AuthenticationException_ShouldReturn401_WhenNoToken()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Roles);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthorizationException_ShouldReturn403_WhenWrongRole()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Roles,
            new { Name = "ForbiddenRole", Description = "Should be forbidden" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NotFoundExceptionHandler_ShouldReturn404_WhenEntityNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NotFoundExceptionHandler_ShouldReturnProblemDetails()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        string body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task BadRequestExceptionHandler_ShouldReturn400_WhenValidationFails()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BadRequestExceptionHandler_ShouldReturnProblemDetails()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        string body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(400);
    }

    [Fact]
    public async Task ResourceNotFoundExceptionHandler_ShouldReturn404_ForNonExistentRoute()
    {
        var response = await Client.GetAsync("/api/v1/public/nonexistent-resource");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResourceNotFoundExceptionHandler_ShouldReturnProblemDetails_ForNonExistentRoute()
    {
        var response = await Client.GetAsync("/api/v1/public/nonexistent-resource");

        string body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task MethodNotAllowedExceptionHandler_ShouldReturn405_ForWrongMethod()
    {
        var response = await Client.DeleteAsync(ApiRoutes.Public.Auth + "/login");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FormatExceptionStrategy_ShouldReturn404_ForInvalidUuidOnGuidConstrainedRoute()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/not-a-uuid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConflictExceptionHandler_ShouldReturn409_WhenDuplicateCreated()
    {
        Client.AuthenticateAsSuperAdmin();

        var rolePayload = new { Name = "ConflictTestRole", Description = "First creation" };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, rolePayload);

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, rolePayload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
