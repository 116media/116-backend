using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;

/// <summary>
/// Integration tests for the AdminUpdateTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Updated", Slug = "updated" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated", Slug = "updated" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated Tag", Slug = "updated-tag" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{tag.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
