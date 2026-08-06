using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;

/// <summary>
/// Integration tests for the AdminRemoveCategoryPricing endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveCategoryPricingEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemoveCategoryPricing_AsSuperAdmin_ReturnsOk()
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

        var response = await Client.DeleteAsync(Routes.Admin.Categories.PricingItem(category.Id, pricingTier.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminRemoveCategoryPricingResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Pricing.Should().NotContain(p => p.TierId == pricingTier.Id);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (
            await verifyContext.CategoryPricing.AnyAsync(cp =>
                cp.CategoryId == category.Id && cp.PricingTierId == pricingTier.Id
            )
        )
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task RemoveCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveCategoryPricing_WithUnknownCategoryId_ReturnsCategoryNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Categories.PricingItem(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task RemoveCategoryPricing_WithPricingBelongingToAnotherCategory_ReturnsNotFound()
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

        var response = await Client.DeleteAsync(
            Routes.Admin.Categories.PricingItem(addressedCategory.Id, pricingTier.Id)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("CategoryPricing"))
        );

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (
            await verifyContext.CategoryPricing.AnyAsync(cp =>
                cp.CategoryId == owningCategory.Id && cp.PricingTierId == pricingTier.Id
            )
        )
            .Should()
            .BeTrue("pricing owned by another category must not be removed");
    }

    [Fact]
    public async Task RemoveCategoryPricing_NonExistentPricing_ReturnsNotFound()
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

        var response = await Client.DeleteAsync(Routes.Admin.Categories.PricingItem(category.Id, Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("CategoryPricing"))
        );
    }
}
