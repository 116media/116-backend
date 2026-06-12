using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;

/// <summary>
/// Integration tests for the AdminCreateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task CreateCategory_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var name = ShortName("cc");
        var slug = ShortSlug("cc");
        var request = new
        {
            Name = name,
            Slug = slug,
            Description = "Test desc",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var category = await verifyContext.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        category.Should().NotBeNull();
        category!.Name.Should().Be(name);
    }

    [Fact]
    public async Task CreateCategory_AsAdmin_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            Name = ShortName("fa"),
            Slug = ShortSlug("fa"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = ShortName("na"),
            Slug = ShortSlug("na"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var existing = CategoryFactory.Create(contentType.Id, ShortName("dup"), "dup-slug-test");
        seedContext.Categories.Add(existing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("dup2"),
            Slug = "dup-slug-test",
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "",
            Slug = ShortSlug("en"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCategory_WithEmptySlug_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("es"),
            Slug = "",
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCategory_WithInvalidContentTypeId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("ic"),
            Slug = ShortSlug("ic"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/not-a-guid", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCategory_WithNonExistentContentType_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("ne"),
            Slug = ShortSlug("ne"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
