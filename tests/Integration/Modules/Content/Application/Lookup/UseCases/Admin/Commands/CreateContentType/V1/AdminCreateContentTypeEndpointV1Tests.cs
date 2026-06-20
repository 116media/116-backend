namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;

/// <summary>
/// Integration tests for the AdminCreateContentType endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateContentTypeEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateContentType_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "TestType" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateContentType_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "TestType" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateContentType_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateContentType_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Podcast" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
