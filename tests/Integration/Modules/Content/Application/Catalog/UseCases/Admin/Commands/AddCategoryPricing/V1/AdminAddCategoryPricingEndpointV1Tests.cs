using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;

/// <summary>
/// Integration tests for the AdminAddCategoryPricing endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddCategoryPricingEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddCategoryPricing_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CategoryEntity category = null!;
        PricingTierEntity pricingTier = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            pricingTier = PricingTierFactory.Create();
            ctx.PricingTiers.Add(pricingTier);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddCategoryPricingRequest request = new AdminAddCategoryPricingRequestBuilder()
            .WithPricingTierId(pricingTier.Id)
            .WithPriceUsd(9.99m)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(category.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminAddCategoryPricingResponse>();
        body.Pricing.TierId.Should().Be(pricingTier.Id);
        body.Pricing.PriceUsd.Should().Be(request.PriceUsd);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryPricingEntity? pricing = await verifyContext.CategoryPricing.FirstOrDefaultAsync(cp =>
            cp.CategoryId == category.Id && cp.PricingTierId == pricingTier.Id
        );
        pricing.Should().NotBeNull();
        pricing!.PriceUsd.Should().Be(request.PriceUsd);
    }

    [Fact]
    public async Task AddCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity tier = PricingTierFactory.Create();
            ctx.PricingTiers.Add(tier);
            return tier;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddCategoryPricingRequest request = new AdminAddCategoryPricingRequestBuilder()
            .WithPricingTierId(pricingTier.Id)
            .WithPriceUsd(4.99m)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(Guid.NewGuid()), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that adding category pricing with a pricing tier that is already
    /// associated with the category returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task AddCategoryPricing_WithDuplicateTier_ReturnsConflict()
    {
        CategoryEntity category = null!;
        PricingTierEntity pricingTier = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            pricingTier = PricingTierFactory.Create();
            ctx.PricingTiers.Add(pricingTier);
            CategoryPricingEntity existingPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 5.99m);
            ctx.CategoryPricing.Add(existingPricing);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddCategoryPricingRequest request = new AdminAddCategoryPricingRequestBuilder()
            .WithPricingTierId(pricingTier.Id)
            .WithPriceUsd(12.99m)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(category.Id), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that adding category pricing with an inactive pricing tier
    /// returns a 400 BadRequest response because the tier must be active.
    /// </summary>
    [Fact]
    public async Task AddCategoryPricing_WithInactiveTier_ReturnsBadRequest()
    {
        CategoryEntity category = null!;
        PricingTierEntity inactiveTier = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            inactiveTier = PricingTierFactory.CreateInactive();
            ctx.PricingTiers.Add(inactiveTier);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddCategoryPricingRequest request = new AdminAddCategoryPricingRequestBuilder()
            .WithPricingTierId(inactiveTier.Id)
            .WithPriceUsd(9.99m)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(category.Id), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that adding category pricing with a negative price
    /// returns a 400 BadRequest response because the price must be non-negative.
    /// </summary>
    [Fact]
    public async Task AddPricing_WithNegativePrice_ReturnsBadRequest()
    {
        CategoryEntity category = null!;
        PricingTierEntity pricingTier = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            pricingTier = PricingTierFactory.Create();
            ctx.PricingTiers.Add(pricingTier);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddCategoryPricingRequest request = new AdminAddCategoryPricingRequestBuilder()
            .WithPricingTierId(pricingTier.Id)
            .WithPriceUsd(-1m)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Categories.Pricing(category.Id), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
