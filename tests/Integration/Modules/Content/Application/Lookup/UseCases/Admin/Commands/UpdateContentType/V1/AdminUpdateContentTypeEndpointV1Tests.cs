using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType.V1;

/// <summary>
/// Integration tests for the AdminUpdateContentType endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateContentTypeEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateContentType_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "UpdatedName" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateContentType_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "UpdatedName" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContentType_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "UpdatedType" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that updating a content type with a name exceeding the maximum allowed length
    /// (30 characters) returns a 400 Bad Request or 422 Unprocessable Entity response.
    /// </summary>
    [Fact]
    public async Task UpdateContentType_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = new string('X', 100) };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
