using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing.V1;

/// <summary>
/// Integration tests for the AdminUpdateCategoryPricing endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCategoryPricingEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateCategoryPricing_AsSuperAdmin_WithValidData_ReturnsOk()
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
            CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 5.99m);
            ctx.CategoryPricing.Add(categoryPricing);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCategoryPricingRequest request = new AdminUpdateCategoryPricingRequestBuilder()
            .WithPriceUsd(12.99m)
            .Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Categories.PricingItem(category.Id, pricingTier.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateCategoryPricingResponse>();
        body.Pricing.TierId.Should().Be(pricingTier.Id);
        body.Pricing.PriceUsd.Should().Be(request.PriceUsd);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryPricingEntity? updated = await verifyContext.CategoryPricing.FirstOrDefaultAsync(cp =>
            cp.CategoryId == category.Id && cp.PricingTierId == pricingTier.Id
        );
        updated.Should().NotBeNull();
        updated!.PriceUsd.Should().Be(request.PriceUsd);
    }

    [Fact]
    public async Task UpdateCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()),
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
            Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategoryPricing_NonExistentPricing_ReturnsNotFound()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCategoryPricingRequest request = new AdminUpdateCategoryPricingRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Categories.PricingItem(category.Id, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("CategoryPricing"))
        );
    }

    [Fact]
    public async Task UpdateCategoryPricing_WithUnknownCategoryId_ReturnsCategoryNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCategoryPricingRequest request = new AdminUpdateCategoryPricingRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task UpdateCategoryPricing_WithPricingBelongingToAnotherCategory_ReturnsNotFound()
    {
        CategoryEntity owningCategory = null!;
        CategoryEntity addressedCategory = null!;
        PricingTierEntity pricingTier = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            owningCategory = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(owningCategory);
            addressedCategory = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(addressedCategory);
            pricingTier = PricingTierFactory.Create();
            ctx.PricingTiers.Add(pricingTier);
            CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(
                owningCategory.Id,
                pricingTier.Id,
                5.99m
            );
            ctx.CategoryPricing.Add(categoryPricing);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCategoryPricingRequest request = new AdminUpdateCategoryPricingRequestBuilder()
            .WithPriceUsd(99.99m)
            .Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Categories.PricingItem(addressedCategory.Id, pricingTier.Id),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("CategoryPricing"))
        );

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryPricingEntity? untouched = await verifyContext.CategoryPricing.FirstOrDefaultAsync(cp =>
            cp.CategoryId == owningCategory.Id && cp.PricingTierId == pricingTier.Id
        );
        untouched.Should().NotBeNull();
        untouched!.PriceUsd.Should().Be(5.99m);
    }
}
