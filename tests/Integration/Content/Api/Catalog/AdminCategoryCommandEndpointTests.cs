using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Catalog;

/// <summary>
/// Integration tests for the admin category command endpoints verifying create, update,
/// activate, deactivate, set-exclusive, upload-poster, and pricing operations
/// against a real PostgreSQL database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminCategoryCommandEndpointTests(PostgresFixture db) : BaseApiTest(db)
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

    [Fact]
    public async Task ActivateCategory_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.CreateInactive(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateCategory_AsAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.CreateInactive(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateCategory_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateCategory_AsAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

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

    [Fact]
    public async Task UploadCategoryPoster_AsSuperAdmin_WithFile_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadCategoryPoster_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadCategoryPoster_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadCategoryPoster_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCategoryPricing_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = pricingTier.Id, PriceUsd = 9.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var pricing = await verifyContext.CategoryPricing.FirstOrDefaultAsync(cp =>
            cp.CategoryId == category.Id && cp.PricingTierId == pricingTier.Id
        );
        pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task AddCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = pricingTier.Id, PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategoryPricing_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 5.99m);
        seedContext.CategoryPricing.Add(categoryPricing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/pricing/{pricingTier.Id}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveCategoryPricing_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 5.99m);
        seedContext.CategoryPricing.Add(categoryPricing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/pricing/{pricingTier.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
