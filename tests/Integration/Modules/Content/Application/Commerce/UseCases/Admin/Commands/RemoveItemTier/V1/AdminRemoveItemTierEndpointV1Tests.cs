using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier.V1;

/// <summary>
/// Integration tests for the AdminRemoveItemTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveItemTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemoveItemTier_AsSuperAdmin_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        ContentItemTierEntity itemTier = ContentItemTierFactory.Create(orderItem.Id, pricingTier.Id, 25.00m);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.PricingTiers.Add(pricingTier);
            ctx.ContentItemTiers.Add(itemTier);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Orders.ItemTier(order.Id, orderItem.Id, itemTier.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRemoveItemTierResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentItemTierEntity? persisted = await db.ContentItemTiers.FindAsync(itemTier.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveItemTier_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            Routes.Admin.Orders.ItemTier(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that an unknown tier identifier on an existing Draft order and an existing item
    /// returns the <c>ItemTierNotFound</c> 404 raised by <c>GetItemTierByIdOrThrowAsync</c> rather
    /// than the order lookup's own 404, so the response names the item tier.
    /// </summary>
    [Fact]
    public async Task RemoveItemTier_ExistingOrderAndItemWithUnknownTier_ReturnsNotFoundNamingTheItemTier()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsSuperAdmin();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var response = await Client.DeleteAsync(Routes.Admin.Orders.ItemTier(order.Id, orderItem.Id, Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound, "Could not find the requested item tier.");
    }

    [Fact]
    public async Task RemoveItemTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            Routes.Admin.Orders.ItemTier(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
