using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.V1;

/// <summary>
/// Integration tests for the AdminUpdateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UpdateCategory_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("un"),
            Slug = ShortSlug("un"),
            Description = "Updated",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("nf"),
            Slug = ShortSlug("nf"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_AsAdmin_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            Name = ShortName("uf"),
            Slug = ShortSlug("uf"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = ShortName("ua"),
            Slug = ShortSlug("ua"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "",
            Slug = ShortSlug("ev"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("iv"),
            Slug = ShortSlug("iv"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/not-a-guid", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
