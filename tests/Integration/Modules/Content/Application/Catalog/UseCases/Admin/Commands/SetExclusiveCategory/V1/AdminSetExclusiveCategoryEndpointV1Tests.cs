using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.V1;

/// <summary>
/// Integration tests for the AdminSetExclusiveCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminSetExclusiveCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task SetExclusiveCategory_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/set-exclusive", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetExclusiveCategory_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/set-exclusive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetExclusiveCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/set-exclusive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetExclusiveCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/set-exclusive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
