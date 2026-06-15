using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;

/// <summary>
/// Integration tests for the AdminCreateTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Fally Ipupa", Slug = "fally-ipupa" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTag_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "Fally Ipupa", Slug = "fally-ipupa" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "", Slug = "fally-ipupa" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Fally Ipupa", Slug = "fally-ipupa" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_DuplicateSlug_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var existingTag = TagFactory.Create("Fally Ipupa", "fally-ipupa");
        context.Tags.Add(existingTag);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Fally Ipupa Duplicate", Slug = "fally-ipupa" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
